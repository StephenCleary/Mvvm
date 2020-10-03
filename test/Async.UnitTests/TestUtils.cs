using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace UnitTests
{
    public class TestUtils
    {
        public static Func<bool> PropertyNotified<T, P>(T obj, Expression<Func<T, P>> prop) where T : INotifyPropertyChanged
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