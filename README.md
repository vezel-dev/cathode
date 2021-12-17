# System.Terminal [![Build Status](https://github.com/alexrp/system-terminal/actions/workflows/build.yml/badge.svg)](https://github.com/alexrp/system-terminal/actions/workflows/build.yml)

[![Terminal](https://img.shields.io/nuget/v/Terminal.svg?label=Terminal)](https://www.nuget.org/packages/Terminal)
[![Terminal.Hosting](https://img.shields.io/nuget/v/Terminal.Hosting.svg?label=Terminal.Hosting)](https://www.nuget.org/packages/Terminal.Hosting)
[![Terminal.Extensions](https://img.shields.io/nuget/v/Terminal.Extensions.svg?label=Terminal.Extensions)](https://www.nuget.org/packages/Terminal.Extensions)

System.Terminal is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for System.Console.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. System.Terminal provides
an API centered around an emulated [VT100 terminal](https://vt100.net) (with
various modern and widely supported extensions) and works on all desktop
platforms that .NET 6+ supports.

Please note that, since System.Terminal replaces a very fundamental component of
the framework, the use of certain framework APIs becomes problematic. As an
example, intermixing System.Terminal and System.Console usage *will* break.
Referencing System.Terminal (or a package that uses it) will pull in a Roslyn
analyzer that will diagnose
[problematic APIs](src/core/buildTransitive/BannedSymbols.txt) and suggest
working replacements.

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

## Statistics

![Repobeats](https://repobeats.axiom.co/api/embed/56d1f4cda2c680fe93627ab2f884a3ce78c7d1d6.svg)
