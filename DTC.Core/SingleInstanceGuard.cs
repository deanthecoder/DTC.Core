// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DTC.Core;

/// <summary>
/// Holds a process-wide named mutex so an application can detect and reject duplicate launches.
/// </summary>
/// <remarks>
/// Keep the returned guard alive for as long as the application should be considered running.
/// Disposing the guard releases the mutex and allows a later process to become the single instance.
/// </remarks>
public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex m_mutex;
    private readonly FileStream m_lockFile;
    private bool m_isDisposed;

    private SingleInstanceGuard(Mutex mutex, FileStream lockFile)
    {
        m_mutex = mutex ?? throw new ArgumentNullException(nameof(mutex));
        m_lockFile = lockFile;
    }

    /// <summary>
    /// Attempts to acquire the single-instance mutex for the supplied application name.
    /// </summary>
    /// <param name="applicationName">The stable application name used to create the mutex.</param>
    /// <returns>A guard that owns the mutex, or <c>null</c> when another process already owns it.</returns>
    public static SingleInstanceGuard TryAcquire(string applicationName)
    {
        var safeName = CreateSafeName(applicationName);
        var mutexName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $@"Local\{safeName}" : safeName;
        var mutex = new Mutex(true, mutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            mutex.Dispose();
            return null;
        }

        var lockFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : TryAcquireFileLock(safeName);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && lockFile == null)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
            return null;
        }
        return new SingleInstanceGuard(mutex, lockFile);
    }

    /// <summary>
    /// Releases the single-instance mutex.
    /// </summary>
    public void Dispose()
    {
        if (m_isDisposed)
            return;

        m_isDisposed = true;
        if (m_lockFile != null)
            m_lockFile.Dispose();
        m_mutex.ReleaseMutex();
        m_mutex.Dispose();
    }

    private static FileStream TryAcquireFileLock(string safeName)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DTC.Core",
            "SingleInstance");
        Directory.CreateDirectory(folder);
        try
        {
            return new FileStream(
                Path.Combine(folder, safeName + ".lock"),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static string CreateSafeName(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name is required.", nameof(applicationName));

        var safeName = new string(applicationName
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray());

        return safeName;
    }
}
