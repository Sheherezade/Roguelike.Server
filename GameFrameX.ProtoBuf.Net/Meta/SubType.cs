﻿#if !NO_RUNTIME
using System.Collections;
using ProtoBuf.Serializers;

namespace ProtoBuf.Meta;

/// <summary>
/// Represents an inherited type in a type hierarchy.
/// </summary>
public sealed class SubType
{
    private readonly DataFormat dataFormat;

    private int _fieldNumber;

    private IProtoSerializer serializer;

    /// <summary>
    /// Creates a new SubType instance.
    /// </summary>
    /// <param name="fieldNumber">
    /// The field-number that is used to encapsulate the data (as a nested
    /// message) for the derived dype.
    /// </param>
    /// <param name="derivedType">The sub-type to be considered.</param>
    /// <param name="format">Specific encoding style to use; in particular, Grouped can be used to avoid buffering, but is not the default.</param>
    public SubType(int fieldNumber, MetaType derivedType, DataFormat format)
    {
        if (derivedType == null)
        {
            throw new ArgumentNullException(nameof(derivedType));
        }

        if (fieldNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldNumber));
        }

        _fieldNumber = fieldNumber;
        this.DerivedType = derivedType;
        dataFormat = format;
    }

    /// <summary>
    /// The field-number that is used to encapsulate the data (as a nested
    /// message) for the derived dype.
    /// </summary>
    public int FieldNumber
    {
        get { return _fieldNumber; }
        internal set
        {
            if (_fieldNumber != value)
            {
                MetaType.AssertValidFieldNumber(value);
                ThrowIfFrozen();
                _fieldNumber = value;
            }
        }
    }


    /// <summary>
    /// The sub-type to be considered.
    /// </summary>
    public MetaType DerivedType { get; }

    internal IProtoSerializer Serializer
    {
        get { return serializer ?? (serializer = BuildSerializer()); }
    }

    private void ThrowIfFrozen()
    {
        if (serializer != null)
        {
            throw new InvalidOperationException("The type cannot be changed once a serializer has been generated");
        }
    }

    private IProtoSerializer BuildSerializer()
    {
        // note the caller here is MetaType.BuildSerializer, which already has the sync-lock
        var wireType = WireType.String;
        if (dataFormat == DataFormat.Group)
        {
            wireType = WireType.StartGroup; // only one exception
        }

        IProtoSerializer ser = new SubItemSerializer(DerivedType.Type, DerivedType.GetKey(false, false), DerivedType, false);
        return new TagDecorator(_fieldNumber, wireType, false, ser);
    }

    internal sealed class Comparer : IComparer, IComparer<SubType>
    {
        public static readonly Comparer Default = new();

        public int Compare(object x, object y)
        {
            return Compare(x as SubType, y as SubType);
        }

        public int Compare(SubType x, SubType y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return x.FieldNumber.CompareTo(y.FieldNumber);
        }
    }
}
#endif