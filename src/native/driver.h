#pragma once

typedef struct TerminalDescriptor TerminalDescriptor;

typedef enum {
    TerminalException_None,
    TerminalException_ArgumentOutOfRange,
    TerminalException_PlatformNotSupported,
    TerminalException_TerminalNotAttached,
    TerminalException_TerminalConfiguration,
    TerminalException_Terminal,
} TerminalException;

typedef struct {
    TerminalException exception;
    const uint16_t *nullable message; // TODO: This should be char16_t.
    int32_t error;
} TerminalResult;

// Keep in sync with src/core/TerminalSignal.cs (public API).
typedef enum {
    TerminalSignal_Close,
    TerminalSignal_Interrupt,
    TerminalSignal_Quit,
    TerminalSignal_Terminate,
} TerminalSignal;

CATHODE_API void cathode_initialize(void);

CATHODE_API void cathode_get_descriptors(
    TerminalDescriptor *nonnull *nonnull std_in,
    TerminalDescriptor *nonnull *nonnull std_out,
    TerminalDescriptor *nonnull *nonnull std_err,
    TerminalDescriptor *nonnull *nonnull tty_in,
    TerminalDescriptor *nonnull *nonnull tty_out);

CATHODE_API bool cathode_is_valid(const TerminalDescriptor *nonnull descriptor, bool write);

CATHODE_API bool cathode_is_interactive(const TerminalDescriptor *nonnull descriptor);

CATHODE_API bool cathode_query_size(int32_t *nonnull width, int32_t *nonnull height);

CATHODE_API bool cathode_get_mode(void);

// Requires synchronization by the caller.
CATHODE_API TerminalResult cathode_set_mode(bool raw, bool flush);

CATHODE_API TerminalResult cathode_generate_signal(TerminalSignal signal);

CATHODE_API TerminalResult cathode_read(
    const TerminalDescriptor *nonnull handle, uint8_t *nullable buffer, int32_t length, int32_t *nonnull progress);

CATHODE_API TerminalResult cathode_write(
    const TerminalDescriptor *nonnull handle,
    const uint8_t *nullable buffer,
    int32_t length,
    int32_t *nonnull progress);
