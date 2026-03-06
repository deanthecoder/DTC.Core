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
using System.Threading.Tasks;

namespace DTC.Core.Commands;

/// <summary>
/// Wraps an asynchronous action as an <see cref="System.Windows.Input.ICommand"/>.
/// </summary>
/// <remarks>
/// Useful in MVVM UIs when a button or menu action needs to run asynchronous work (for example I/O,
/// service calls, or long-running workflows) while still participating in command enable/disable logic.
/// By default, concurrent execution is blocked while a prior execution is in flight so users cannot
/// accidentally trigger the same action repeatedly.
/// </remarks>
public sealed class AsyncRelayCommand : CommandBase
{
    private readonly Func<object, Task> m_executeAsync;
    private readonly Func<object, bool> m_canExecute;
    private readonly Action<Exception> m_onException;
    private readonly bool m_allowConcurrentExecutions;
    private bool m_isExecuting;

    public AsyncRelayCommand(
        Func<object, Task> executeAsync,
        Func<object, bool> canExecute = null,
        Action<Exception> onException = null,
        bool allowConcurrentExecutions = false)
    {
        m_executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        m_canExecute = canExecute;
        m_onException = onException;
        m_allowConcurrentExecutions = allowConcurrentExecutions;
    }

    public override bool CanExecute(object parameter) =>
        (m_allowConcurrentExecutions || !m_isExecuting) &&
        m_canExecute?.Invoke(parameter) != false;

    public override async void Execute(object parameter)
    {
        if (!CanExecute(parameter))
            return;

        if (!m_allowConcurrentExecutions)
        {
            m_isExecuting = true;
            RaiseCanExecuteChanged();
        }

        try
        {
            await m_executeAsync(parameter);
        }
        catch (Exception exception)
        {
            if (m_onException == null)
                throw;

            m_onException(exception);
        }
        finally
        {
            if (!m_allowConcurrentExecutions)
            {
                m_isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }
}
