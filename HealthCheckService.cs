using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Lightweight HTTP health check endpoint for container liveness/readiness probes.
    /// Listens on the port specified by the HEALTH_CHECK_PORT environment variable (default: 8080).
    /// Responds to GET /health with HTTP 200 and a JSON status payload.
    /// </summary>
    public class HealthCheckService
    {
        private readonly HttpListener _listener;
        private Thread _listenerThread;
        private volatile bool _running;

        private static readonly string Port =
            Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT") ?? "8080";

        public HealthCheckService()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{Port}/health/");
        }

        public void Start()
        {
            try
            {
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
                // Log but do not crash the application if health check cannot start
                Objects.EventLogger.Post($"HealthCheck :: Failed to start health check service: {ex.Message}");
            }
        }

        public void Stop()
        {
            _running = false;
            try
            {
                _listener.Stop();
            }
            catch { /* ignore */ }
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
                    Objects.EventLogger.Post($"HealthCheck :: Listener error: {ex.Message}");
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
                    string json = "{\"status\":\"UP\",\"application\":\"SPTC-APPLICATION\"}";
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
                Objects.EventLogger.Post($"HealthCheck :: Request handling error: {ex.Message}");
            }
        }
    }
}
