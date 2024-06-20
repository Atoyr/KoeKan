using System.IO;

using Microsoft.Extensions.Logging;

using Medoz.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// </summary>
public class LoggerUtility 
{
    private static ILoggerFactory? _loggerFactory;

    public static ILoggerFactory GetLoggerFactory(FileLoggerSettings? settings = null)
    {
        if (settings is null && _loggerFactory is null)
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName, "config");
            settings = new FileLoggerSettings(folderPath){ FileName = "log.txt"};
        }
        else if (settings is null)
        {
            return _loggerFactory!;
        }
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFile(settings);
        });

        return _loggerFactory;
    }


}