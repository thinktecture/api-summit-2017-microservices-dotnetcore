using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using EasyNetQ;
using Microsoft.AspNet.SignalR;
using OrdersService.Hubs;
using OrdersService.Properties;
using QueuingMessages;
using Serilog;
using Order = OrdersService.DTOs.Order;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private static readonly ConcurrentDictionary<Guid, Order> Datastore;

        static OrdersController()
        {
            Datastore = new ConcurrentDictionary<Guid, Order>();
        }

        [HttpGet]
        [Route]
        public List<Order> GetOrders()
        {
            try
            {
                return Datastore.Values.OrderByDescending(o => o.Created).ToList();
            }
            catch (Exception e)
            {
                string message = "We could not retrieve the list of orders.";
                Log.Error(message + $" Reason: {0}", e);

                throw new OrderServiceException(message);
            }
        }

        [HttpPost]
        [Route]
        public void AddNewOrder(Order newOrder)
        {
            var orderId = Guid.NewGuid();
            newOrder.Id = orderId;

            try
            {
                Datastore.TryAdd(orderId, newOrder);
            }
            catch (Exception e)
            {
                string message = "We could not add the new order.";
                Log.Error(message + $" Reason: {0}", e);

                throw new OrderServiceException(message);
            }

            // TODO: Retry & exception handling
            using (var bus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString))
            {
                var identity = User.Identity as ClaimsIdentity;
                var subjectId = identity?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                var message = new NewOrderMessage
                {
                    UserId = subjectId,
                    Order = Mapper.Map<QueuingMessages.Order>(newOrder)
                };

                // TODO: Exception handling
                bus.Publish(message);

                GlobalHost.ConnectionManager.GetHubContext<OrdersHub>()
                   .Clients.Group(message.UserId)
                   .orderCreated();
            }
        }
    }
}