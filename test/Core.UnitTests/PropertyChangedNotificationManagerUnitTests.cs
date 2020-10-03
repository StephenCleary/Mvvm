using Nito.Mvvm;
using System;
using System.Threading.Tasks;
using Xunit;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;

namespace UnitTests
{
    public class PropertyChangedNotificationManagerUnitTests
    {
        [Fact]
        public void Register_NotDeferred_RaisesEvent()
        {
            var pc = new DelegatePropertyChanged();
            var name = Guid.NewGuid().ToString("N");
            PropertyChangedNotificationManager.Instance.Register(pc, name);
            Assert.Equal(new[] { name }, pc.ObservedArgs.Select(x => x.PropertyName));
        }

        [Fact]
        public void Register_Deferred_DefersEvent()
        {
            var pc = new DelegatePropertyChanged();
            var name = Guid.NewGuid().ToString("N");
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                PropertyChangedNotificationManager.Instance.Register(pc, name);
                Assert.Equal(0, pc.ObservedArgs.Count);
            }
            Assert.Equal(new[] { name }, pc.ObservedArgs.Select(x => x.PropertyName));
        }

        [Fact]
        public void DeferNotifications_IsRefCounted()
        {
            var pc = new DelegatePropertyChanged();
            var name = Guid.NewGuid().ToString("N");
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                using (PropertyChangedNotificationManager.Instance.DeferNotifications())
                    PropertyChangedNotificationManager.Instance.Register(pc, name);
                Assert.Equal(0, pc.ObservedArgs.Count);
            }
            Assert.Equal(new[] { name }, pc.ObservedArgs.Select(x => x.PropertyName));
        }

        [Fact]
        public void Register_Deferred_ConsolidatesEvents()
        {
            var pc = new DelegatePropertyChanged();
            var name = Guid.NewGuid().ToString("N");
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                PropertyChangedNotificationManager.Instance.Register(pc, name);
                PropertyChangedNotificationManager.Instance.Register(pc, name);
            }
            Assert.Equal(new[] { name }, pc.ObservedArgs.Select(x => x.PropertyName));
        }

        [Fact]
        public void Consolidation_DifferentNames_AreDifferent()
        {
            var pc = new DelegatePropertyChanged();
            var name1 = Guid.NewGuid().ToString("N");
            var name2 = Guid.NewGuid().ToString("N");
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                PropertyChangedNotificationManager.Instance.Register(pc, name1);
                PropertyChangedNotificationManager.Instance.Register(pc, name2);
                PropertyChangedNotificationManager.Instance.Register(pc, name1);
                PropertyChangedNotificationManager.Instance.Register(pc, name2);
            }
            Assert.Equal(new[] { name1, name2 }.OrderBy(x => x), pc.ObservedArgs.Select(x => x.PropertyName).OrderBy(x => x));
        }

        [Fact]
        public void Consolidation_DifferentObjects_AreDifferent()
        {
            var pc1 = new DelegatePropertyChanged();
            var pc2 = new DelegatePropertyChanged();
            var name = Guid.NewGuid().ToString("N");
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                PropertyChangedNotificationManager.Instance.Register(pc1, name);
                PropertyChangedNotificationManager.Instance.Register(pc2, name);
                PropertyChangedNotificationManager.Instance.Register(pc1, name);
                PropertyChangedNotificationManager.Instance.Register(pc2, name);
            }
            Assert.Equal(new[] { name }, pc1.ObservedArgs.Select(x => x.PropertyName));
            Assert.Equal(new[] { name }, pc2.ObservedArgs.Select(x => x.PropertyName));
        }

        [Fact]
        public void Deferral_IsPerThread()
        {
            AsyncContext.Run(async () =>
            {
                var pc = new DelegatePropertyChanged();
                var name = Guid.NewGuid().ToString("N");
                using (PropertyChangedNotificationManager.Instance.DeferNotifications())
                {
                    await Task.Run(() => PropertyChangedNotificationManager.Instance.Register(pc, name));
                    Assert.Equal(new[] { name }, pc.ObservedArgs.Select(x => x.PropertyName));
                }
            });
        }

        private sealed class DelegatePropertyChanged : IRaisePropertyChanged
        {
            public void RaisePropertyChanged(PropertyChangedEventArgs e)
            {
                ObservedArgs.Add(e);
            }

            public List<PropertyChangedEventArgs> ObservedArgs = new List<PropertyChangedEventArgs>();
        }
    }
}