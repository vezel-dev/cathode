# System.Terminal

[![Latest Release](https://img.shields.io/github/release/alexrp/system-terminal/all.svg)](https://github.com/alexrp/system-terminal/releases)
[![NuGet Package](https://img.shields.io/nuget/v/System.Terminal.svg)](https://www.nuget.org/packages/System.Terminal)
[![Build Status](https://github.com/alexrp/system-terminal/workflows/CI/badge.svg)](https://github.com/alexrp/system-terminal/actions?workflow=CI)

`System.Terminal` is a terminal-centric replacement for `System.Console`.

With the Windows console host now supporting VT100 sequences, it makes little
sense for console interaction to still be centered around the old Windows
console host and the many limitations it had. `System.Terminal` provides an API
centered around a [full-featured VT100 terminal](https://vt100.net) and works on
all platforms that .NET Core supports.

Please note that intermixing usage of `System.Terminal` and `System.Console` is
*not* guaranteed to work, even if certain usage patterns of that sort happen to
do so currently. An application using `System.Terminal` should avoid
`System.Console` *entirely* to ensure that it will work correctly with all
future releases of `System.Terminal`.

## Usage

To install the core package, run `dotnet add package Terminal`. If you are
developing an application that uses the .NET Generic Host (with e.g. ASP.NET
Core or Orleans), you can instead install the extensions package by running
`dotnet add package Terminal.Extensions`.

See the [sample program](src/sample) for examples of what the API can do.

## Resources

These are useful resources used in the development of `System.Terminal`:

* <https://www.ecma-international.org/publications/standards/Ecma-048.htm>
* <https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences>
* <https://invisible-island.net/xterm/ctlseqs/ctlseqs.html>
* <https://linux.die.net/man/4/console_codes>
* <https://vt100.net/docs>

## License

Please see [LICENSE.md](LICENSE.md).

## Funding

[![Liberapay Receiving](http://img.shields.io/liberapay/receives/alexrp.svg?logo=liberapay)](https://liberapay.com/alexrp/donate)
[![Liberapay Patrons](http://img.shields.io/liberapay/patrons/alexrp.svg?logo=liberapay)](https://liberapay.com/alexrp)

I work on open source software projects such as this one in my spare time, and
make them available free of charge under permissive licenses. If you like my
work and would like to support me, you might consider donating. Please only
donate if you want to and have the means to do so; I want to be very clear that
all open source software I write will always be available for free and you
should not feel obligated to donate or pay for it in any way.
