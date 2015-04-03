using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.ComponentModel;
using Nito.Mvvm;
using Xunit;

namespace UnitTests
{
    public class NotifyTaskUnitTests
    {
        [Fact]
        public void Notifier_TaskCompletesSuccessfully_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => (Task)tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetResult(null);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.True(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.True(isSuccessfullyCompletedNotification());
            Assert.False(isCanceledNotification());
            Assert.False(isFaultedNotification());
            Assert.False(exceptionNotification());
            Assert.False(innerExceptionNotification());
            Assert.False(errorMessageNotification());
        }

        [Fact]
        public void Notifier_TaskCanceled_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => (Task)tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetCanceled();

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.True(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.False(isSuccessfullyCompletedNotification());
            Assert.True(isCanceledNotification());
            Assert.False(isFaultedNotification());
            Assert.False(exceptionNotification());
            Assert.False(innerExceptionNotification());
            Assert.False(errorMessageNotification());
        }

        [Fact]
        public void Notifier_TaskFaulted_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => (Task)tcs.Task);
            var exception = new NotImplementedException(Guid.NewGuid().ToString("N"));
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetException(exception);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.True(notifier.IsFaulted);
            Assert.NotNull(notifier.Exception);
            Assert.Same(exception, notifier.InnerException);
            Assert.Equal(exception.Message, notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.False(isSuccessfullyCompletedNotification());
            Assert.False(isCanceledNotification());
            Assert.True(isFaultedNotification());
            Assert.True(exceptionNotification());
            Assert.True(innerExceptionNotification());
            Assert.True(errorMessageNotification());
        }

        [Fact]
        public void NotifierT_TaskCompletesSuccessfully_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => tcs.Task);
            var result = new object();
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.Null(notifier.Result);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetResult(result);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.True(notifier.IsSuccessfullyCompleted);
            Assert.Same(result, notifier.Result);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.True(isSuccessfullyCompletedNotification());
            Assert.False(isCanceledNotification());
            Assert.False(isFaultedNotification());
            Assert.False(exceptionNotification());
            Assert.False(innerExceptionNotification());
            Assert.False(errorMessageNotification());
            Assert.True(resultNotification());
        }

        [Fact]
        public void NotifierT_TaskCanceled_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => tcs.Task);
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.Null(notifier.Result);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetCanceled();

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.Null(notifier.Result);
            Assert.True(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.False(isSuccessfullyCompletedNotification());
            Assert.True(isCanceledNotification());
            Assert.False(isFaultedNotification());
            Assert.False(exceptionNotification());
            Assert.False(innerExceptionNotification());
            Assert.False(errorMessageNotification());
            Assert.False(resultNotification());
        }

        [Fact]
        public void NotifierT_TaskFaulted_NotifiesProperties()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => tcs.Task);
            var exception = new NotImplementedException(Guid.NewGuid().ToString("N"));
            var taskNotification = PropertyNotified(notifier, n => n.Task);
            var statusNotification = PropertyNotified(notifier, n => n.Status);
            var isCompletedNotification = PropertyNotified(notifier, n => n.IsCompleted);
            var isSuccessfullyCompletedNotification = PropertyNotified(notifier, n => n.IsSuccessfullyCompleted);
            var isCanceledNotification = PropertyNotified(notifier, n => n.IsCanceled);
            var isFaultedNotification = PropertyNotified(notifier, n => n.IsFaulted);
            var exceptionNotification = PropertyNotified(notifier, n => n.Exception);
            var innerExceptionNotification = PropertyNotified(notifier, n => n.InnerException);
            var errorMessageNotification = PropertyNotified(notifier, n => n.ErrorMessage);
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.False(notifier.TaskCompleted.IsCompleted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.False(notifier.IsCompleted);
            Assert.True(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.Null(notifier.Result);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);

            tcs.SetException(exception);

            Assert.Same(tcs.Task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(tcs.Task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.Null(notifier.Result);
            Assert.False(notifier.IsCanceled);
            Assert.True(notifier.IsFaulted);
            Assert.NotNull(notifier.Exception);
            Assert.Same(exception, notifier.InnerException);
            Assert.Equal(exception.Message, notifier.ErrorMessage);

            Assert.False(taskNotification());
            Assert.True(statusNotification());
            Assert.True(isCompletedNotification());
            Assert.False(isSuccessfullyCompletedNotification());
            Assert.False(isCanceledNotification());
            Assert.True(isFaultedNotification());
            Assert.True(exceptionNotification());
            Assert.True(innerExceptionNotification());
            Assert.True(errorMessageNotification());
            Assert.False(resultNotification());
        }

        [Fact]
        public void PropertyChanged_NoListeners_DoesNotThrow()
        {
            var tcs = new TaskCompletionSource<object>();
            var notifier = NotifyTask.Create(() => (Task)tcs.Task);
            tcs.SetResult(null);
        }

        [Fact]
        public void NotifierT_NonDefaultResult_TaskCompletesSuccessfully_UpdatesResult()
        {
            var tcs = new TaskCompletionSource<object>();
            var defaultResult = new object();
            var notifier = NotifyTask.Create(() => tcs.Task, defaultResult);
            var result = new object();
            var resultNotification = PropertyNotified(notifier, n => n.Result);

            Assert.Same(defaultResult, notifier.Result);

            tcs.SetResult(result);

            Assert.Same(result, notifier.Result);
            Assert.True(resultNotification());
        }

        [Fact]
        public void NotifierT_NonDefaultResult_TaskCanceled_KeepsNonDefaultResult()
        {
            var tcs = new TaskCompletionSource<object>();
            var defaultResult = new object();
            var notifier = NotifyTask.Create(() => tcs.Task, defaultResult);

            Assert.Same(defaultResult, notifier.Result);

            tcs.SetCanceled();

            Assert.Same(defaultResult, notifier.Result);
        }

        [Fact]
        public void NotifierT_NonDefaultResult_TaskFaulted_KeepsNonDefaultResult()
        {
            var tcs = new TaskCompletionSource<object>();
            var defaultResult = new object();
            var notifier = NotifyTask.Create(() => tcs.Task, defaultResult);
            var exception = new NotImplementedException(Guid.NewGuid().ToString("N"));

            Assert.Same(defaultResult, notifier.Result);

            tcs.SetException(exception);

            Assert.Same(defaultResult, notifier.Result);
        }

        [Fact]
        public void Notifier_SynchronousTaskCompletedSuccessfully_SetsProperties()
        {
            var task = Task.CompletedTask;
            var notifier = NotifyTask.Create(() => task);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.True(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);
        }

        [Fact]
        public void Notifier_SynchronousTaskCanceled_SetsProperties()
        {
            var task = Task.FromCanceled(new CancellationToken(true));
            var notifier = NotifyTask.Create(() => task);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.True(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);
        }

        [Fact]
        public void Notifier_SynchronousTaskFaulted_SetsProperties()
        {
            var exception = new Exception(Guid.NewGuid().ToString("N"));
            var task = Task.FromException(exception);
            var notifier = NotifyTask.Create(() => task);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.True(notifier.IsFaulted);
            Assert.NotNull(notifier.Exception);
            Assert.Same(exception, notifier.InnerException);
            Assert.Equal(exception.Message, notifier.ErrorMessage);
        }

        [Fact]
        public void NotifierT_SynchronousTaskCompletedSuccessfully_SetsProperties()
        {
            var task = Task.FromResult(13);
            var notifier = NotifyTask.Create(() => task, 17);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(13, notifier.Result);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.True(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);
        }

        [Fact]
        public void NotifierT_SynchronousTaskCanceled_SetsProperties()
        {
            var task = Task.FromCanceled<int>(new CancellationToken(true));
            var notifier = NotifyTask.Create(() => task, 17);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(17, notifier.Result);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.True(notifier.IsCanceled);
            Assert.False(notifier.IsFaulted);
            Assert.Null(notifier.Exception);
            Assert.Null(notifier.InnerException);
            Assert.Null(notifier.ErrorMessage);
        }

        [Fact]
        public void NotifierT_SynchronousTaskFaulted_SetsProperties()
        {
            var exception = new Exception(Guid.NewGuid().ToString("N"));
            var task = Task.FromException<int>(exception);
            var notifier = NotifyTask.Create(() => task, 17);

            Assert.Same(task, notifier.Task);
            Assert.True(notifier.TaskCompleted.IsCompleted && !notifier.TaskCompleted.IsCanceled && !notifier.TaskCompleted.IsFaulted);
            Assert.Equal(17, notifier.Result);
            Assert.Equal(task.Status, notifier.Status);
            Assert.True(notifier.IsCompleted);
            Assert.False(notifier.IsNotCompleted);
            Assert.False(notifier.IsSuccessfullyCompleted);
            Assert.False(notifier.IsCanceled);
            Assert.True(notifier.IsFaulted);
            Assert.NotNull(notifier.Exception);
            Assert.Same(exception, notifier.InnerException);
            Assert.Equal(exception.Message, notifier.ErrorMessage);
        }

        private static Func<bool> PropertyNotified<T, P>(T obj, Expression<Func<T, P>> prop) where T : INotifyPropertyChanged
        {
            var expression = (MemberExpression)prop.Body;
            string name = expression.Member.Name;

            bool invoked = false;
            obj.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == name)
                    invoked = true;
            };

            return () => invoked;
        }
    }
}
