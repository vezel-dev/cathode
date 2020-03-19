using System;

namespace Microsoft.Extensions.Logging.Terminal
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
