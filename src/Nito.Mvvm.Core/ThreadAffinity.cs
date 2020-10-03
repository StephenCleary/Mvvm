using System;
using System.Collections.Generic;

namespace Nito.Mvvm
{
    /// <summary>
    /// A small type used to verify that the correct thread is used to access thread-affine objects.
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct ThreadAffinity
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private int? _managedThreadId;

        /// <summary>
        /// Binds this object to the current thread.
        /// </summary>
        /// <returns>A <see cref="ThreadAffinity"/> instance that can be used to verify future access to this object.</returns>
        public static ThreadAffinity BindToCurrentThread()
        {
            return new ThreadAffinity { _managedThreadId = Environment.CurrentManagedThreadId };
        }

        /// <summary>
        /// Verifies that the current thread is the thread that is bound to this object.
        /// </summary>
        public void VerifyCurrentThread()
        {
            if (_managedThreadId != Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("The calling thread cannot access this object because a different thread owns it.");
        }
    }
}
