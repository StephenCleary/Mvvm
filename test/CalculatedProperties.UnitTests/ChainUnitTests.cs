using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nito.Mvvm;
using Nito.Mvvm.CalculatedProperties;
using Xunit;
using System.Linq;

namespace UnitTests
{
    public class ChainUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public int Leaf
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int Branch
            {
                get { return Properties.Get(11); }
                set { Properties.Set(value); }
            }

            public int Intermediate
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++IntermediateExecutionCount;
                        return Leaf * 2;
                    });
                }
            }

            public int Root
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++RootExecutionCount;
                        return Intermediate + Branch;
                    });
                }
            }

            public int RootExecutionCount;
            public int IntermediateExecutionCount;
        }
        
        [Fact]
        public void Root_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.Equal(25, vm.Root);
            Assert.Equal(1, vm.IntermediateExecutionCount);
            Assert.Equal(1, vm.RootExecutionCount);
        }

        [Fact]
        public void LeafChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Leaf = 13;
            Assert.Equal(new[] { "Leaf", "Intermediate", "Root" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void BranchChanges_RaisesPropertyChangedForAllAffectedProperties()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Branch = 13;
            Assert.Equal(new[] { "Branch", "Root" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void LeafChanges_NotificationsDeferred_RaisesPropertyChangedForAllAffectedPropertiesAfterNotificationsResumed()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                vm.Leaf = 13;
                Assert.Equal(new string[] { }, changes);
            }
            Assert.Equal(new[] { "Leaf", "Intermediate", "Root" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void LeafAndBranchChanges_RaisesPropertyChangedForAllAffectedPropertiesImmediately()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            vm.Leaf = 13;
            vm.Branch = 13;
            Assert.Equal(new[] { "Leaf", "Intermediate", "Root", "Branch", "Root" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void LeafAndBranchChanges_NotificationsDeferred_RaisesPropertyChangedForAllAffectedPropertiesAfterNotificationsResumed_AndCombinesThem()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.Root;
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                vm.Leaf = 13;
                vm.Branch = 13;
                Assert.Equal(new string[] { }, changes);
            }
            Assert.Equal(new[] { "Leaf", "Intermediate", "Root", "Branch" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }
    }
}
