using Microsoft.Extensions.Logging;
using System;

namespace Medoz.MessageTransporter;

public class ProxyLogger : ILogger
{
    public Action<LogLevel, EventId, string> WriteLog;

    public ProxyLogger(Action<LogLevel, EventId, string> writeLog)
    {
        WriteLog = writeLog;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        // スコープの開始処理（必要に応じて）
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // 特定のログレベルでログを記録するかどうかを決定
        return true; // すべてのログレベルを記録
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // ログメッセージのフォーマット
        string message = formatter(state, exception);
        
        WriteLog(logLevel, eventId, message);
    }
}