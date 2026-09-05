using System;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

// NOTE: System.Drawing.Bitmap is available in .NET 8 via the System.Drawing.Common NuGet package.
// However, System.Drawing.Common on .NET 8 is Windows-only (throws PlatformNotSupportedException on non-Windows).
// This application targets net8.0-windows so this usage is acceptable.

namespace SPTC_APPLICATION.Objects
{
    public class Resource
    {
        public static class BitmapConversion
        {
            /// <summary>
            /// Converts a System.Drawing.Bitmap to a WPF BitmapSource.
            /// Uses GDI interop - Windows-only, compatible with net8.0-windows target.
            /// </summary>
            public static BitmapSource ToBitmapSource(Bitmap bitmap)
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
            }

            /// <summary>
            /// Converts a WPF BitmapSource to a System.Drawing.Bitmap.
            /// </summary>
            public static Bitmap ToBitmap(BitmapSource bitmapSource)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(stream);

                    using (Bitmap bitmap = new Bitmap(stream))
                    {
                        return new Bitmap(bitmap);
                    }
                }
            }
        }

        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
