using System;
using System.Runtime.InteropServices;

var windowName = "";
var processname = "";

while (true) {
    var activeWindow = Win32Proxy.GetActiveWindow();

    if (activeWindow == null) {
        continue;
    }

    if (activeWindow.WindowName != windowName
        || activeWindow.ProcessName != processname
    ) {
        windowName = activeWindow.WindowName;
        processname = activeWindow.ProcessName;

        Console.WriteLine($"{windowName} ({processname})");
    }

    Thread.Sleep(1000);
}