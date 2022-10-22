# Cathode

<div align="center">
    <img src="cathode.svg"
         width="128" />
</div>

<p align="center">
    <strong>
        A terminal-centric replacement for the .NET console APIs.
    </strong>
</p>

<div align="center">

[![License](https://img.shields.io/github/license/vezel-dev/cathode?color=brown)](LICENSE-0BSD)
[![Commits](https://img.shields.io/github/commit-activity/m/vezel-dev/cathode/master?label=commits&color=slateblue)](https://github.com/vezel-dev/cathode/commits/master)
[![Build](https://img.shields.io/github/workflow/status/vezel-dev/cathode/Build/master)](https://github.com/vezel-dev/cathode/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/vezel-dev/cathode?color=teal)](https://github.com/vezel-dev/cathode/discussions)
[![Discord](https://img.shields.io/discord/960716713136095232?color=peru&label=discord)](https://discord.gg/UvAU4KTS8Q)

</div>

---

**Cathode** is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for the .NET console APIs.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. **Cathode** provides an
API centered around a [VT100 terminal](https://vt100.net) with some extensions
from later models and modern terminal emulators. It works on all desktop
platforms that .NET supports.

Please note that, since **Cathode** replaces a very fundamental component of the
framework, the use of certain framework APIs becomes problematic. As an example,
intermixing **Cathode** and `System.Console` usage *will* break. Referencing
**Cathode** (or a package that uses it) will pull in a Roslyn analyzer that
diagnoses problematic APIs and suggests working replacements.

## Usage

This project offers the following packages:

| Package | Description | Downloads |
| -: | - | :- |
| [![Vezel.Cathode][core-img]][core-pkg] | Provides the core terminal API. | ![Downloads][core-dls] |
| [![Vezel.Cathode.Hosting][hosting-img]][hosting-pkg] | Provides the terminal hosting model. | ![Downloads][hosting-dls] |
| [![Vezel.Cathode.Extensions][extensions-img]][extensions-pkg] | Provides terminal hosting and logging for the .NET Generic Host. | ![Downloads][extensions-dls] |

[core-pkg]: https://www.nuget.org/packages/Vezel.Cathode
[hosting-pkg]: https://www.nuget.org/packages/Vezel.Cathode.Hosting
[extensions-pkg]: https://www.nuget.org/packages/Vezel.Cathode.Extensions

[core-img]: https://img.shields.io/nuget/v/Vezel.Cathode?label=Vezel.Cathode
[hosting-img]: https://img.shields.io/nuget/v/Vezel.Cathode.Hosting?label=Vezel.Cathode.Hosting
[extensions-img]: https://img.shields.io/nuget/v/Vezel.Cathode.Extensions?label=Vezel.Cathode.Extensions

[core-dls]: https://img.shields.io/nuget/dt/Vezel.Cathode?label=
[hosting-dls]: https://img.shields.io/nuget/dt/Vezel.Cathode.Hosting?label=
[extensions-dls]: https://img.shields.io/nuget/dt/Vezel.Cathode.Extensions?label=

To install a package, run `dotnet add package <name>`.

See the [sample programs](src/samples) for examples of what the API can do. The
samples can be run with
[`dotnet example`](https://github.com/patriksvensson/dotnet-example).

For more information, please visit the
[project home page](https://docs.vezel.dev/cathode).

## Terminals

This project aims to have excellent support for the following terminal
emulators:

* [Alacritty](https://alacritty.org)
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
* [Windows Console](https://learn.microsoft.com/en-us/windows/console)
* [Windows Terminal](https://aka.ms/terminal)
* [xterm](https://invisible-island.net/xterm)

Even if you are using a terminal emulator that is not listed here, chances are
that it will work just fine; these are just the ones that are tested regularly
while developing **Cathode**.

## License

This project is licensed under the terms found in
[`LICENSE-0BSD`](LICENSE-0BSD).
