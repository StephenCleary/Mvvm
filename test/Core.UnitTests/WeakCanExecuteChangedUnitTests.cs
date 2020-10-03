using Nito.Mvvm;
using System;
using Xunit;

namespace UnitTests
{
    public class WeakCanExecuteChangedUnitTests
    {
        [Fact]
        public void CanExecuteChanged_Unsubscribed_IsNotNotified()
        {
            var sender = new object();
            object observedSender = null;
            var command = new WeakCanExecuteChanged(sender);
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
            };

            command.CanExecuteChanged += subscription;
            command.CanExecuteChanged -= subscription;
            command.OnCanExecuteChanged();

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
            command.OnCanExecuteChanged();

            Assert.Null(weakObservedSender);
            Assert.Same(sender, strongObservedSender);

            GC.KeepAlive(strongSubscription);

            (WeakCanExecuteChanged, object, EventHandler) Create()
            {
                var sender = new object();
                var command = new WeakCanExecuteChanged(sender);
                EventHandler weakSubscription = (s, _) =>
                {
                    weakObservedSender = s;
                };
                EventHandler strongSubscription = (s, _) =>
                {
                    strongObservedSender = s;
                };

                command.CanExecuteChanged += weakSubscription;
                command.CanExecuteChanged += strongSubscription;

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
            command.CanExecuteChanged -= unsubscribedSubscription;
            command.OnCanExecuteChanged();

            Assert.Null(weakObservedSender);
            Assert.Same(sender, strongObservedSender);
            Assert.Null(unsubscribedObservedSender);

            GC.KeepAlive(strongSubscription);
            GC.KeepAlive(unsubscribedSubscription);

            (WeakCanExecuteChanged, EventHandler, EventHandler, object) Create()
            {
                var sender = new object();
                var command = new WeakCanExecuteChanged(sender);
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

                command.CanExecuteChanged += strongSubscription;
                command.CanExecuteChanged += weakSubscription;

                return (command, strongSubscription, unsubscribedSubscription, sender);
            }
        }
    }
}