#if defined(ZIG_OS_WINDOWS)

#include <windows.h>

#include "driver-windows.h"

typedef struct {
    HANDLE handle;
    DWORD original_mode;
    UINT original_code_page;
} ConsoleState;

static HANDLE stdio_in;
static HANDLE stdio_out;
static HANDLE stdio_err;
static ConsoleState in_state;
static ConsoleState out_state;
static bool original_state_saved;
static bool raw_mode;

static HANDLE open_stdio_handle(DWORD type)
{
    HANDLE handle = GetStdHandle(type);

    // Convert a -1 handle to 0 so other code only has to worry about 0.
    return handle != INVALID_HANDLE_VALUE ? handle : nullptr;
}

static HANDLE open_console_handle(const wchar_t *nonnull name)
{
    assert(name);

    SECURITY_ATTRIBUTES attrs =
    {
        .nLength = sizeof(SECURITY_ATTRIBUTES),
        .lpSecurityDescriptor = nullptr,
        .bInheritHandle = true,
    };

    HANDLE handle = CreateFileW(
        name,
        FILE_GENERIC_READ | FILE_GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        &attrs,
        OPEN_EXISTING,
        0,
        nullptr);

    // Convert a -1 handle to 0 so other code only has to worry about 0.
    return handle != INVALID_HANDLE_VALUE ? handle : nullptr;
}

[[gnu::constructor]]
static void constructor(void)
{
    stdio_in = open_stdio_handle(STD_INPUT_HANDLE);
    stdio_out = open_stdio_handle(STD_OUTPUT_HANDLE);
    stdio_err = open_stdio_handle(STD_ERROR_HANDLE);
    in_state.handle = open_console_handle(u"CONIN$");
    out_state.handle = open_console_handle(u"CONOUT$");
}

[[gnu::destructor]]
static void destructor(void)
{
    if (original_state_saved)
    {
        SetConsoleMode(in_state.handle, in_state.original_mode);
        SetConsoleMode(out_state.handle, out_state.original_mode);

        SetConsoleCP(in_state.original_code_page);
        SetConsoleOutputCP(out_state.original_code_page);
    }

    // CloseHandle will throw an exception under a debugger if the given handle is invalid, so check first.

    if (in_state.handle)
        CloseHandle(in_state.handle);

    if (out_state.handle)
        CloseHandle(out_state.handle);
}

void cathode_get_handles(
    size_t *nonnull std_in,
    size_t *nonnull std_out,
    size_t *nonnull std_err,
    size_t *nonnull tty_in,
    size_t *nonnull tty_out)
{
    assert(std_in);
    assert(std_out);
    assert(std_err);
    assert(tty_in);
    assert(tty_out);

    *std_in = (size_t)stdio_in;
    *std_out = (size_t)stdio_out;
    *std_err = (size_t)stdio_err;
    *tty_in = (size_t)in_state.handle;
    *tty_out = (size_t)out_state.handle;
}

bool cathode_is_valid(size_t handle, bool write)
{
    if (!handle)
        return false;

    // Apparently, for Windows GUI programs, the standard I/O handles will appear to be valid (i.e. not -1 or 0) but
    // will not actually be usable. So do a zero-byte write to figure out if the handle is actually valid.
    if (write)
    {
        DWORD written;

        return WriteFile((HANDLE)handle, nullptr, 0, &written, nullptr);
    }

    return true;
}

bool cathode_is_interactive(size_t handle)
{
    DWORD mode;

    // Note that this also returns true for invalid handles.
    return GetFileType((HANDLE)handle) == FILE_TYPE_CHAR && GetConsoleMode((HANDLE)handle, &mode);
}

bool cathode_query_size(int32_t *nonnull width, int32_t *nonnull height)
{
    assert(width);
    assert(height);

    CONSOLE_SCREEN_BUFFER_INFO info;

    if (!GetConsoleScreenBufferInfo(out_state.handle, &info))
        return false;

    *width = info.srWindow.Right - info.srWindow.Left + 1;
    *height = info.srWindow.Bottom - info.srWindow.Top + 1;

    return true;
}

bool cathode_get_mode(void)
{
    return raw_mode;
}

TerminalResult cathode_set_mode(bool raw, bool flush)
{
    DWORD in_mode;
    DWORD out_mode;

    UINT in_code_page;
    UINT out_code_page;

    if (!GetConsoleMode(in_state.handle, &in_mode) ||
        !GetConsoleMode(out_state.handle, &out_mode) ||
        !(in_code_page = GetConsoleCP()) ||
        !(out_code_page = GetConsoleOutputCP()))
        return (TerminalResult)
        {
            .exception = TerminalException_TerminalNotAttached,
        };

    // Stash away the original modes the first time we are successfully called.
    if (!original_state_saved)
    {
        in_state.original_mode = in_mode;
        out_state.original_mode = out_mode;

        in_state.original_code_page = in_code_page;
        out_state.original_code_page = out_code_page;

        original_state_saved = true;
    }

    DWORD orig_in_mode = in_mode;
    DWORD orig_out_mode = out_mode;

    // Set up some sensible defaults.
    in_mode &= (DWORD)~(ENABLE_WINDOW_INPUT | ENABLE_MOUSE_INPUT | ENABLE_QUICK_EDIT_MODE);
    in_mode |= ENABLE_INSERT_MODE | ENABLE_EXTENDED_FLAGS | ENABLE_VIRTUAL_TERMINAL_INPUT;
    out_mode &= (DWORD)~ENABLE_LVB_GRID_WORLDWIDE;
    out_mode |= ENABLE_PROCESSED_OUTPUT | ENABLE_WRAP_AT_EOL_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING;

    DWORD in_mode_extra = ENABLE_PROCESSED_INPUT | ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT;
    DWORD out_mode_extra = DISABLE_NEWLINE_AUTO_RETURN;

    // Enable/disable features that depend on cooked/raw mode.
    if (!raw)
    {
        in_mode |= in_mode_extra;
        out_mode |= out_mode_extra;
    }
    else
    {
        in_mode &= ~in_mode_extra;
        out_mode &= ~out_mode_extra;
    }

    TerminalResult result;

    if (!SetConsoleCP(CP_UTF8) || !SetConsoleOutputCP(CP_UTF8))
    {
        result = (TerminalResult)
        {
            .exception = TerminalException_TerminalConfiguration,
            .message = u"Could not change console code page.",
            .error = (int32_t)GetLastError(),
        };

        goto done;
    }

    if (!SetConsoleMode(in_state.handle, in_mode) || !SetConsoleMode(out_state.handle, out_mode))
    {
        result = (TerminalResult)
        {
            .exception = TerminalException_TerminalConfiguration,
            .message = u"Could not change console mode.",
            .error = (int32_t)GetLastError(),
        };

        goto done;
    }

    if (flush && !FlushConsoleInputBuffer(in_state.handle))
    {
        result = (TerminalResult)
        {
            .exception = TerminalException_TerminalConfiguration,
            .message = u"Could not flush console input buffer.",
            .error = (int32_t)GetLastError(),
        };

        goto done;
    }

    result = (TerminalResult)
    {
        .exception = TerminalException_None,
    };

done:
    if (result.exception != TerminalException_None)
    {
        result.error = (int32_t)GetLastError();

        // If we failed to configure the console, try to undo partial configuration (if any).

        SetConsoleMode(in_state.handle, orig_in_mode);
        SetConsoleMode(out_state.handle, orig_out_mode);

        SetConsoleCP(in_code_page);
        SetConsoleOutputCP(out_code_page);
    }
    else
        raw_mode = raw;

    return result;
}

TerminalResult cathode_generate_signal(TerminalSignal signal)
{
    DWORD event;

    switch (signal)
    {
        case TerminalSignal_Interrupt:
            event = CTRL_C_EVENT;
            break;
        case TerminalSignal_Quit:
            event = CTRL_BREAK_EVENT;
            break;
        case TerminalSignal_Close:
        case TerminalSignal_Terminate:
            return (TerminalResult)
            {
                .exception = TerminalException_PlatformNotSupported,
            };
        default:
            return (TerminalResult)
            {
                .exception = TerminalException_ArgumentOutOfRange,
            };
    }

    GenerateConsoleCtrlEvent(event, 0);

    return (TerminalResult)
    {
        .exception = TerminalException_None,
    };
}

TerminalResult cathode_read(size_t handle, uint8_t *nullable buffer, int32_t length, int32_t *nonnull progress)
{
    assert(buffer);
    assert(progress);

    BOOL result = ReadFile((HANDLE)handle, buffer, (DWORD)length, (LPDWORD)progress, nullptr);
    DWORD error = GetLastError();

    // See driver-unix.c for the error handling rationale.
    return result || *progress || error == ERROR_HANDLE_EOF || error == ERROR_BROKEN_PIPE || error == ERROR_NO_DATA
        ? (TerminalResult)
        {
            .exception = TerminalException_None,
        }
        : (TerminalResult)
        {
            .exception = TerminalException_Terminal,
            .message = u"Could not read from input handle.",
            .error = (int32_t)error,
        };
}

TerminalResult cathode_write(size_t handle, const uint8_t *nullable buffer, int32_t length, int32_t *nonnull progress)
{
    assert(buffer);
    assert(progress);

    BOOL result = WriteFile((HANDLE)handle, buffer, (DWORD)length, (LPDWORD)progress, nullptr);
    DWORD error = GetLastError();

    // See driver-unix.c for the error handling rationale.
    return result || *progress || error == ERROR_HANDLE_EOF || error == ERROR_BROKEN_PIPE || error == ERROR_NO_DATA
        ? (TerminalResult)
        {
            .exception = TerminalException_None,
        }
        : (TerminalResult)
        {
            .exception = TerminalException_Terminal,
            .message = u"Could not write to output handle.",
            .error = (int32_t)error,
        };
}

#endif
