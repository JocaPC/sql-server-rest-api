using Common.Logging;
using System;
using Microsoft.Extensions.Logging;

namespace MsSql.RestApi.Util
{
    public class CommonILogAdapter4ExtensionILogger: ILog
    {
        ILogger logger = null;

        public CommonILogAdapter4ExtensionILogger(ILogger logger)
        {
            this.logger = logger;
        }

        public bool IsTraceEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace);

        public bool IsDebugEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug);

        public bool IsErrorEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error);

        public bool IsFatalEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Critical);

        public bool IsInfoEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information);

        public bool IsWarnEnabled => logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning);

        public IVariablesContext GlobalVariablesContext => throw new NotImplementedException();

        public IVariablesContext ThreadVariablesContext => throw new NotImplementedException();

        public INestedVariablesContext NestedThreadVariablesContext => throw new NotImplementedException();

        public void Debug(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, params object[] args)
        {
            logger.LogDebug(format, args);
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Error(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            logger.LogError(format, args);
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Critical, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Critical, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }
        public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, params object[] args)
        {
            logger.LogError(format, args);
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Info(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, params object[] args)
        {
            logger.LogInformation(format, args);
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Trace, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Trace(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Trace, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(string format, params object[] args)
        {
            logger.LogTrace(format, args);
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, 0, message, null, (msg, ex) => msg.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, 0, message, exception, (msg, ex) => msg.ToString() + '\n' + ex.Message);
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            throw new NotImplementedException();
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, params object[] args)
        {
            logger.LogWarning(format, args);
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
