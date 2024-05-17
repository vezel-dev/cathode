// SPDX-License-Identifier: 0BSD

namespace Vezel.Cathode.IO;

public abstract class TerminalHandle
{
    public abstract Stream Stream { get; }

    public abstract bool IsValid { get; }

    public abstract bool IsInteractive { get; }
}
