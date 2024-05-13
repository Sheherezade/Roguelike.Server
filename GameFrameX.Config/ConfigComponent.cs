﻿using System.Text.Json;
using GameFrameX.Config.Core;
using GameFrameX.Log;

namespace GameFrameX.Config;

public class ConfigComponent
{
    public static ConfigComponent Instance { get; } = new ConfigComponent();
    private ConfigManager m_ConfigManager;

    public ConfigComponent()
    {
        m_ConfigManager = new ConfigManager();
    }

    public TablesComponent Tables { get; private set; }

    public async void LoadConfig()
    {
        LogHelper.Info($"Load Config Start...");
        Tables = new TablesComponent();
        Tables.Init(Instance);
        await Tables.LoadAsync(Loader);
        LogHelper.Info($"Load Config End...");
        LogHelper.Info("== load success ==");
    }

    private static async Task<JsonElement> Loader(string file)
    {
        var configJson = await File.ReadAllTextAsync($"json/{file}.json");
        JsonElement jsonElement = JsonDocument.Parse(configJson).RootElement;
        return jsonElement;
    }

    /// <summary>
    /// 获取指定全局配置项。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetConfig<T>() where T : IDataTable
    {
        if (HasConfig<T>())
        {
            var configName = typeof(T).Name;
            var config = m_ConfigManager.GetConfig(configName);
            if (config != null)
            {
                return (T)config;
            }
        }

        return default;
    }

    /// <summary>
    /// 检查是否存在指定全局配置项。
    /// </summary>
    /// <returns>指定的全局配置项是否存在。</returns>
    public bool HasConfig<T>() where T : IDataTable
    {
        var configName = typeof(T).Name;
        return m_ConfigManager.HasConfig(configName);
    }

    /// <summary>
    /// 移除指定全局配置项。
    /// </summary>
    /// <returns>是否移除全局配置项成功。</returns>
    public bool RemoveConfig<T>() where T : IDataTable
    {
        var configName = typeof(T).Name;
        return m_ConfigManager.RemoveConfig(configName);
    }

    /// <summary>
    /// 清空所有全局配置项。
    /// </summary>
    public void RemoveAllConfigs()
    {
        m_ConfigManager.RemoveAllConfigs();
    }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="dataTable"></param>
    public void Add(string configName, IDataTable dataTable)
    {
        m_ConfigManager.AddConfig(configName, dataTable);
    }
}