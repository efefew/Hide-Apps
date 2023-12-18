using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

internal class HideApps
{
    #region Fields

    private const int SW_HIDE = 0;
    private const int MILLISECONDS_TIMEOUT = 100;
    private const string AUTORUN_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private static readonly string? APPLICATION_NAME = Assembly.GetEntryAssembly()?.GetName().Name;
    private static readonly string? APPLICATION_PATH = Assembly.GetEntryAssembly()?.Location.Replace(".dll", "");

    #endregion Fields

    #region Methods

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static void Main()
    {
        AutoRun();

        List<string> hideProcesses;
        using StreamReader reader = new($"{APPLICATION_PATH}.txt");
        hideProcesses = reader.ReadToEnd().Split("\r\n").ToList();

        while (hideProcesses.Count > 0)
        {
            Thread.Sleep(MILLISECONDS_TIMEOUT);
            foreach (Process process in Process.GetProcesses())
            {
                if (hideProcesses.Contains(process.MainWindowTitle))
                {
                    _ = ShowWindow(process.MainWindowHandle, SW_HIDE);
                    _ = hideProcesses.Remove(process.MainWindowTitle);
                    if (hideProcesses.Count == 0)
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Подключение автозагрузки
    /// </summary>
    /// <param name="autorun">подключить?</param>
    public static void AutoRun(bool autorun = true)
    {
        using RegistryKey? registry = Registry.CurrentUser.OpenSubKey(AUTORUN_PATH, true);
        if (registry == null)
            return;
        if (autorun)
        {
            if (!registry.GetValueNames().Contains(APPLICATION_NAME))
                registry.SetValue(APPLICATION_NAME, $"\"{APPLICATION_PATH}.exe\"");
        }
        else
        {
            if (APPLICATION_NAME == null)
                return;
            registry.DeleteValue(APPLICATION_NAME, false);
        }
    }

    #endregion Methods
}