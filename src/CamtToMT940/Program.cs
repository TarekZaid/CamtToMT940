using System;
using System.IO;
using System.Windows.Forms;

namespace CamtToMT940
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Exception-Logging einrichten
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                try
                {
                    var logPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "CamtToMT940_Error.log");
                    File.AppendAllText(logPath,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {e.Exception.GetType().FullName}\n" +
                        $"Message: {e.Exception.Message}\n" +
                        $"StackTrace:\n{e.Exception.StackTrace}\n" +
                        "--------------------------------------------------\n");
                }
                catch { }
            };

            Application.ThreadException += (sender, e) =>
            {
                try
                {
                    var logPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "CamtToMT940_Error.log");
                    File.AppendAllText(logPath,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED: {e.Exception.GetType().FullName}\n" +
                        $"Message: {e.Exception.Message}\n" +
                        $"StackTrace:\n{e.Exception.StackTrace}\n" +
                        "--------------------------------------------------\n");
                }
                catch { }
                MessageBox.Show(
                    $"Ein unerwarteter Fehler ist aufgetreten:\n\n{e.Exception.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}