using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class SimpleUnitTests
    {
        public sealed class ViewModel : ViewModelBase
        {
            public int TriggerValue
            {
                get { return Properties.Get(7); }
                set { Properties.Set(value); }
            }

            public int CalculatedValue
            {
                get
                {
                    return Properties.Calculated(() =>
                    {
                        ++CalculatedValueExecutionCount;
                        return TriggerValue * 2;
                    });
                }
            }

            public int CalculatedValueExecutionCount;
        }

        [Fact]
        public void Trigger_UsesSpecifiedDefaultValue()
        {
            var vm = new ViewModel();
            Assert.Equal(7, vm.TriggerValue);
        }

        [Fact]
        public void Calculated_BeforeRead_DoesNotExecute()
        {
            var vm = new ViewModel();
            Assert.Equal(0, vm.CalculatedValueExecutionCount);
        }

        [Fact]
        public void Calculated_InitialEvaluation_CalculatesValue()
        {
            var vm = new ViewModel();
            Assert.Equal(14, vm.CalculatedValue);
            Assert.Equal(1, vm.CalculatedValueExecutionCount);
        }

        [Fact]
        public void Calculated_MultipleReads_CachesValue()
        {
            var vm = new ViewModel();
            var value = vm.CalculatedValue;
            value = vm.CalculatedValue;
            Assert.Equal(1, vm.CalculatedValueExecutionCount);
        }

        [Fact]
        public void TriggerChanged_RaisesPropertyChanged()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            var value = vm.TriggerValue;
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            vm.TriggerValue = 13;
            Assert.Equal(new[] { "TriggerValue" }, changes);
        }

        [Fact]
        public void TriggerChanged_UpdatesValue()
        {
            var vm = new ViewModel();
            vm.TriggerValue = 13;
            Assert.Equal(13, vm.TriggerValue);
        }

        [Fact]
        public void TriggerChanged_AfterCalculatedIsRead_RaisesPropertyChangedForCalculated()
        {
            var changes = new List<string>();
            var vm = new ViewModel();
            vm.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
            var originalValue = vm.CalculatedValue;
            vm.TriggerValue = 13;
            Assert.Equal(new[] { "TriggerValue", "CalculatedValue" }.OrderBy(x => x).ToArray(), changes.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void TriggerChanged_AfterCalculatedIsRead_RecalculatesCalculatedValue()
        {
            var vm = new ViewModel();
            var originalValue = vm.CalculatedValue;
            vm.TriggerValue = 13;
            var updatedValue = vm.CalculatedValue;
            Assert.Equal(14, originalValue);
            Assert.Equal(26, updatedValue);
            Assert.Equal(2, vm.CalculatedValueExecutionCount);
        }
    }
}
