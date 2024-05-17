// SPDX-License-Identifier: 0BSD

using Vezel.Cathode.Native;

namespace Vezel.Cathode.Terminals;

internal sealed class WindowsVirtualTerminal : NativeVirtualTerminal
{
    public override event Action? Resumed
    {
        add
        {
            // Windows does not have a SIGSTOP/SIGCONT concept.
        }

        remove
        {
        }
    }

    public static WindowsVirtualTerminal Instance { get; } = new();

    private WindowsVirtualTerminal()
    {
    }

    internal override unsafe IDisposable? ArrangeCancellation(
        TerminalInterop.TerminalDescriptor* descriptor, bool write, CancellationToken cancellationToken)
    {
        return cancellationToken.CanBeCanceled
            ? cancellationToken.UnsafeRegister(
                static descriptor =>
                    TerminalInterop.Cancel((TerminalInterop.TerminalDescriptor*)Unsafe.Unbox<nuint>(descriptor!)),
                (nuint)descriptor)
            : null;
    }
}
