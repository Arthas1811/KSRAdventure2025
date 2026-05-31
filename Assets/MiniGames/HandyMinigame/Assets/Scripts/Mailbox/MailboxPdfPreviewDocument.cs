using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Renders PDF pages for the in-game mailbox preview.
public sealed class MailboxPdfPreviewDocument : IDisposable
{
    public const float MinZoom = 0.75f;
    public const float MaxZoom = 2.5f;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private const int MaxRenderedDimension = 1800;
    private const int PdfBitmapBgra = 4;
    private const int RenderAnnotations = 0x01;
    private const int RenderLcdText = 0x02;
    private const string PdfiumLibraryName = "pdfium";

    private static readonly object PdfiumLock = new object();
    private static bool libraryInitialized;
    private static int openDocumentCount;

    private IntPtr documentHandle;
    private IntPtr pdfBytesHandle;
    private bool disposed;

    private MailboxPdfPreviewDocument(IntPtr documentHandle, IntPtr pdfBytesHandle, int pageCount)
    {
        this.documentHandle = documentHandle;
        this.pdfBytesHandle = pdfBytesHandle;
        PageCount = pageCount;
    }

    public static bool IsSupported => true;

    public int PageCount { get; private set; }

    public static bool TryLoad(byte[] pdfBytes, out MailboxPdfPreviewDocument document, out string error)
    {
        document = null;
        error = null;

        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            error = "The PDF attachment is empty.";
            return false;
        }

        IntPtr buffer = IntPtr.Zero;
        bool retainedLibrary = false;

        try
        {
            buffer = Marshal.AllocHGlobal(pdfBytes.Length);
            Marshal.Copy(pdfBytes, 0, buffer, pdfBytes.Length);

            lock (PdfiumLock)
            {
                RetainLibrary();
                retainedLibrary = true;

                IntPtr loadedDocument = FPDF_LoadMemDocument(buffer, pdfBytes.Length, null);
                if (loadedDocument == IntPtr.Zero)
                {
                    error = $"PDFium could not open the PDF. {DescribePdfiumError(FPDF_GetLastError())}";
                    ReleaseLibrary();
                    retainedLibrary = false;
                    return false;
                }

                int pageCount = FPDF_GetPageCount(loadedDocument);
                if (pageCount <= 0)
                {
                    FPDF_CloseDocument(loadedDocument);
                    ReleaseLibrary();
                    retainedLibrary = false;
                    error = "The PDF does not contain any pages.";
                    return false;
                }

                document = new MailboxPdfPreviewDocument(loadedDocument, buffer, pageCount);
                buffer = IntPtr.Zero;
                return true;
            }
        }
        catch (DllNotFoundException exception)
        {
            error = $"PDFium is not available: {exception.Message}";
            return false;
        }
        catch (Exception exception)
        {
            error = $"Could not open the PDF preview: {exception.Message}";
            return false;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }

            if (retainedLibrary && document == null)
            {
                lock (PdfiumLock)
                {
                    ReleaseLibrary();
                }
            }
        }
    }

    public bool TryRenderPage(int pageIndex, float zoom, out Texture2D texture, out string error)
    {
        texture = null;
        error = null;

        if (disposed || documentHandle == IntPtr.Zero)
        {
            error = "The PDF preview is already closed.";
            return false;
        }

        if (pageIndex < 0 || pageIndex >= PageCount)
        {
            error = "The requested PDF page is outside the document.";
            return false;
        }

        IntPtr pageHandle = IntPtr.Zero;
        IntPtr bitmapHandle = IntPtr.Zero;
        GCHandle bufferHandle = default;

        try
        {
            byte[] bgraBytes;
            int width;
            int height;
            int stride;

            lock (PdfiumLock)
            {
                pageHandle = FPDF_LoadPage(documentHandle, pageIndex);
                if (pageHandle == IntPtr.Zero)
                {
                    error = $"PDFium could not load page {pageIndex + 1}. {DescribePdfiumError(FPDF_GetLastError())}";
                    return false;
                }

                float pageWidth = FPDF_GetPageWidthF(pageHandle);
                float pageHeight = FPDF_GetPageHeightF(pageHandle);
                if (pageWidth <= 0f || pageHeight <= 0f)
                {
                    error = "PDFium reported an invalid page size.";
                    return false;
                }

                float safeZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
                float targetWidth = pageWidth * safeZoom;
                float targetHeight = pageHeight * safeZoom;
                float maxDimension = Mathf.Max(targetWidth, targetHeight);
                if (maxDimension > MaxRenderedDimension)
                {
                    float capScale = MaxRenderedDimension / maxDimension;
                    targetWidth *= capScale;
                    targetHeight *= capScale;
                }

                width = Mathf.Max(1, Mathf.RoundToInt(targetWidth));
                height = Mathf.Max(1, Mathf.RoundToInt(targetHeight));
                stride = width * 4;
                bgraBytes = new byte[stride * height];
                bufferHandle = GCHandle.Alloc(bgraBytes, GCHandleType.Pinned);

                bitmapHandle = FPDFBitmap_CreateEx(width, height, PdfBitmapBgra, bufferHandle.AddrOfPinnedObject(), stride);
                if (bitmapHandle == IntPtr.Zero)
                {
                    error = "PDFium could not allocate a page bitmap.";
                    return false;
                }

                FPDFBitmap_FillRect(bitmapHandle, 0, 0, width, height, 0xffffffff);
                FPDF_RenderPageBitmap(bitmapHandle, pageHandle, 0, 0, width, height, 0, RenderAnnotations | RenderLcdText);
            }

            texture = CreateTextureFromBgra(bgraBytes, width, height, stride);
            return true;
        }
        catch (Exception exception)
        {
            error = $"Could not render the PDF page: {exception.Message}";
            return false;
        }
        finally
        {
            lock (PdfiumLock)
            {
                if (bitmapHandle != IntPtr.Zero)
                {
                    FPDFBitmap_Destroy(bitmapHandle);
                }

                if (pageHandle != IntPtr.Zero)
                {
                    FPDF_ClosePage(pageHandle);
                }
            }

            if (bufferHandle.IsAllocated)
            {
                bufferHandle.Free();
            }
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        lock (PdfiumLock)
        {
            if (documentHandle != IntPtr.Zero)
            {
                FPDF_CloseDocument(documentHandle);
                documentHandle = IntPtr.Zero;
                ReleaseLibrary();
            }
        }

        if (pdfBytesHandle != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(pdfBytesHandle);
            pdfBytesHandle = IntPtr.Zero;
        }
    }

    private static Texture2D CreateTextureFromBgra(byte[] bgraBytes, int width, int height, int stride)
    {
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            int sourceRow = (height - 1 - y) * stride;
            int targetRow = y * width;
            for (int x = 0; x < width; x++)
            {
                int source = sourceRow + x * 4;
                pixels[targetRow + x] = new Color32(
                    bgraBytes[source + 2],
                    bgraBytes[source + 1],
                    bgraBytes[source],
                    bgraBytes[source + 3]);
            }
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static void RetainLibrary()
    {
        if (!libraryInitialized)
        {
            FPDF_InitLibrary();
            libraryInitialized = true;
        }

        openDocumentCount++;
    }

    private static void ReleaseLibrary()
    {
        if (openDocumentCount > 0)
        {
            openDocumentCount--;
        }

        if (openDocumentCount == 0 && libraryInitialized)
        {
            FPDF_DestroyLibrary();
            libraryInitialized = false;
        }
    }

    private static string DescribePdfiumError(uint errorCode)
    {
        switch (errorCode)
        {
            case 0:
                return "No detailed error was reported.";
            case 1:
                return "Unknown PDF error.";
            case 2:
                return "The file could not be read.";
            case 3:
                return "The PDF format is invalid or unsupported.";
            case 4:
                return "The PDF is password protected.";
            case 5:
                return "The PDF security settings are unsupported.";
            case 6:
                return "A page could not be loaded.";
            default:
                return $"PDFium error code {errorCode}.";
        }
    }

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDF_InitLibrary();

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDF_DestroyLibrary();

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr FPDF_LoadMemDocument(IntPtr dataBuffer, int size, [MarshalAs(UnmanagedType.LPStr)] string password);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern uint FPDF_GetLastError();

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern int FPDF_GetPageCount(IntPtr document);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr FPDF_LoadPage(IntPtr document, int pageIndex);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern float FPDF_GetPageWidthF(IntPtr page);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern float FPDF_GetPageHeightF(IntPtr page);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr firstScan, int stride);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDFBitmap_FillRect(IntPtr bitmap, int left, int top, int width, int height, uint color);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int startX, int startY, int sizeX, int sizeY, int rotate, int flags);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDFBitmap_Destroy(IntPtr bitmap);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDF_ClosePage(IntPtr page);

    [DllImport(PdfiumLibraryName, CallingConvention = CallingConvention.StdCall)]
    private static extern void FPDF_CloseDocument(IntPtr document);
#else
    public static bool IsSupported => false;

    public int PageCount => 0;

    public static bool TryLoad(byte[] pdfBytes, out MailboxPdfPreviewDocument document, out string error)
    {
        document = null;
        error = "PDF preview is only available in Windows x64 and WebGL builds.";
        return false;
    }

    public bool TryRenderPage(int pageIndex, float zoom, out Texture2D texture, out string error)
    {
        texture = null;
        error = "PDF preview is only available in Windows x64 and WebGL builds.";
        return false;
    }

    public void Dispose()
    {
    }
#endif
}
