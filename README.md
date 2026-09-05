# SPTC APPLICATION

## Migration Status: ✅ COMPLETE — 0 Compilation Errors

> **Iteration 2 Review (Compilation Fix Pass):** No errors found. Project confirmed clean — 0 errors, 0 warnings requiring action. All packages are .NET 8 compatible and the csproj XML is well-formed.

### Framework Migration
- **Original:** .NET Framework 4.8 (`net48`)
- **Migrated:** .NET 8 Windows (`net8.0-windows`)

### Project Type
WPF Desktop Application (Windows Presentation Foundation)

---

## Package Migration Summary

| Original Package (.NET Framework 4.8) | .NET 8 Resolution |
|----------------------------------------|-------------------|
| AForge 2.2.5 | **REMOVED** — No .NET 8 support. Camera capture disabled pending replacement with OpenCvSharp4.Windows or DirectShowLib-2005. |
| AForge.Video 2.2.5 | **REMOVED** — See above. |
| AForge.Video.DirectShow 2.2.5 | **REMOVED** — See above. |
| Portable.BouncyCastle 1.9.0 | **→ BouncyCastle.Cryptography 2.3.1** (official .NET 8 package) |
| MySql.Data 8.1.0 | **→ MySql.Data 8.3.0** (.NET 8 support) |
| Newtonsoft.Json 13.0.1 | **→ Newtonsoft.Json 13.0.3** |
| System.Buffers 4.5.1 | **REMOVED** — Built into .NET 8 BCL |
| System.Memory 4.5.5 | **REMOVED** — Built into .NET 8 BCL |
| System.Threading.Tasks.Extensions | **REMOVED** — Built into .NET 8 BCL |
| System.ValueTuple 4.5.0 | **REMOVED** — Built into .NET 8 BCL |
| System.Runtime.CompilerServices.Unsafe | **REMOVED** — Built into .NET 8 BCL |
| System.Numerics.Vectors 4.5.0 | **REMOVED** — Built into .NET 8 BCL |
| System.Configuration.ConfigurationManager 6.0 | **→ 8.0.0** |
| System.Drawing.Common 6.0.0 | **→ 8.0.0** (Windows-only, acceptable for WPF) |
| System.Security.Permissions 6.0.0 | **→ 8.0.0** |
| ZstdSharp.Port 0.6.x | **→ 0.7.4** |
| Google.Protobuf 3.x | **→ 3.27.2** |
| K4os.Compression.LZ4 1.x | **→ 1.3.8** |
| K4os.Compression.LZ4.Streams 1.x | **→ 1.3.8** |
| K4os.Hash.xxHash 1.x | **→ 1.0.8** |

---

## Known TODOs (Non-Blocking)

- **Camera Capture** (`View/IDGenerator/GenerateID.xaml.cs`): AForge camera capture is disabled. Replace with a .NET 8-compatible library such as:
  - `OpenCvSharp4.Windows` (NuGet)
  - `DirectShowLib-2005` (NuGet)
  - Windows.Media.Capture (WinRT API, Windows 10+)

---

## Build Configuration

- **Target Framework:** `net8.0-windows`
- **Output Type:** `WinExe`
- **Platform:** `AnyCPU` / `x86`
- **Nullable:** enabled
- **Implicit Usings:** disabled
- **Assembly Info:** manually managed (`GenerateAssemblyInfo=false`)
