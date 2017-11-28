using System;
using Microsoft.Owin.Hosting;
using MyOrdersAppService.Properties;

namespace MyOrdersAppService
{
    public class MyOrdersAppServiceHost
    {
        private static IDisposable _server;

        public void Start()
        {
            _server = WebApp.Start<Startup>(Settings.Default.SelfHostBaseUrl);
        }

        public void Stop()
        {
            _server?.Dispose();
        }
    }
}
