using System;
using Topshelf;

namespace MyOrdersAppService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<MyOrdersAppServiceHost>(s =>
                {
                    s.ConstructUsing(name => new MyOrdersAppServiceHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsNetworkService();

                x.SetDescription("MyOrders App Service");
                x.SetDisplayName("MyOrders App Service");
                x.SetServiceName("MyOrdersAppService");
            });

            Console.ReadLine();
        }
    }
}
