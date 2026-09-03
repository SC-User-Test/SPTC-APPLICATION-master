using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Provides a lightweight HTTP health check endpoint for containerization readiness.
    /// Listens on the port specified by the HEALTH_CHECK_PORT environment variable (default: 8080).
    /// Responds to GET /health with a JSON status payload.
    /// </summary>
    public class HealthCheckService
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private volatile bool _running;

        private static readonly string Port =
            Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT") ?? "8080";

        public void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{Port}/health/");
                _listener.Start();
                _running = true;

                _listenerThread = new Thread(Listen)
                {
                    IsBackground = true,
                    Name = "HealthCheckListener"
                };
                _listenerThread.Start();
            }
            catch (Exception ex)
            {
                // Health check startup failure should not crash the application
                System.Diagnostics.Debug.WriteLine($"[HealthCheckService] Failed to start: {ex.Message}");
            }
        }

        public void Stop()
        {
            _running = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HealthCheckService] Failed to stop: {ex.Message}");
            }
        }

        private void Listen()
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
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[HealthCheckService] Listener error: {ex.Message}");
                }
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" &&
                    request.Url.AbsolutePath.TrimEnd('/').Equals("/health", StringComparison.OrdinalIgnoreCase))
                {
                    string json = "{\"status\":\"UP\",\"application\":\"SPTC-APPLICATION\",\"timestamp\":\"" +
                                  DateTime.UtcNow.ToString("o") + "\"}";

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
                System.Diagnostics.Debug.WriteLine($"[HealthCheckService] Request handling error: {ex.Message}");
            }
        }
    }
}
