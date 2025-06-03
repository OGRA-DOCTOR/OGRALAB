using System;
using System.IO;

namespace OGRALAB
{
    public static class ErrorLogger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OGRALAB_ErrorLog.txt");

        public static void Log(Exception ex, string? context = null)
        {
            try
            {
                using var sw = new StreamWriter(LogFilePath, true);
                sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {(context != null ? "[" + context + "] " : "")}Exception: {ex.Message}");
                sw.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    sw.WriteLine($"InnerException: {ex.InnerException.Message}");
                    sw.WriteLine(ex.InnerException.StackTrace);
                }
                sw.WriteLine("--------------------------------------------------");
            }
            catch { /* Ignore logging errors */ }
        }
    }
}
