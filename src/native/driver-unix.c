// SPDX-License-Identifier: 0BSD

#if !defined(ZIG_OS_WINDOWS)

#include <errno.h>
#include <fcntl.h>
#include <poll.h>
#include <signal.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <unistd.h>

#include "driver-unix.h"

struct TerminalDescriptor
{
    int fd;
};

static TerminalDescriptor stdio_in;
static TerminalDescriptor stdio_out;
static TerminalDescriptor stdio_err;
static TerminalDescriptor tty;
static struct termios original_termios;
static bool original_termios_saved;
static bool raw_mode;
static struct sigaction original_ttou;
static atomic bool ttou_seen;

[[gnu::constructor]]
static void constructor(void)
{
    stdio_in.fd = STDIN_FILENO;
    stdio_out.fd = STDOUT_FILENO;
    stdio_err.fd = STDERR_FILENO;
    tty.fd = open("/dev/tty", O_RDWR | O_NOCTTY | O_CLOEXEC);
}

[[gnu::destructor]]
static void destructor(void)
{
    if (original_termios_saved)
        tcsetattr(tty.fd, TCSAFLUSH, &original_termios);

    close(tty.fd);
}

void cathode_get_descriptors(
    TerminalDescriptor *nonnull *nonnull std_in,
    TerminalDescriptor *nonnull *nonnull std_out,
    TerminalDescriptor *nonnull *nonnull std_err,
    TerminalDescriptor *nonnull *nonnull tty_in,
    TerminalDescriptor *nonnull *nonnull tty_out)
{
    assert(std_in);
    assert(std_out);
    assert(std_err);
    assert(tty_in);
    assert(tty_out);

    *std_in = &stdio_in;
    *std_out = &stdio_out;
    *std_err = &stdio_err;
    *tty_in = *tty_out = &tty;
}

bool cathode_is_valid(const TerminalDescriptor *nonnull descriptor, bool)
{
    assert(descriptor);

    return descriptor->fd >= 0;
}

bool cathode_is_interactive(const TerminalDescriptor *nonnull descriptor)
{
    assert(descriptor);

    return isatty(descriptor->fd) == 1;
}

bool cathode_query_size(int32_t *nonnull width, int32_t *nonnull height)
{
    assert(width);
    assert(height);

    struct winsize size;

    if (ioctl(tty.fd, TIOCGWINSZ, &size))
        return false;

    *width = size.ws_col;
    *height = size.ws_row;

    return true;
}

bool cathode_get_mode(void)
{
    return raw_mode;
}

static bool is_extended(const struct sigaction *nonnull action)
{
    assert(action);

    return action->sa_flags & SA_SIGINFO;
}

static bool is_default(const struct sigaction *nonnull action)
{
    assert(action);

    return ((const void *)&action->sa_handler == &action->sa_sigaction || !is_extended(action)) &&
        action->sa_handler == SIG_DFL;
}

static bool is_ignored(const struct sigaction *nonnull action)
{
    assert(action);

    return ((const void *)&action->sa_handler == &action->sa_sigaction || !is_extended(action)) &&
        action->sa_handler == SIG_IGN;
}

static void ttou_handler(int signo, siginfo_t *info, void *context)
{
    ttou_seen = true;

    // If a non-default handler already existed, we should chain to it.
    if (is_default(&original_ttou))
        return;

    if (is_extended(&original_ttou))
        original_ttou.sa_sigaction(signo, info, context);
    else
        original_ttou.sa_handler(signo);
}

TerminalResult cathode_set_mode(bool raw, bool flush)
{
    struct termios termios;

    if (tcgetattr(tty.fd, &termios) == -1)
        return (TerminalResult)
        {
            .exception = TerminalException_TerminalNotAttached,
        };

    // Stash away the original settings the first time we are successfully called.
    if (!original_termios_saved)
    {
        original_termios = termios;

        original_termios_saved = true;
    }

    // These values are usually the default, but we set them just to be safe since UnixTerminalReader would not behave
    // as expected by callers if these values differ.
    termios.c_cc[VTIME] = 0;
    termios.c_cc[VMIN] = 1;

    // Turn off some features that make little or no sense for virtual terminals.
    termios.c_iflag &= (tcflag_t)~(IGNBRK | IGNPAR | PARMRK | INPCK | ISTRIP | IXOFF | IMAXBEL);
    termios.c_oflag &= (tcflag_t)~(OFILL | OFDEL | NLDLY | CRDLY | TABDLY | BSDLY | VTDLY | FFDLY);
    termios.c_oflag |= NL0 | CR0 | TAB0 | BS0 | VT0 | FF0;
    termios.c_cflag &= (tcflag_t)~(CSTOPB | PARENB | PARODD | HUPCL | CLOCAL | CRTSCTS);
#if defined(ZIG_OS_LINUX)
    termios.c_cflag &= (tcflag_t)~CMSPAR;
#elif defined(ZIG_OS_MACOS)
    termios.c_cflag &= (tcflag_t)~(CDTR_IFLOW | CDSR_OFLOW | MDMBUF);
#endif
    termios.c_lflag &= (tcflag_t)~(FLUSHO | EXTPROC);

    // Set up some sensible defaults.
    termios.c_iflag &= (tcflag_t)~(IGNCR | INLCR | IXANY);
#if defined(ZIG_OS_LINUX)
    termios.c_iflag &= (tcflag_t)~IUCLC;
#endif
    termios.c_iflag |= IUTF8;
    termios.c_oflag &= (tcflag_t)~(OCRNL | ONOCR | ONLRET);
#if defined(ZIG_OS_LINUX)
    termios.c_oflag &= (tcflag_t)~OLCUC;
#elif defined(ZIG_OS_MACOS)
    termios.c_oflag &= (tcflag_t)~ONOEOT;
#endif
    termios.c_cflag &= (tcflag_t)~CSIZE;
    termios.c_cflag |= CS8 | CREAD;
    termios.c_lflag &= (tcflag_t)~(ECHONL | NOFLSH | ECHOPRT | PENDIN);
#if defined(ZIG_OS_LINUX)
    termios.c_lflag &= (tcflag_t)~XCASE;
#elif defined(ZIG_OS_MACOS)
    termios.c_lflag &= (tcflag_t)~ALTWERASE;
#endif

    tcflag_t iflag_cooked = BRKINT | ICRNL | IXON;
    tcflag_t oflag_cooked = OPOST | ONLCR;
    tcflag_t lflag_cooked = ISIG | ICANON | ECHO | ECHOE | ECHOK | ECHOCTL | ECHOKE | IEXTEN;
    tcflag_t lflag_raw = TOSTOP;
#if defined(ZIG_OS_MACOS)
    lflag_raw |= NOKERNINFO;
#endif

    // Finally, enable/disable features that depend on raw/cooked mode.
    if (raw)
    {
        termios.c_iflag &= ~iflag_cooked;
        termios.c_oflag &= ~oflag_cooked;
        termios.c_lflag &= ~lflag_cooked;
        termios.c_lflag |= lflag_raw;
    }
    else
    {
        termios.c_iflag |= iflag_cooked;
        termios.c_oflag |= oflag_cooked;
        termios.c_lflag |= lflag_cooked;
        termios.c_lflag &= ~lflag_raw;
    }

    if (!raw)
    {
        sigaction(SIGTTOU, nullptr, &original_ttou);

        // If SIGTTOU is ignored, we do not need to do anything.
        if (!is_ignored(&original_ttou))
        {
            struct sigaction ttou;

            if (is_default(&original_ttou))
            {
                sigemptyset(&ttou.sa_mask);

                ttou.sa_flags = 0;
            }
            else
            {
                // If a non-default handler already exists, maintain its mask and flags (although we must remove
                // SA_RESTART as that would defeat the purpose of this whole exercise).
                ttou.sa_mask = original_ttou.sa_mask;
                ttou.sa_flags = original_ttou.sa_flags & ~SA_RESTART;
            }

            // Ensure that we receive full signal information (SA_SIGINFO) so that we can chain to an existing handler.
            // Also, only handle the signal once (SA_RESETHAND).
            ttou.sa_flags |= SA_SIGINFO | SA_RESETHAND;
            ttou.sa_sigaction = ttou_handler;

            sigaction(SIGTTOU, &ttou, nullptr);
        }

        ttou_seen = false;
    }

    int ret;

    while ((ret = tcsetattr(tty.fd, flush ? TCSAFLUSH : TCSANOW, &termios)) == -1 && errno == EINTR)
    {
        // Retry in case we get interrupted by a signal. If we are trying to switch to cooked mode and we saw SIGTTOU,
        // it means we are a background process. We will trust that, by the time we actually read or write anything, we
        // will be in cooked mode.
        if (ttou_seen)
        {
            ret = 0;

            break;
        }
    }

    if (!raw)
        sigaction(SIGTTOU, &original_ttou, nullptr);

    if (ret)
        return (TerminalResult)
        {
            .exception = TerminalException_TerminalConfiguration,
            .message = u"Could not change terminal mode.",
            .error = errno,
        };

    raw_mode = raw;

    return (TerminalResult)
    {
        .exception = TerminalException_None,
    };
}

TerminalResult cathode_generate_signal(TerminalSignal signal)
{
    int signo;

    switch (signal)
    {
        case TerminalSignal_Close:
            signo = SIGHUP;
            break;
        case TerminalSignal_Interrupt:
            signo = SIGINT;
            break;
        case TerminalSignal_Quit:
            signo = SIGQUIT;
            break;
        case TerminalSignal_Terminate:
            signo = SIGTERM;
            break;
        default:
            return (TerminalResult)
            {
                .exception = TerminalException_ArgumentOutOfRange,
            };
    }

    kill(0, signo);

    return (TerminalResult)
    {
        .exception = TerminalException_None,
    };
}

TerminalResult cathode_read(
    TerminalDescriptor *nonnull descriptor, uint8_t *nullable buffer, int32_t length, int32_t *nonnull progress)
{
    assert(descriptor);
    assert(buffer);
    assert(progress);

    while (true)
    {
        ssize_t ret;

        // Note that this call may get us suspended by way of a SIGTTIN signal if we are a background process and the
        // handle refers to a terminal.
        while ((ret = read(descriptor->fd, buffer, (size_t)length)) == -1 && errno == EINTR)
        {
            // Retry in case we get interrupted by a signal.
        }

        bool success = true;

        // EPIPE means the descriptor was probably redirected to a program that ended.
        if (ret != -1)
            *progress = (int32_t)ret;
        else if (errno == EPIPE)
            *progress = 0;
        else
            success = false;

        // EAGAIN means the descriptor was configured as non-blocking. Instead of busily trying to read over and over,
        // poll until something happens (we can read, or an error occurs) and loop around again.
        if (!success && errno == EAGAIN)
        {
            cathode_poll(false, &descriptor->fd, nullptr, 1);

            continue;
        }

        return success
            ? (TerminalResult)
            {
                .exception = TerminalException_None,
            }
            : (TerminalResult)
            {
                .exception = TerminalException_Terminal,
                .message = u"Could not read from input handle.",
                .error = errno,
            };
    }
}

TerminalResult cathode_write(
    TerminalDescriptor *nonnull descriptor, const uint8_t *nullable buffer, int32_t length, int32_t *nonnull progress)
{
    assert(descriptor);
    assert(buffer);
    assert(progress);

    while (true)
    {
        ssize_t ret;

        // Note that this call may get us suspended by way of a SIGTTOU signal if we are a background process, the
        // handle refers to a terminal, and the TOSTOP bit is set (we disable TOSTOP but there are ways that it could
        // get set anyway).
        while ((ret = write(descriptor->fd, buffer, (size_t)length)) == -1 && errno == EINTR)
        {
            // Retry in case we get interrupted by a signal.
        }

        bool success = true;

        // EPIPE means the descriptor was probably redirected to a program that ended.
        if (ret != -1)
            *progress = (int32_t)ret;
        else if (errno == EPIPE)
            *progress = 0;
        else
            success = false;

        // EAGAIN means the descriptor was configured as non-blocking. Instead of busily trying to write over and over,
        // poll until something happens (we can write, or an error occurs) and loop around again.
        if (!success && errno == EAGAIN)
        {
            cathode_poll(true, &descriptor->fd, nullptr, 1);

            continue;
        }

        return success
            ? (TerminalResult)
            {
                .exception = TerminalException_None,
            }
            : (TerminalResult)
            {
                .exception = TerminalException_Terminal,
                .message = u"Could not write to output handle.",
                .error = errno,
            };
    }
}

void cathode_poll(bool write, const int *nonnull fds, bool *nullable results, int count)
{
    assert(fds);
    assert(count);

    struct pollfd pfds[count]; // count is only ever expected to be 1 or 2.

    for (int i = 0; i < count; i++)
        pfds[i] = (struct pollfd)
        {
            .fd = fds[i],
            .events = write ? POLLOUT : POLLIN,
        };

    int ret;

    while ((ret = poll(pfds, (nfds_t)count, -1)) == -1 && errno == EINTR)
    {
        // Retry in case we get interrupted by a signal.
    }

    if (results)
        for (int i = 0; i < count; i++)
            results[i] = pfds[i].revents & (write ? POLLOUT : POLLIN);
}

#endif
