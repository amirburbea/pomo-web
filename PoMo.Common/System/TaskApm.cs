using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoMo.Common.System
{
    public static class TaskApm
    {
        public static void EndInvoke(IAsyncResult asyncResult)
        {
            TaskApm.EndInvoke<object>(asyncResult);
        }

        public static TResult EndInvoke<TResult>(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }
            Task<TResult> task = asyncResult as Task<TResult>;
            if (task == null)
            {
                throw new ArgumentException("Can not call EndInvoke on IAsyncResult that was not generated using the ToApm methods.", nameof(asyncResult));
            }
            using (task)
            {
                try
                {
                    return task.Result;
                }
                catch (AggregateException aggregate)
                {
                    throw aggregate.InnerException;
                }
            }
        }

        public static IAsyncResult ToApm(this Task task, AsyncCallback callback, object state)
        {
            return TaskApm.ToApm(task, callback, state, delegate { return default(object); });
        }

        public static IAsyncResult ToApm<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            return TaskApm.ToApm(task, callback, state, apmTask => apmTask.Result);
        }

        private static IAsyncResult ToApm<TTask, TResult>(TTask task, AsyncCallback callback, object state, Func<TTask, TResult> resultFunction)
            where TTask : Task
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            if (typeof(TTask) == typeof(Task<TResult>) && object.Equals(task.AsyncState, state))
            {
                if (callback != null)
                {
                    task.ContinueWith(callback.Invoke, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                }
                return task;
            }
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>(state);
            task.ContinueWith(
                (asyncTask, arg) =>
                {
                    TaskCompletionSource<TResult> tcs = (TaskCompletionSource<TResult>)arg;
                    if (asyncTask.IsFaulted && asyncTask.Exception != null)
                    {
                        tcs.TrySetException(asyncTask.Exception.InnerExceptions);
                    }
                    else if (asyncTask.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(resultFunction((TTask)asyncTask));
                    }
                    callback?.Invoke(taskCompletionSource.Task);
                },
                taskCompletionSource,
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default
            );
            return taskCompletionSource.Task;
        }
    }
}