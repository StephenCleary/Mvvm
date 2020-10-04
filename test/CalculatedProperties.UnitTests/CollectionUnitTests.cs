using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace UnitTests
{
    public class CollectionUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public ObservableCollection<int> Leaf
            {
                get { return Properties.Get(new ObservableCollection<int>()); }
                set { Properties.Set(value); }
            }

            public int FirstOr13
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        if (Leaf.Count == 0)
                            return 13;
                        return Leaf.First();
                    });
                }
            }

            public ObservableCollection<int> Intermediate
            {
                get { return Properties.Calculated(() => new ObservableCollection<int>(Leaf)); }
            }

            public int Calculated
            {
                get { return Properties.Calculated(() => Intermediate.Count); }
            }

            public ObservableCollection<ChildViewModel> Children
            {
                get { return Properties.Get(new ObservableCollection<ChildViewModel>()); }
                set { Properties.Set(value); }
            }

            public string FirstChildName
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        if (Children.Count == 0)
                            return null;
                        return Children[0].Name;
                    });
                }
            }

            public sealed class ChildViewModel : INotifyPropertyChanged
            {
                private string _name;

                public string Name
                {
                    get { return _name; }
                    set { _name = value; OnPropertyChanged(); }
                }

                public event PropertyChangedEventHandler PropertyChanged;

                private void OnPropertyChanged([CallerMemberName] string propertyName = null)
                {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        
        [Fact]
        public void TriggerCollection_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.Equal(13, vm.FirstOr13);
        }

        [Fact]
        public void TriggerCollectionUpdated_ValueIsRecalculated()
        {
            var vm = new ViewModel();
            var value = vm.FirstOr13;
            vm.Leaf.Add(7);
            Assert.Equal(7, vm.FirstOr13);
        }

        [Fact]
        public void TriggerCollectionReplaced_NewCollectionIsSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(7);
            Assert.Equal(7, vm.FirstOr13);
            var newValue = new ObservableCollection<int>();
            vm.Leaf = newValue;
            Assert.Equal(13, vm.FirstOr13);
            newValue.Add(11);
            Assert.Equal(11, vm.FirstOr13);
        }

        [Fact]
        public void TriggerCollectionReplaced_OldCollectionIsNotSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(7);
            Assert.Equal(7, vm.FirstOr13);
            var oldValue = vm.Leaf;
            vm.Leaf = new ObservableCollection<int>();
            Assert.Equal(13, vm.FirstOr13);

            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            oldValue.Add(11);
            Assert.Equal(13, vm.FirstOr13);
            Assert.Equal(new string[] { }, changes);
        }

        [Fact]
        public void CalculatedCollectionUpdated_DependenciesAreRecalculated()
        {
            var vm = new ViewModel();
            Assert.Equal(0, vm.Calculated);
            vm.Intermediate.Add(13);
            Assert.Equal(1, vm.Calculated);
        }

        [Fact]
        public void TriggerCollectionUpdated_CalculatedCollectionIsRecalculated()
        {
            var vm = new ViewModel();
            Assert.Equal(0, vm.Calculated);
            vm.Intermediate.Add(13);
            vm.Leaf.Add(11);
            Assert.Equal(1, vm.Calculated);
        }

        [Fact]
        public void TriggerCollectionUpdated_NewCollectionIsSubscribedTo()
        {
            var vm = new ViewModel();
            Assert.Equal(0, vm.Calculated);
            var newValue = new ObservableCollection<int>();
            vm.Leaf = newValue;
            Assert.Equal(0, vm.Calculated);
            newValue.Add(11);
            Assert.Equal(1, vm.Calculated);
        }

        [Fact]
        public void TriggerCollectionUpdated_OldCollectionIsNotSubscribedTo()
        {
            var vm = new ViewModel();
            vm.Leaf.Add(11);
            Assert.Equal(1, vm.Calculated);
            var oldValue = vm.Leaf;
            vm.Leaf = new ObservableCollection<int>();
            Assert.Equal(0, vm.Calculated);

            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            oldValue.Add(11);
            Assert.Equal(0, vm.Calculated);
            Assert.Equal(new string[] { }, changes);
        }

        [Fact]
        public void ItemPropertyUpdated_DoesNotRecalculate()
        {
            var vm = new ViewModel();
            vm.Children.Add(new ViewModel.ChildViewModel { Name = "Steve" });
            Assert.Equal("Steve", vm.FirstChildName);
            var changes = new List<string>();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

            vm.Children[0].Name = "Bob";

            Assert.Equal("Steve", vm.FirstChildName);
            Assert.Equal(new string[] { }, changes);
        }
    }
}
