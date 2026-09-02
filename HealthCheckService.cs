using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Lightweight HTTP health check endpoint for containerization support.
    /// Listens on the port specified by the HEALTH_CHECK_PORT environment variable
    /// (default: 8080) and responds to GET /health requests with a JSON status payload.
    /// </summary>
    public static class HealthCheckService
    {
        private static HttpListener? _listener;
        private static Thread? _listenerThread;
        private static volatile bool _running = false;

        private static readonly string Port =
            Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT") ?? "8080";

        /// <summary>
        /// Starts the health check HTTP listener in a background thread.
        /// Call this from App.xaml.cs during application startup.
        /// </summary>
        public static void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{Port}/health/");
                _listener.Start();
                _running = true;

                _listenerThread = new Thread(ListenLoop)
                {
                    IsBackground = true,
                    Name = "HealthCheckListener"
                };
                _listenerThread.Start();
            }
            catch (Exception ex)
            {
                // Non-fatal: log and continue — health check failure must not crash the app
                EventLogger.Post($"HealthCheckService :: Failed to start on port {Port}: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the health check HTTP listener gracefully.
        /// Call this from App.xaml.cs during application exit.
        /// </summary>
        public static void Stop()
        {
            _running = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception ex)
            {
                EventLogger.Post($"HealthCheckService :: Error stopping listener: {ex.Message}");
            }
        }

        private static void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped — exit loop cleanly
                    break;
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"HealthCheckService :: Listener error: {ex.Message}");
                }
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)
                    && request.Url.AbsolutePath.TrimEnd('/').Equals("/health", StringComparison.OrdinalIgnoreCase))
                {
                    string json = $"{{\"status\":\"UP\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}";
                    byte[] buffer = Encoding.UTF8.GetBytes(json);

                    response.StatusCode = 200;
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = 404;
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                EventLogger.Post($"HealthCheckService :: Request handling error: {ex.Message}");
            }
        }
    }
}
