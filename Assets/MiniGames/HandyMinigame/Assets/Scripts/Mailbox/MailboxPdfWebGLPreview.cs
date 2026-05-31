using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Opens the browser-side PDF.js preview in WebGL builds.
public static class MailboxPdfWebGLPreview
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void MailboxPdfPreview_Open(string title, byte[] pdfBytes, int length, string streamingAssetsPath);

    public static bool Open(string title, byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return false;
        }

        try
        {
            MailboxPdfPreview_Open(FallbackTitle(title), pdfBytes, pdfBytes.Length, Application.streamingAssetsPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"MailboxPdfWebGLPreview: Could not open WebGL PDF preview. {exception.Message}");
            return false;
        }
    }
#else
    public static bool Open(string title, byte[] pdfBytes)
    {
        return false;
    }
#endif

    private static string FallbackTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title) ? "PDF attachment" : title;
    }
}
