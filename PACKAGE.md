# Cathode

**Cathode** is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for the .NET console APIs.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. **Cathode** provides an
API centered around a [VT100 terminal](https://vt100.net) with some extensions
from later models and modern terminal emulators. It works on all desktop
platforms that .NET 6+ supports.

This project offers the following packages:

* [Vezel.Cathode](https://www.nuget.org/packages/Vezel.Cathode): Provides the
  core terminal API.
* [Vezel.Cathode.Analyzers](https://www.nuget.org/packages/Vezel.Cathode.Analyzers):
  Provides diagnostic analyzers and source generators.
* [Vezel.Cathode.Hosting](https://www.nuget.org/packages/Vezel.Cathode.Hosting):
  Provides the terminal hosting model.
* [Vezel.Cathode.Extensions](https://www.nuget.org/packages/Vezel.Cathode.Extensions):
  Provides terminal hosting and logging for the .NET Generic Host.

See the
[sample programs](https://github.com/vezel-dev/cathode/tree/master/src/samples)
for examples of what the API can do.

For more information, please visit the
[project page](https://github.com/vezel-dev/cathode).
