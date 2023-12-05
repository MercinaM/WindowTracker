using System;
using System.Runtime.InteropServices;

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8601 // Possible null reference assignment.

partial class Program
{
    static void Main()
    {
        WindowActivityLogger logger = new WindowActivityLogger();
        Timer timer = new Timer(logger.logActiveWindow, null, 0, 1000);

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}

public class WindowActivityLogger
{
    string windowName = "";
    string processname = "";
    DateTime intervalStartTime = DateTime.Now;
    List<ActiveWindowEvent> windowEvents = new();

    public WindowActivityLogger()
    {
        #pragma warning disable CS8622
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
    }
    
    public void logActiveWindow(object state)
    {
        var activeWindow = Win32Proxy.GetActiveWindow();

        if (activeWindow == null)
        {
            return;
        }

        if (activeWindow.WindowName != null && activeWindow.WindowName != windowName || activeWindow.ProcessName != processname)
        {
            windowName = activeWindow.WindowName;
            processname = activeWindow.ProcessName;
            DateTime windowActivatedTime = DateTime.Now;

            Console.WriteLine($"{windowName} ({processname}) ({windowActivatedTime.ToLocalTime()})");

            ActiveWindowEvent currentWindow = new()
            {
                title = windowName,
                application = processname,
                openedTime = DateTime.Now
            };

            if (windowEvents.Count() > 0)
            {
                ActiveWindowEvent previousActiveWindow = windowEvents.Last();
                previousActiveWindow.closedTime = currentWindow.openedTime;
                previousActiveWindow.duration = Math.Abs((previousActiveWindow.openedTime - previousActiveWindow.closedTime).TotalSeconds);
            }

            windowEvents.Add(currentWindow);
            Console.WriteLine($"List Size: {windowEvents.Count}");

            if ((DateTime.Now - intervalStartTime).TotalHours >= 1)
            {
                Console.WriteLine("Writing Hourly Log");
                writeLog();
                intervalStartTime = DateTime.Now;
            }
        }
    }

    void writeLog()
    {
        //Close the last Window Event
        ActiveWindowEvent previousActiveWindow = windowEvents.Last();
        previousActiveWindow.closedTime = DateTime.Now;
        previousActiveWindow.duration = Math.Abs((previousActiveWindow.openedTime - previousActiveWindow.closedTime).TotalSeconds);

        // Set a variable to the Documents path.
        string docPath =
          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Activity Logs";

        // Write the string array to a new file named "WriteLines.txt".
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, $"activity-log-{DateTime.Now.ToString("MM-dd-yyyy")}.csv")))
        {
            outputFile.WriteLine($"\"WINDOW TITLE\",\"APPLICATION\",\"OPENED\",\"CLOSED\",\"DURATION\"");

            foreach (ActiveWindowEvent windowEvent in windowEvents)
            {
                outputFile.WriteLine($"\"{windowEvent.title}\",\"{windowEvent.application}\",\"{windowEvent.openedTime.ToLocalTime()}\",\"{windowEvent.closedTime.ToLocalTime()}\",\"{(int)Math.Round(windowEvent.duration, 0, MidpointRounding.AwayFromZero)}\",\"{windowEvent.duration}\"");
            }
        }
    }

    // Event handler for AppDomain.CurrentDomain.ProcessExit
    void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("Process is exiting. Performing cleanup...");
        writeLog();
        // Perform cleanup or finalization logic
    }
    // Event handler for Console.CancelKeyPress
    void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Ctrl+C detected. Cleaning up and exiting...");
        writeLog();
        // Perform cleanup or other necessary tasks before exiting

        // Set e.Cancel to true to prevent the default behavior (application termination)
        //e.Cancel = true;
    }
}

class ActiveWindowEvent
{
    public String? title;
    public String? application;
    public DateTime openedTime;
    public DateTime closedTime;
    public double duration;
}
