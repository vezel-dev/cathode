using System;

namespace Microsoft.Extensions.Logging
{
    sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        NullScope()
        {
        }

        public void Dispose()
        {
        }
    }
}
