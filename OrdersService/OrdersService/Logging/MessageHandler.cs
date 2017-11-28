using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrdersService.Logging
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corrId = $"{DateTime.Now.Ticks}{Thread.CurrentThread.ManagedThreadId}";
            var requestInfo = $"{request.Method} {request.RequestUri}";

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncomingMessageAsync(corrId, requestInfo, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage = new byte[0];

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    responseMessage = await response.Content.ReadAsByteArrayAsync();
                }
            }
            else
            {
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);
            }

            await OutgoingMessageAsync(corrId, requestInfo, responseMessage);

            return response;
        }


        protected abstract Task IncomingMessageAsync(string correlationId, string requestInfo, byte[] message);
        protected abstract Task OutgoingMessageAsync(string correlationId, string responseInfo, byte[] message);
    }
}
