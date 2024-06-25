using Serilog;
using Serilog.Configuration;

namespace SimpleApiSink
{
    public static class ApiSinkExtensions
    {
        public static LoggerConfiguration ApiSink(this LoggerSinkConfiguration loggerConfiguration, Uri url, IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new ApiSink(formatProvider, url));
        }
    }
}
