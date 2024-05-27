using GameFrameX.Serialize.Serialize;
using SuperSocket.ProtoBase;

namespace GameFrameX.Launcher.StartUp;

class BaseMessageEncoderHandler : IMessageEncoderHandler, IPackageEncoder<IMessage>
{
    /// <summary>
    /// 和客户端之间的消息 数据长度(4)+消息唯一ID(4)+消息ID(4)+消息内容
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public byte[] Handler(IMessage message)
    {
        var messageObject = message as MessageObject;
        var bytes = SerializerHelper.Serialize(message);
        // len +uniqueId + msgId + bytes.length
        int len = 4 + 8 + 4 + bytes.Length;
        var span = new byte[len];
        int offset = 0;
        span.WriteInt(len, ref offset);
        span.WriteLong(messageObject.UniqueId, ref offset);
        var messageType = message.GetType();
        var msgId = ProtoMessageIdHandler.GetRespMessageIdByType(messageType);
        messageObject.MessageId = msgId;
        span.WriteInt(msgId, ref offset);
        span.WriteBytesWithoutLength(bytes, ref offset);
        // LogHelper.Debug(message.ToSendMessageString(ServerType.Router, ServerType.Client));
        return span;
    }

    /// <summary>
    /// 和服务器之间的消息 数据长度(4)+消息唯一ID(8)+消息ID(4)+消息内容
    /// </summary>
    /// <param name="messageUniqueId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public byte[] RpcHandler(long messageUniqueId, IMessage message)
    {
        var bytes = SerializerHelper.Serialize(message);
        // len + UniqueId + msgId + bytes.length
        int len = 4 + 8 + 4 + 4 + bytes.Length;
        var span = new byte[len];
        int offset = 0;
        span.WriteInt(len, ref offset);
        span.WriteLong(messageUniqueId, ref offset);
        var messageType = message.GetType();
        var msgId = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
        span.WriteInt(msgId, ref offset);
        span.WriteBytes(bytes, ref offset);
        return span;
    }

    public int Encode(IBufferWriter<byte> writer, IMessage pack)
    {
        var bytes = Handler(pack);
        writer.Write(bytes);
        return bytes.Length;
    }
}