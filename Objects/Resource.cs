// Resource.cs
// cz-dotnet-0007: Removed Windows-specific System.Drawing (GDI+) and WPF
// System.Windows.Media.Imaging dependencies.
// The Resource class now provides cross-platform image utilities using only
// System.IO.MemoryStream, compatible with net8.0 on Linux AKS node pools.

using System.IO;

namespace SPTC_APPLICATION.Objects
{
    public class Resource
    {
        public static class BitmapConversion
        {
            /// <summary>
            /// Converts a raw image byte array to a MemoryStream for cross-platform use.
            /// Replaces the previous Windows-only GetHbitmap() + gdi32.dll P/Invoke approach
            /// (cz-dotnet-0007). Compatible with net8.0 on Linux AKS node pools.
            /// </summary>
            public static MemoryStream? ToStream(byte[]? imageBytes)
            {
                if (imageBytes == null || imageBytes.Length == 0)
                    return null;

                var stream = new MemoryStream(imageBytes);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            /// <summary>
            /// Converts a MemoryStream to a byte array for database storage.
            /// </summary>
            public static byte[]? ToBytes(MemoryStream? stream)
            {
                if (stream == null)
                    return null;

                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        // NativeMethods class with DllImport("gdi32.dll") has been removed.
        // The Windows-only P/Invoke (cz-dotnet-0007) is no longer needed because
        // image handling now uses a fully managed MemoryStream-based path that is
        // cross-platform compatible with AKS Linux node pools.
    }
}
