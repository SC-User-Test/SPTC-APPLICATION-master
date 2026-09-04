using System.Windows;
using SPTC_APPLICATION.Objects;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private int openWindowCount = 0;
        private readonly HealthCheckService _healthCheckService = new HealthCheckService();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _healthCheckService.Start();
            IncrementOpenWindowCount();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            openWindowCount--;

            if (openWindowCount <= 0)
            {
                EventLogger.Post("Main :: Application Closed");
                AppState.SaveToJson();
                _healthCheckService.Stop();
            }
        }
        public void IncrementOpenWindowCount()
        {
            openWindowCount++;
        }

    }
}
