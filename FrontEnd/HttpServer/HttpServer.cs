using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace FrontEnd.HttpServer
{
    internal class HttpServer
    {
        private readonly HttpListener _listener = new();
        private BlockingCollection<string>? _logSink;

        internal void Start(IPAddress address, int port, BlockingCollection<string> logSink)
        {
            _logSink = logSink;
            _listener.Prefixes.Add($"http://{address}:{port}/");
            _listener.Start();
            _logSink.TryAdd($"Listening for HTTP requests on http://{address}:{port}/");
            Receive();
        }

        internal void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(ListenerCallback, _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (!_listener.IsListening) return;

            try
            {
                HttpListenerContext context = _listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                _ = _logSink?.TryAdd($"{request.HttpMethod} {request.Url}");

                string body = ProcessPost(context);
                _ = _logSink?.TryAdd($"Body: {body}");

                Receive();
            }
            catch (HttpListenerException e)
            {
                _ = _logSink?.TryAdd(e.Message);
            }
        }

        private static string ProcessPost(HttpListenerContext context)
        {
            // Get the data from the HTTP stream
            string body = new StreamReader(context.Request.InputStream).ReadToEnd();

            byte[] b = "ACK"u8.ToArray();
            context.Response.StatusCode = 200;
            context.Response.KeepAlive = false;
            context.Response.ContentLength64 = b.Length;

            Stream output = context.Response.OutputStream;
            output.Write(b, 0, b.Length);
            context.Response.Close();
            return body;
        }
    }
}