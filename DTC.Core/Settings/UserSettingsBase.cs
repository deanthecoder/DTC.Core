// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DTC.Core.Extensions;
using DTC.Core.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DTC.Core.Settings;

/// <summary>
/// Persistent application user settings.
/// </summary>
/// <remarks>
/// Implementers add their required properties, each one calling Get()/Set() as appropriate.
/// Also implement ApplyDefaults() to set any property defaults (that differ from their type's default).
/// Disposing will automatically save settings in a user- and platform-specific location.
/// Setting a property will automatically raise a property change event.
/// </remarks>
public abstract class UserSettingsBase : INotifyPropertyChanged, IDisposable
{
    private readonly FileInfo m_filePath = Assembly.GetEntryAssembly().GetAppSettingsPath().GetFile("settings.json");
    private readonly Dictionary<string, object> m_state = new Dictionary<string, object>();

    public event PropertyChangedEventHandler PropertyChanged;

    protected abstract void ApplyDefaults();

    protected T Get<T>([CallerMemberName] string key = null)
    {
        if (!m_state.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out var value))
            value = default(T);
        if (typeof(T) == typeof(FileInfo))
        {
            if (value is string s)
            {
                value = s.ToFile();
                m_state[key] = value;
            }
        }
        else if (typeof(T) == typeof(DirectoryInfo))
        {
            if (value is string s)
            {
                value = s.ToDir();
                m_state[key] = value;
            }
        }
        else if (typeof(T) == typeof(byte[]))
        {
            // Convert value from base64 string
            if (value is string s)
            {
                value = Convert.FromBase64String(s);
                m_state[key] = value;
            }
        }
        else if (value is JToken token)
        {
            value = token.ToObject<T>();
            m_state[key] = value;
        }

        return (T)value;
    }

    protected void Set(object value, [CallerMemberName] string key = null)
    {
        if (m_state.TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out var oldValue) && Equals(oldValue, value))
            return;
        m_state[key] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    protected UserSettingsBase()
    {
        ApplyDefaults();
        if (!m_filePath.Exists)
            return;
        
        // Restore state from JSON.
        JsonConvert.PopulateObject(m_filePath.ReadAllText(), m_state, CreateSerializerSettings());
            
        // Remove any surplus keys from the JSON.
        var knownProperties =
            GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(o => o.Name);
        var surplusJsonKeys = m_state.Keys.Except(knownProperties).ToArray();
        foreach (var surplusJsonKey in surplusJsonKeys)
            m_state.Remove(surplusJsonKey);
    }

    public void Dispose() => Save();

    public void Save()
    {
        try
        {
            m_filePath.WriteAllText(JsonConvert.SerializeObject(m_state, Formatting.Indented, CreateSerializerSettings()));
        }
        catch (Exception e)
        {
            Logger.Instance.Exception($"Failed to serialize app settings to '{m_filePath}'.", e);
            throw;
        }
    }

    private static JsonSerializerSettings CreateSerializerSettings() =>
        new JsonSerializerSettings
        {
            Converters =
            [
                new FileInfoConverter(),
                new DirectoryInfoConverter(),
                new StringEnumConverter()
            ]
        };
}