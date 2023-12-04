using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class Win32Proxy
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

    /// <summary> Get the text for the window pointed to by hWnd </summary>
    private static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        return string.Empty;
    }

    public static Models.ActiveWindow? GetActiveWindow() {
        var activeWindow = new Models.ActiveWindow();
        uint pid = 0;
        var hwnd = GetForegroundWindow();

        if (hwnd == IntPtr.Zero) {
            return null;
        }

        activeWindow.hwnd = hwnd;
        activeWindow.WindowName = GetWindowText(hwnd);

        GetWindowThreadProcessId(hwnd, out pid);

        if (pid == 0) {
            return null;
        }

        activeWindow.pid = pid;

        try {
            var p = Process.GetProcessById((int)pid);
            activeWindow.ProcessName = p?.MainModule?.FileName ?? "";
        }
        catch {
            return null;
        }

        return activeWindow;
    }

}