﻿using System.Diagnostics;
using System.Reflection;
using TUnit.Core.Exceptions;
using TUnit.Engine.Extensions;
using TimeoutException = TUnit.Core.Exceptions.TimeoutException;

namespace TUnit.Engine;

public static class RunHelpers
{
    public static Task RunAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }
    
    public static async Task RunAsync(Func<Task> action)
    {
        await action();
    }
    
    public static async Task RunAsync(Func<ValueTask> action)
    {
        await action();
    }

    internal static async Task RunWithTimeoutAsync(Func<CancellationToken, Task> taskDelegate, TimeSpan? timeout)
    {
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(EngineCancellationToken.Token);
        
        if (timeout != null)
        {
            cancellationTokenSource.CancelAfter(timeout.Value);
        }

        var cancellationToken = cancellationTokenSource.Token;
        
        var taskCompletionSource = new TaskCompletionSource();

        var task = taskDelegate(cancellationToken);

        _ = task.ContinueWith(async t =>
        {
            try
            {
                await t;
                taskCompletionSource.TrySetResult();
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        }, CancellationToken.None);

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                if (EngineCancellationToken.Token.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetException(new TestRunCanceledException());
                    return;
                }

                if (timeout.HasValue)
                {
                    taskCompletionSource.TrySetException(new TimeoutException(timeout.Value));
                }
                else
                {
                    taskCompletionSource.TrySetCanceled();
                }
            });
        }

        await taskCompletionSource.Task;
    }

    public static Task RunSafelyAsync(Action action, List<Exception> exceptions)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }
        
        return Task.CompletedTask;
    }
    
    public static async Task RunSafelyAsync(Func<Task> action, List<Exception> exceptions)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }
    }
    
    public static async Task RunSafelyAsync(Func<ValueTask> action, List<Exception> exceptions)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }
    }
    
    public static ValueTask Dispose(object? obj)
    {
        if (obj is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }

        if (obj is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return ValueTask.CompletedTask;
    }
}