# Home

**Cathode** is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for the .NET console APIs.

With Windows Terminal having replaced the old Windows console host, it makes
little sense for console interaction to still be centered around the old console
host and the many limitations it had. **Cathode** provides an API centered
around a [VT100 terminal](https://vt100.net) with some extensions from later
models and modern terminal emulators. It works on all desktop platforms that
.NET supports.

Please note that, since **Cathode** replaces a very fundamental component of the
framework, the use of certain framework APIs (e.g. `System.Console`) becomes
problematic. An analyzer will automatically diagnose usage of such APIs and
suggest working replacements.

## Terminals

**Cathode** aims to have excellent support for the following terminal emulators,
multiplexers, and libraries:

* [Alacritty](https://alacritty.org)
* [Black Box](https://gitlab.gnome.org/raggesilver/blackbox)
* [ConEmu](https://conemu.github.io)
* [Contour](https://contour-terminal.org)
* [foot](https://codeberg.org/dnkl/foot)
* [GNOME Terminal](https://help.gnome.org/users/gnome-terminal/stable)
* [Guake](http://guake.org)
* [hterm](https://hterm.org)
* [iTerm2](https://iterm2.com)
* [kitty](https://sw.kovidgoyal.net/kitty)
* [Konsole](https://konsole.kde.org)
* [mintty](https://mintty.github.io)
* [mlterm](http://mlterm.sourceforge.net)
* [PuTTY](https://www.putty.org)
* [rxvt-unicode](http://software.schmorp.de/pkg/rxvt-unicode.html)
* [Terminal.app](https://support.apple.com/guide/terminal/welcome/mac)
* [Terminology](https://terminolo.gy)
* [Tilix](https://gnunn1.github.io/tilix-web)
* [tmux](https://github.com/tmux/tmux/wiki)
* [VTE](https://gitlab.gnome.org/GNOME/vte)
* [WezTerm](https://wezfurlong.org/wezterm)
* [Windows Terminal](https://aka.ms/terminal)
* [Xfce Terminal](https://docs.xfce.org/apps/terminal/start)
* [Xterm.js](https://xtermjs.org)
* [xterm](https://invisible-island.net/xterm)
* [Yakuake](https://apps.kde.org/yakuake)

Even if you are using a terminal emulator that is not listed here, chances are
that it will work just fine; these are just the ones that are tested regularly
during development.
