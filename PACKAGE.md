# System.Terminal

System.Terminal is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for System.Console.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. System.Terminal provides
an API centered around a [VT100 terminal](https://vt100.net) with some
extensions from later models and modern terminal emulators. It works on all
desktop platforms that .NET 6+ supports.

This project offers the following packages:

* [Terminal](https://www.nuget.org/packages/Terminal): Provides the core
  terminal API.
* [Terminal.Hosting](https://www.nuget.org/packages/Terminal.Hosting): Provides
  the terminal hosting model.
* [Terminal.Extensions](https://www.nuget.org/packages/Terminal.Extensions):
  Provides terminal hosting and logging for the .NET Generic Host.
* [Terminal.Testing](https://www.nuget.org/packages/Terminal.Testing): Provides
  testing utilities for terminal applications.

See the
[sample programs](https://github.com/alexrp/system-terminal/tree/master/src/samples)
for examples of what the API can do.

For more information, please visit the
[project page](https://github.com/alexrp/system-terminal).
