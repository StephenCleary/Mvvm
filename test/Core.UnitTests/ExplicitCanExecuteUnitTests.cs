using Nito.Mvvm;
using System;
using Xunit;

namespace UnitTests
{
    public class ExplicitCanExecuteUnitTests
    {
        [Fact]
        public void CanExecute_DefaultValue_IsFalse()
        {
            var command = new ExplicitCanExecute(new object());
            Assert.False(command.CanExecute);
            Assert.False(((ICanExecute)command).CanExecute(null));
        }

        [Fact]
        public void CanExecute_SetToTrue_RaisesNotification()
        {
            var sender = new object();
            object observedSender = null;
            var command = new ExplicitCanExecute(sender);
            bool observedValue = false;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedValue = ((ICanExecute)command).CanExecute(null);
            };

            ((ICanExecute)command).CanExecuteChanged += subscription;
            command.CanExecute = true;

            Assert.True(command.CanExecute);
            Assert.True(((ICanExecute)command).CanExecute(null));
            Assert.True(observedValue);
            Assert.Same(sender, observedSender);

            GC.KeepAlive(subscription);
        }

        [Fact]
        public void CanExecute_TrueSetToFalse_RaisesNotification()
        {
            var sender = new object();
            object observedSender = null;
            var command = new ExplicitCanExecute(sender);
            command.CanExecute = true;
            bool observedValue = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedValue = ((ICanExecute)command).CanExecute(null);
            };

            ((ICanExecute)command).CanExecuteChanged += subscription;
            command.CanExecute = false;

            Assert.False(command.CanExecute);
            Assert.False(((ICanExecute)command).CanExecute(null));
            Assert.False(observedValue);
            Assert.Same(sender, observedSender);

            GC.KeepAlive(subscription);
        }

        [Fact]
        public void CanExecute_FalseSetToFalse_DoesNotRaiseNotification()
        {
            var sender = new object();
            object observedSender = null;
            var command = new ExplicitCanExecute(sender);
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
            };

            ((ICanExecute)command).CanExecuteChanged += subscription;
            command.CanExecute = false;

            Assert.False(command.CanExecute);
            Assert.False(((ICanExecute)command).CanExecute(null));
            Assert.Null(observedSender);

            GC.KeepAlive(subscription);
        }

        [Fact]
        public void CanExecuteChanged_Unsubscribed_IsNotNotified()
        {
            var sender = new object();
            object observedSender = null;
            var command = new ExplicitCanExecute(sender);
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
            };

            ((ICanExecute)command).CanExecuteChanged += subscription;
            ((ICanExecute)command).CanExecuteChanged -= subscription;
            command.CanExecute = true;

            Assert.True(command.CanExecute);
            Assert.True(((ICanExecute)command).CanExecute(null));
            Assert.Null(observedSender);

            GC.KeepAlive(subscription);
        }

        [Fact]
        public void CanExecuteChanged_IsWeakEvent()
        {
            object weakObservedSender = null;
            object strongObservedSender = null;
            var (command, sender, strongSubscription) = Create();

            GC.Collect();
            command.CanExecute = true;

            Assert.True(command.CanExecute);
            Assert.True(((ICanExecute)command).CanExecute(null));
            Assert.Null(weakObservedSender);
            Assert.Same(sender, strongObservedSender);

            GC.KeepAlive(strongSubscription);

            (ExplicitCanExecute, object, EventHandler) Create()
            {
                var sender = new object();
                var command = new ExplicitCanExecute(sender);
                EventHandler weakSubscription = (s, _) =>
                {
                    weakObservedSender = s;
                };
                EventHandler strongSubscription = (s, _) =>
                {
                    strongObservedSender = s;
                };

                ((ICanExecute)command).CanExecuteChanged += weakSubscription;
                ((ICanExecute)command).CanExecuteChanged += strongSubscription;

                return (command, sender, strongSubscription);
            }
        }

        [Fact]
        public void CanExecuteChanged_UnsubscribeNonexistentEvent_DoesNothing()
        {
            object weakObservedSender = null;
            object strongObservedSender = null;
            object unsubscribedObservedSender = null;
            var (command, strongSubscription, unsubscribedSubscription, sender) = Create();

            GC.Collect();
            ((ICanExecute)command).CanExecuteChanged -= unsubscribedSubscription;
            command.CanExecute = true;

            Assert.True(command.CanExecute);
            Assert.True(((ICanExecute)command).CanExecute(null));
            Assert.Null(weakObservedSender);
            Assert.Same(sender, strongObservedSender);
            Assert.Null(unsubscribedObservedSender);

            GC.KeepAlive(strongSubscription);
            GC.KeepAlive(unsubscribedSubscription);

            (ExplicitCanExecute, EventHandler, EventHandler, object) Create()
            {
                var sender = new object();
                var command = new ExplicitCanExecute(sender);
                EventHandler weakSubscription = (s, _) =>
                {
                    weakObservedSender = s;
                };
                EventHandler strongSubscription = (s, _) =>
                {
                    strongObservedSender = s;
                };
                EventHandler unsubscribedSubscription = (s, _) =>
                {
                    unsubscribedObservedSender = s;
                };

                ((ICanExecute)command).CanExecuteChanged += strongSubscription;
                ((ICanExecute)command).CanExecuteChanged += weakSubscription;
                return (command, strongSubscription, unsubscribedSubscription, sender);
            }
        }
    }
}