# System.Terminal [![Build Status](https://github.com/alexrp/system-terminal/actions/workflows/build.yml/badge.svg)](https://github.com/alexrp/system-terminal/actions/workflows/build.yml)

[![Terminal](https://img.shields.io/nuget/v/Terminal.svg?label=Terminal)](https://www.nuget.org/packages/Terminal)
[![Terminal.Hosting](https://img.shields.io/nuget/v/Terminal.Hosting.svg?label=Terminal.Hosting)](https://www.nuget.org/packages/Terminal.Hosting)
[![Terminal.Extensions](https://img.shields.io/nuget/v/Terminal.Extensions.svg?label=Terminal.Extensions)](https://www.nuget.org/packages/Terminal.Extensions)

`System.Terminal` is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for `System.Console`.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. `System.Terminal` provides
an API centered around a [VT100 terminal](https://vt100.net) (with various
modern and widely supported extensions) and works on all desktop platforms that
.NET 6+ supports.

Please note that intermixing usage of `System.Terminal` and `System.Console` is
*not* supported; an application using `System.Terminal` should avoid using
`System.Console` entirely. A project that directly or indirectly references
`System.Terminal` will pull in
[a Roslyn analyzer](https://github.com/dotnet/roslyn-analyzers/blob/main/README.md#microsoftcodeanalysisbannedapianalyzers)
which diagnoses [problematic uses](src/core/BannedSymbols.txt) of
`System.Console` and related APIs.

## Usage

This project offers the following packages:

* [Terminal](https://www.nuget.org/packages/Terminal): Provides the core
  terminal API.
* [Terminal.Hosting](https://www.nuget.org/packages/Terminal.Hosting): Provides
  the terminal hosting model.
* [Terminal.Extensions](https://www.nuget.org/packages/Terminal.Extensions):
  Provides terminal hosting and logging for the .NET Generic Host.

To install a package, run `dotnet add package <name>`.

See the [sample programs](src/samples) for examples of what the API can do. The
samples can be run with
[`dotnet example`](https://github.com/patriksvensson/dotnet-example).

## Terminals

This project aims to have excellent support for the following terminal
emulators:

* [Alacritty](https://github.com/alacritty/alacritty)
* [ConEmu](https://conemu.github.io)
* [foot](https://codeberg.org/dnkl/foot)
* [GNOME Terminal](https://help.gnome.org/users/gnome-terminal/stable)
* [iTerm2](https://iterm2.com)
* [kitty](https://sw.kovidgoyal.net/kitty)
* [Konsole](https://konsole.kde.org)
* [mintty](https://mintty.github.io)
* [mlterm](http://mlterm.sourceforge.net)
* [PuTTY](https://www.putty.org)
* [rxvt-unicode](http://software.schmorp.de/pkg/rxvt-unicode.html)
* [Terminal.app](https://support.apple.com/guide/terminal/welcome/mac)
* [Terminology](https://terminolo.gy)
* [WezTerm](https://wezfurlong.org/wezterm)
* [Windows Console](https://docs.microsoft.com/en-us/windows/console)
* [Windows Terminal](https://aka.ms/terminal)
* [xterm](https://invisible-island.net/xterm)

(Even if you are using a terminal emulator that is not listed here, chances are
that it will work just fine; these are just the ones that are tested regularly
while developing `System.Terminal`.)

## Resources

These are helpful resources used in the development of `System.Terminal`:

* <https://bjh21.me.uk/all-escapes/all-escapes.txt>
* <https://codeberg.org/dnkl/foot/src/branch/master/doc/foot-ctlseqs.7.scd>
* <https://conemu.github.io/en/AnsiEscapeCodes.html>
* <https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences>
* <https://en.wikipedia.org/wiki/ANSI_escape_code>
* <https://en.wikipedia.org/wiki/C0_and_C1_control_codes>
* <https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797>
* <https://github.com/alacritty/alacritty/blob/master/docs/escape_support.md>
* <https://github.com/arakiken/mlterm/blob/master/doc/en/ControlSequences>
* <https://github.com/borisfaure/terminology/blob/master/README.md#extended-escapes-for-terminology-only>
* <https://github.com/chromium/hterm/blob/main/doc/ControlSequences.md>
* <https://github.com/mintty/mintty/wiki/CtrlSeqs>
* <https://invisible-island.net/xterm/ctlseqs/ctlseqs.html>
* <https://iterm2.com/documentation-escape-codes.html>
* <https://linux.die.net/man/4/console_codes>
* <https://linux.die.net/man/7/urxvt>
* <https://sw.kovidgoyal.net/kitty/protocol-extensions>
* <https://terminalguide.namepad.de>
* <https://vt100.net/docs>
* <https://wezfurlong.org/wezterm/escape-sequences.html>
* <https://www.ecma-international.org/publications/standards/Ecma-048.htm>
* <https://xtermjs.org/docs/api/vtfeatures/>
