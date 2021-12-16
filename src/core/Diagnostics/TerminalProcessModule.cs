namespace System.Diagnostics;

public sealed class TerminalProcessModule
{
    public string? FileName => _module.FileName;

    public string? ModuleName => _module.ModuleName;

    public unsafe nuint Address => (nuint)(void*)_module.BaseAddress;

    public long Length => _module.ModuleMemorySize;

    readonly ProcessModule _module;

    internal TerminalProcessModule(ProcessModule module)
    {
        _module = module;
    }
}
