using Nito.Mvvm;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class PropertyChangedEventArgsCacheUnitTests
    {
        [Fact]
        public void Get_NotInCache_IsCreated()
        {
            var name = Guid.NewGuid().ToString("N");
            var result = PropertyChangedEventArgsCache.Instance.Get(name);
            Assert.Equal(name, result.PropertyName);
        }

        [Fact]
        public void Get_InCache_IsReturned()
        {
            var name = Guid.NewGuid().ToString("N");
            var result1 = PropertyChangedEventArgsCache.Instance.Get(name);
            var result2 = PropertyChangedEventArgsCache.Instance.Get(name);
            Assert.Equal(name, result1.PropertyName);
            Assert.Same(result1, result2);
        }

        [Fact]
        public async Task Cache_IsSharedBetweenThreads()
        {
            var name = Guid.NewGuid().ToString("N");
            var result1 = await Task.Run(() => PropertyChangedEventArgsCache.Instance.Get(name));
            var result2 = PropertyChangedEventArgsCache.Instance.Get(name);
            Assert.Equal(name, result1.PropertyName);
            Assert.Same(result1, result2);
        }
    }
}