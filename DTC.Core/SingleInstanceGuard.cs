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
    private bool m_isDisposed;

    private SingleInstanceGuard(Mutex mutex) =>
        m_mutex = mutex ?? throw new ArgumentNullException(nameof(mutex));

    /// <summary>
    /// Attempts to acquire the single-instance mutex for the supplied application name.
    /// </summary>
    /// <param name="applicationName">The stable application name used to create the mutex.</param>
    /// <returns>A guard that owns the mutex, or <c>null</c> when another process already owns it.</returns>
    public static SingleInstanceGuard TryAcquire(string applicationName)
    {
        var mutexName = CreateMutexName(applicationName);
        var mutex = new Mutex(true, mutexName, out var isFirstInstance);
        if (isFirstInstance)
            return new SingleInstanceGuard(mutex);

        mutex.Dispose();
        return null;
    }

    /// <summary>
    /// Releases the single-instance mutex.
    /// </summary>
    public void Dispose()
    {
        if (m_isDisposed)
            return;

        m_isDisposed = true;
        m_mutex.ReleaseMutex();
        m_mutex.Dispose();
    }

    private static string CreateMutexName(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name is required.", nameof(applicationName));

        var safeName = new string(applicationName
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray());

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $@"Local\{safeName}"
            : safeName;
    }
}
