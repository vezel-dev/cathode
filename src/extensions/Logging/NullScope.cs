// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.Extensions.Logging;

internal sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new();

    private NullScope()
    {
    }

    public void Dispose()
    {
    }
}
