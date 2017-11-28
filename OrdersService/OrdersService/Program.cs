using Topshelf;

namespace OrdersService
{
    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(x =>
            {
                x.Service<OrdersServiceHost>(s =>
                {
                    s.ConstructUsing(name => new OrdersServiceHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsNetworkService();

                x.SetDisplayName("Orders Service");
                x.SetDescription("Orders Service");
                x.SetServiceName("OrdersService");
            });
        }
    }
}
