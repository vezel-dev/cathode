# System.Terminal

<div align="center">
    <img src="system-terminal.svg"
         width="128" />
</div>

<p align="center">
    <strong>
        A terminal-centric replacement for System.Console.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/alexrp/system-terminal?color=brown)](LICENSE.md)
[![Commits](https://img.shields.io/github/commit-activity/m/alexrp/system-terminal/master?label=commits&color=slateblue)](https://github.com/alexrp/system-terminal/commits/master)
[![Build](https://img.shields.io/github/workflow/status/alexrp/system-terminal/Build/master)](https://github.com/alexrp/system-terminal/actions/workflows/build.yml)
[![Sponsors](https://img.shields.io/github/sponsors/alexrp?color=mediumorchid)](https://github.com/sponsors/alexrp)
[![Discussions](https://img.shields.io/github/discussions/alexrp/system-terminal?color=teal)](https://github.com/alexrp/system-terminal/discussions)

</div>

---

System.Terminal is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for System.Console.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. System.Terminal provides
an API centered around a [VT100 terminal](https://vt100.net) with some
extensions from later models and modern terminal emulators. It works on all
desktop platforms that .NET 6+ supports.

Please note that, since System.Terminal replaces a very fundamental component of
the framework, the use of certain framework APIs becomes problematic. As an
example, intermixing System.Terminal and System.Console usage *will* break.
Referencing System.Terminal (or a package that uses it) will pull in a Roslyn
analyzer which will diagnose problematic APIs and suggest working replacements.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![Terminal][core-img]][core-pkg] | Provides the core terminal API. | ![Downloads][core-dls] |
| [![Terminal.Analyzers][analyzers-img]][analyzers-pkg] | Provides diagnostic analyzers and source generators. | ![Downloads][analyzers-dls] |
| [![Terminal.Hosting][hosting-img]][hosting-pkg] | Provides the terminal hosting model. | ![Downloads][hosting-dls] |
| [![Terminal.Extensions][extensions-img]][extensions-pkg] | Provides terminal hosting and logging for the .NET Generic Host. | ![Downloads][extensions-dls] |

[core-pkg]: https://www.nuget.org/packages/Terminal
[analyzers-pkg]: https://www.nuget.org/packages/Terminal.Analyzers
[hosting-pkg]: https://www.nuget.org/packages/Terminal.Hosting
[extensions-pkg]: https://www.nuget.org/packages/Terminal.Extensions

[core-img]: https://img.shields.io/nuget/v/Terminal?label=Terminal
[analyzers-img]: https://img.shields.io/nuget/v/Terminal.Analyzers?label=Terminal.Analyzers
[hosting-img]: https://img.shields.io/nuget/v/Terminal.Hosting?label=Terminal.Hosting
[extensions-img]: https://img.shields.io/nuget/v/Terminal.Extensions?label=Terminal.Extensions

[core-dls]: https://img.shields.io/nuget/dt/Terminal?label=
[analyzers-dls]: https://img.shields.io/nuget/dt/Terminal.Analyzers?label=
[hosting-dls]: https://img.shields.io/nuget/dt/Terminal.Hosting?label=
[extensions-dls]: https://img.shields.io/nuget/dt/Terminal.Extensions?label=

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
while developing System.Terminal.)

## Statistics

![Repobeats](https://repobeats.axiom.co/api/embed/56d1f4cda2c680fe93627ab2f884a3ce78c7d1d6.svg)
