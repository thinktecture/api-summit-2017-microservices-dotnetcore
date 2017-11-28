using System;
using System.Threading.Tasks;
using AutoMapper;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using OrdersService.Discovery;
using OrdersService.Hubs;
using OrdersService.Properties;
using Polly;
using QueuingMessages;
using Serilog;
using Order = OrdersService.DTOs.Order;
using OrderItem = OrdersService.DTOs.OrderItem;

namespace OrdersService
{
    class OrdersServiceHost
    {
        private static ServiceRegistry _serviceRegistry;
        private static RegistryInformation _registryInformation;
        private static IDisposable _server;
        private static IBus _bus;

        public void Start()
        {
            var hostBaseUrl = Settings.Default.SelfHostBaseUrl;
            var webApiBaseUrl = Settings.Default.WebApiBaseUrl;
            var healthUrl = Settings.Default.WebApiHealthUrl;

            ConfigureLogging();
            WireAppDomainHandlers();
            InitializeMapper();
            SetupQueues();
            ListenOnQueues();
            StartWebApi(hostBaseUrl, webApiBaseUrl, healthUrl);
        }

        public void Stop()
        {
            _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id).Wait();

            _bus?.Dispose();
            _server?.Dispose();
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich
                .FromLogContext()
                //.WriteTo.LiterateConsole()
                .WriteTo.Seq(Settings.Default.SeqBaseUrl)
                .CreateLogger();
        }

        private static void SetupQueues()
        {
            var retryPolicy = Policy.Handle<TimeoutException>()
                .Retry(3, (exception, retryCount) =>
                {
                    Log.Warning($"Tried to connect to RMQ - {0} time(s) - reason: {1}", retryCount, exception);
                });

            try
            {
                retryPolicy.Execute(() =>
                {
                    using (var advancedBus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString).Advanced)
                    {
                        var newOrderQueue = advancedBus.QueueDeclare("QueuingMessages.NewOrderMessage:QueuingMessages_shipping");
                        var newOrderExchange = advancedBus.ExchangeDeclare("QueuingMessages.NewOrderMessage:QueuingMessages", ExchangeType.Topic);
                        advancedBus.Bind(newOrderExchange, newOrderQueue, String.Empty);

                        var shippingCreatedQueue = advancedBus.QueueDeclare("QueuingMessages.ShippingCreatedMessage:QueuingMessages_shipping");
                        var shippingCreatedExchange = advancedBus.ExchangeDeclare("QueuingMessages.ShippingCreatedMessage:QueuingMessages", ExchangeType.Topic);
                        advancedBus.Bind(shippingCreatedExchange, shippingCreatedQueue, String.Empty);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error($"Could not connect to RMQ - reason: {0}", e);
                throw;
            }
        }

        private void ListenOnQueues()
        {
            _bus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString);
            
            _bus.Subscribe<ShippingCreatedMessage>("shipping", msg =>
            {
                Log.Information("###Shipping created: " + msg.Created + " for " + msg.OrderId);

                GlobalHost.ConnectionManager.GetHubContext<OrdersHub>()
                   .Clients.Group(msg.UserId)
                   .shippingCreated(msg.OrderId);
            });
        }

        private static void WireAppDomainHandlers()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            currentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<OrderItem, QueuingMessages.OrderItem>();
                cfg.CreateMap<Order, QueuingMessages.Order>()
                    .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            });
            Mapper.AssertConfigurationIsValid();
        }

        private static void StartWebApi(string hostBaseUrl, string webApiBaseUrl, string healthUrl)
        {
            var registryHost = new ConsulRegistryHost();
            _serviceRegistry = new ServiceRegistry(registryHost);

            Task.Run(async () =>
            {
                _registryInformation = await _serviceRegistry.AddTenantAsync(new CustomWebApiRegistryTenant(new Uri(webApiBaseUrl)), "orders", "0.0.2", new Uri(healthUrl));

                _server = WebApp.Start<Startup>(hostBaseUrl);
                Log.Information("Orders Service running - listening at {0} ...", hostBaseUrl);
            }).GetAwaiter().GetResult();
        }

        private static async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (_registryInformation != null)
            {
                await _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id);
            }
        }

        private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_registryInformation != null)
            {
                await _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id);
            }
        }
    }
}