using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace OrdersService.Logging
{
    public class MessageLoggingHandler : MessageHandler
    {
        protected override async Task IncomingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                Log.Information($"{correlationId} - Request: {requestInfo}\r\n{Encoding.UTF8.GetString(message)}");
            });
        }

        protected override async Task OutgoingMessageAsync(string correlationId, string responseInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                Log.Information($"{correlationId} - Response: {responseInfo}\r\n{Encoding.UTF8.GetString(message)}");
            });
        }
    }
}