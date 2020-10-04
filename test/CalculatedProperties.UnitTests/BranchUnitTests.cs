using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class BranchUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public bool UseB
            {
                get { return Properties.Get(false); }
                set { Properties.Set(value); }
            }

            public int A
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int B
            {
                get { return Properties.Get(11); }
                set { Properties.Set(value); }
            }

            public int CalculatedValue
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++CalculatedValueExecutionCount;
                        return UseB ? B : A;
                    });
                }
            }

            public int CalculatedValueExecutionCount;
        }
        
        [Fact]
        public void Calculated_InitialValueIsCalculated()
        {
            var vm = new ViewModel();
            Assert.Equal(7, vm.CalculatedValue);
            Assert.Equal(1, vm.CalculatedValueExecutionCount);
        }

        [Fact]
        public void IndependentPropertyChanges_DoesNotRaisePropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.B;
            value = vm.CalculatedValue;
            vm.B = 13;
            Assert.Equal(new[] { "B" }, changes);
        }

        [Fact]
        public void DependentPropertyChanges_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.A;
            value = vm.CalculatedValue;
            vm.A = 13;
            Assert.Equal(new[] { "A", "CalculatedValue" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void Calculated_AfterBranchSwitch_IsRecalculated()
        {
            var vm = new ViewModel();
            var value = vm.CalculatedValue;
            vm.UseB = true;
            Assert.Equal(11, vm.CalculatedValue);
            Assert.Equal(2, vm.CalculatedValueExecutionCount);
        }

        [Fact]
        public void IndependentPropertyChanges_AfterBranchSwitch_DoesNotRaisePropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.CalculatedValue;
            vm.UseB = true;
            value = vm.CalculatedValue;

            vm.A = 13;

            Assert.Equal(new[] { "UseB", "CalculatedValue", "A" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void DependentPropertyChanges_AfterBranchSwitch_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var value = vm.CalculatedValue;
            vm.UseB = true;
            value = vm.CalculatedValue;

            vm.B = 13;

            Assert.Equal(new[] { "UseB", "CalculatedValue", "B", "CalculatedValue" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }
    }
}
