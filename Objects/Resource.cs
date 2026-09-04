// ============================================================
// Resource.cs
// ============================================================
// cz-dotnet-0007: Removed Windows-specific P/Invoke DllImport("gdi32.dll")
// for DeleteObject and all WPF/System.Drawing dependencies.
//
// The BitmapConversion helper has been replaced with a cross-platform
// implementation that works on Linux AKS containers without any native
// DLL or WPF dependency.  Image data is handled as raw byte arrays
// (see Objects/General.cs Image class).
// ============================================================

namespace SPTC_APPLICATION.Objects
{
    /// <summary>
    /// Cross-platform resource utilities.
    /// WPF-specific BitmapSource / System.Drawing.Bitmap conversions have
    /// been removed to support Linux container deployment on AKS.
    /// Image data is now stored and transferred as raw byte[] arrays.
    /// </summary>
    public class Resource
    {
        // BitmapConversion methods that relied on System.Drawing.Bitmap,
        // System.Windows.Media.Imaging.BitmapSource, and the gdi32.dll
        // P/Invoke DeleteObject have been removed (cz-dotnet-0007).
        //
        // Use Image.GetDataUri() to obtain a base64 data URI for HTML rendering,
        // or work directly with the byte[] picture field on the Image model.
    }
}
