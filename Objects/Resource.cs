using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// cz-dotnet-0007 FIX: Removed [DllImport("gdi32.dll")] P/Invoke for DeleteObject.
// The Windows-specific NativeMethods.DeleteObject(hBitmap) call caused DllNotFoundException
// on Linux containers (AKS). Replaced with a fully managed cross-platform implementation:
// System.Drawing.Bitmap is encoded to a MemoryStream using PNG format — no native GDI
// handle (HBITMAP) is needed. WPF BitmapSource references removed for net8.0 Linux compatibility.

namespace SPTC_APPLICATION.Objects
{
    public class Resource
    {
        public static class BitmapConversion
        {
            /// <summary>
            /// Converts a System.Drawing.Bitmap to a byte array using PNG format.
            /// This is cross-platform safe and does not rely on any Windows-specific
            /// GDI32 P/Invoke calls.
            /// </summary>
            public static byte[] ToByteArray(Bitmap bitmap)
            {
                if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

                using (MemoryStream stream = new MemoryStream())
                {
                    // Save the bitmap into the stream using PNG format (lossless, cross-platform)
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }

            /// <summary>
            /// Creates a System.Drawing.Bitmap from a byte array.
            /// </summary>
            public static Bitmap FromByteArray(byte[] data)
            {
                if (data == null || data.Length == 0) return null;

                using (MemoryStream stream = new MemoryStream(data))
                {
                    return new Bitmap(stream);
                }
            }
        }

        // cz-dotnet-0007: NativeMethods class with [DllImport("gdi32.dll")] has been removed.
        // The DeleteObject P/Invoke is no longer needed because ToBitmapSource no longer
        // calls bitmap.GetHbitmap(), eliminating the unmanaged GDI handle lifecycle entirely.
    }
}
