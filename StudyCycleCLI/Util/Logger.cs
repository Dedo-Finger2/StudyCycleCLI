using System.Globalization;
using System.Text;

namespace StudyCycleCLI.Util;

public static class Logger
{
    private static readonly string BaseMessageStructure = "[{TYPE}]\n[MESSAGE]: {MESSAGE}\n[DATE]: {DATE}\n[STACK TRACE]: {STACK TRACE}\n";
    private static readonly string UserTempFolderPath = Path.GetTempPath();
    private static readonly string LogFilePath = Path.Join(UserTempFolderPath, "StudyCycleCLILogs", "studycyclecli.log");

    private static void CreateLogFileIfNotExists()
    {
        if (Path.Exists(LogFilePath)) return;
        Directory.CreateDirectory(Path.Combine(UserTempFolderPath, "StudyCycleCLILogs"));
        
        var logFileStream = File.Create(LogFilePath);
        logFileStream.Close();
    }

    private static string FormatLogMessage(string type, string message, string? stackTrace)
    {
        var dateString = DateTime.Now;
        
        return BaseMessageStructure
            .Replace("{TYPE}", type)
            .Replace("{MESSAGE}", message)
            .Replace("{DATE}", $"{dateString.ToShortDateString()} - {dateString.ToLongTimeString()}")
            .Replace("{STACK TRACE}", stackTrace);
    }
    
    public static void Info(string message, Exception exception)
    {
        CreateLogFileIfNotExists();
        
        var logMessage = FormatLogMessage("INFO", message, exception.StackTrace);

        using var streamWriter = File.AppendText(LogFilePath);
        streamWriter.WriteLine(logMessage);
        streamWriter.Close();
    }

    public static void Error(string message, Exception exception)
    {
        CreateLogFileIfNotExists();
        
        var logMessage = FormatLogMessage("ERROR", message, exception.StackTrace);
        
        File.WriteAllText(LogFilePath, logMessage);
    }

    public static void Warning(string message, Exception exception)
    {
        CreateLogFileIfNotExists();
        
        var logMessage = FormatLogMessage("WARNING", message, exception.StackTrace);
        
        File.WriteAllText(LogFilePath, logMessage);
    }
}