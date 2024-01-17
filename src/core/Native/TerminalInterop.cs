namespace Vezel.Cathode.Native;

internal static unsafe partial class TerminalInterop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TerminalDescriptor
    {
    }

    public enum TerminalException
    {
        None,
        ArgumentOutOfRange,
        OperationCanceled,
        PlatformNotSupported,
        TerminalNotAttached,
        TerminalConfiguration,
        Terminal,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TerminalResult
    {
        public TerminalException Exception;

        public char* Message;

        public int Error;

        public readonly void ThrowIfError()
        {
            // For when ArgumentOutOfRangeException and/or OperationCanceledException are not expected.
            ThrowIfError(value: (object?)null);
        }

        public readonly void ThrowIfError<T>(T value, [CallerArgumentExpression(nameof(value))] string? name = null)
        {
            _ = value;

            if (Exception == TerminalException.None)
                return;

            switch (Exception)
            {
                case TerminalException.ArgumentOutOfRange:
                    throw new ArgumentOutOfRangeException(name);
                case TerminalException.OperationCanceled:
                    throw new OperationCanceledException(Unsafe.As<T, CancellationToken>(ref value));
                case TerminalException.PlatformNotSupported:
                    throw new PlatformNotSupportedException();
                case TerminalException.TerminalNotAttached:
                    throw new TerminalNotAttachedException();
                case TerminalException.TerminalConfiguration:
                    throw new TerminalConfigurationException($"{new(Message)} {new Win32Exception(Error).Message}");
                case TerminalException.Terminal:
                    throw new IO.TerminalException($"{new(Message)} {new Win32Exception(Error).Message}");
            }
        }
    }

    private const string Library = "Vezel.Cathode.Native";

    static TerminalInterop()
    {
        NativeLibrary.SetDllImportResolver(
#pragma warning disable CS0436
            typeof(ThisAssembly).Assembly,
#pragma warning restore CS0436
            static (name, asm, paths) =>
            {
                // First try the normal search algorithm that takes into account the application's configuration and
                // static dependency information.
                if (NativeLibrary.TryLoad(name, asm, paths, out var handle))
                    return handle;

                // If someone is trying to load some unknown library through our assembly, at this point, there is
                // nothing more that we can do.
                if (name != Library)
                    return 0;

                // It is now likely that someone is trying to use Cathode without static dependency information, so the
                // runtime has no idea how to find Vezel.Cathode.Native. In this case, it is likely to either sit right
                // next to Vezel.Cathode.dll, or in runtimes/<rid>/native.

                var directory = AppContext.BaseDirectory;
                var fileName = OperatingSystem.IsWindows()
                    ? $"{name}.dll"
                    : OperatingSystem.IsMacOS()
                        ? $"lib{name}.dylib"
                        : $"lib{name}.so";

                bool TryLoad(out nint handle, params string[] paths)
                {
                    return NativeLibrary.TryLoad(Path.Combine([directory, .. paths, fileName]), out handle);
                }

                return TryLoad(out handle)
                    ? handle
                    : TryLoad(out handle, "runtimes", RuntimeInformation.RuntimeIdentifier, "native")
                        ? handle
                        : 0;
            });
    }

    [LibraryImport(Library, EntryPoint = "cathode_initialize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Initialize();

    [LibraryImport(Library, EntryPoint = "cathode_get_descriptors")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void GetDescriptors(
        TerminalDescriptor** stdIn,
        TerminalDescriptor** stdOut,
        TerminalDescriptor** stdErr,
        TerminalDescriptor** ttyIn,
        TerminalDescriptor** ttyOut);

    [LibraryImport(Library, EntryPoint = "cathode_is_valid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool IsValid(TerminalDescriptor* descriptor, [MarshalAs(UnmanagedType.U1)] bool write);

    [LibraryImport(Library, EntryPoint = "cathode_is_interactive")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool IsInteractive(TerminalDescriptor* descriptor);

    [LibraryImport(Library, EntryPoint = "cathode_query_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool QuerySize(int* width, int* height);

    [LibraryImport(Library, EntryPoint = "cathode_get_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool GetMode();

    [LibraryImport(Library, EntryPoint = "cathode_set_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TerminalResult SetMode(
        [MarshalAs(UnmanagedType.U1)] bool raw, [MarshalAs(UnmanagedType.U1)] bool flush);

    [LibraryImport(Library, EntryPoint = "cathode_generate_signal")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TerminalResult GenerateSignal(TerminalSignal signal);

    [LibraryImport(Library, EntryPoint = "cathode_read")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TerminalResult Read(TerminalDescriptor* descriptor, byte* buffer, int length, int* progress);

    [LibraryImport(Library, EntryPoint = "cathode_write")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TerminalResult Write(TerminalDescriptor* descriptor, byte* buffer, int length, int* progress);

    [LibraryImport(Library, EntryPoint = "cathode_poll")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Poll([MarshalAs(UnmanagedType.U1)] bool write, int* fds, bool* results, int count);

    [LibraryImport(Library, EntryPoint = "cathode_cancel")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Cancel(TerminalDescriptor* descriptor);
}
