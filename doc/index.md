# Home

**Cathode** is a toolkit for writing terminal-based applications. It is
effectively a complete replacement for the .NET console APIs.

With the Windows console host now supporting virtual terminal sequences, it
makes little sense for console interaction to still be centered around the old
Windows console host and the many limitations it had. **Cathode** provides an
API centered around a [VT100 terminal](https://vt100.net) with some extensions
from later models and modern terminal emulators. It works on all desktop
platforms that .NET 6+ supports.
