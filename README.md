# System.Terminal [![Latest Release](https://img.shields.io/github/release/alexrp/system-terminal/all.svg?label=Latest%20Release)](https://github.com/alexrp/system-terminal/releases) [![Build Status](https://github.com/alexrp/system-terminal/workflows/CI/badge.svg)](https://github.com/alexrp/system-terminal/actions?workflow=CI)

[![Terminal](https://img.shields.io/nuget/v/Terminal.svg?label=Terminal)](https://www.nuget.org/packages/Terminal)
[![Terminal.Extensions](https://img.shields.io/nuget/v/Terminal.Extensions.svg?label=Terminal.Extensions)](https://www.nuget.org/packages/Terminal.Extensions)
[![Terminal.Input](https://img.shields.io/nuget/v/Terminal.Input.svg?label=Terminal.Input)](https://www.nuget.org/packages/Terminal.Input)

`System.Terminal` is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for `System.Console`.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many idiosyncrasies it had. `System.Terminal`
provides an API centered around a [VT100 terminal](https://vt100.net) (with
various modern and widely supported extensions) and works on all platforms that
.NET 5.0+ supports.

Please note that intermixing usage of `System.Terminal` and `System.Console` is
*not* supported; an application using `System.Terminal` should avoid using
`System.Console` entirely. A project that directly or indirectly references
`System.Terminal` will pull in
[a Roslyn analyzer](https://github.com/dotnet/roslyn-analyzers/blob/master/README.md#microsoftcodeanalysisbannedapianalyzers)
which diagnoses [problematic uses](src/core/BannedSymbols.txt) of
`System.Console` and related APIs.

## Usage

This project offers the following packages:

* [Terminal](https://www.nuget.org/packages/Terminal): Provides the core
  terminal API.
* [Terminal.Extensions](https://www.nuget.org/packages/Terminal.Extensions):
  Provides terminal hosting and logging for the .NET Generic Host.
* [Terminal.Input](https://www.nuget.org/packages/Terminal.Input): Provides an
  interactive line editor.

To install a package, run `dotnet add package <name>`.

See the [sample program](src/sample) for examples of what the API can do.

## Resources

These are helpful resources used in the development of `System.Terminal`:

* <https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences>
* <https://github.com/chromium/hterm/blob/master/doc/ControlSequences.md>
* <https://invisible-island.net/xterm/ctlseqs/ctlseqs.html>
* <https://linux.die.net/man/4/console_codes>
* <https://terminalguide.namepad.de>
* <https://vt100.net/docs>
* <https://www.ecma-international.org/publications/standards/Ecma-048.htm>

## License

Please see [LICENSE.md](LICENSE.md).

## Funding

I work on open source software projects such as this one in my spare time, and
make them available free of charge under permissive licenses. If you like my
work and would like to support me, you might consider [sponsoring
me](https://github.com/sponsors/alexrp). Please only donate if you want to and
have the means to do so; I want to be very clear that all open source software I
write will always be available for free and you should not feel obligated to
donate or pay for it in any way.
