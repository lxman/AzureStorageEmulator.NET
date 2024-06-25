using System.Text;
using Serilog.Core;
using Serilog.Events;

namespace SimpleApiSink
{
    public class ApiSink(IFormatProvider? formatProvider, Uri url) : ILogEventSink
    {
        private readonly HttpClient _httpClient = new();
        private readonly TaskFactory _taskFactory = new();

        public void Emit(LogEvent logEvent)
        {
            bool result = _taskFactory.StartNew(async () =>
            {
                _ = await _httpClient.PostAsync(url.ToString(), new StringContent(logEvent.RenderMessage(formatProvider), Encoding.UTF8, "application/json"));
            }).Wait(100);
        }
    }
}
