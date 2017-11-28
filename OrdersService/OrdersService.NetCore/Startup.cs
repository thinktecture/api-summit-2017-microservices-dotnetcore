﻿using AutoMapper;
using Consul;
using EasyNetQ;
using EasyNetQ.Topology;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrdersService.Authentication;
using OrdersService.Discovery;
using OrdersService.Hubs;
using Polly;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace OrdersService
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }

        private IBus _bus;
        private IHubContext<OrdersHub> _ordersHubContext;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("appSettings"));
            services.Configure<ConsulConfig>(Configuration.GetSection("consulConfig"));

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration["consulConfig:address"];
                consulConfig.Address = new Uri(address);
            }));
            services.AddSingleton<IHostedService, ConsulHostedService>();

            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddJsonFormatters()
                .AddAuthorization();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Orders API", Version = "v1" });
            });

            services.AddSignalR();

            services.AddCors();
            
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["appSettings:idSrvBaseUrl"];
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "ordersapi";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                appLifetime.StopApplication();
            // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
            eventArgs.Cancel = true;
            };

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });

            app.UseQueryStringAuthorization();
            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API V1");
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<OrdersHub>("ordersHub");
            });

            _ordersHubContext = app.ApplicationServices.GetService<IHubContext<OrdersHub>>();
        }

        private void OnStarted()
        {
            InitializeMapper();
            SetupQueues();
            ListenOnQueues();
        }

        private void OnStopping()
        {
            _bus?.Dispose();
        }

        private void OnStopped()
        {
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
                    using (var advancedBus = RabbitHutch.CreateBus(Configuration["appSettings:rabbitMqConnectionString"]).Advanced)
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
                Log.Error($"Could not connect to queuing system - reason: {0}", e);
                throw;
            }
        }

        private void ListenOnQueues()
        {
            _bus = RabbitHutch.CreateBus(Configuration["appSettings:rabbitMqConnectionString"]);

            _bus.Subscribe<QueuingMessages.ShippingCreatedMessage>("shipping", msg =>
            {
                Log.Information("###Shipping created: " + msg.Created + " for " + msg.OrderId);

                _ordersHubContext.Clients.Group(msg.UserId).InvokeAsync("shippingCreated", msg.OrderId);
            });
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DTOs.OrderItem, QueuingMessages.OrderItem>();
                cfg.CreateMap<DTOs.Order, QueuingMessages.Order>()
                    .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            });
            Mapper.AssertConfigurationIsValid();
        }
    }
}
