using Newtonsoft.Json;
using System.Net;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;

namespace WebSocketStudy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            var app = builder.Build();
            app.UseWebSockets();
            app.Map("/chat/",async (context) => RetornarContentWebSocket(context));
            await app.RunAsync();
        }

        public static async Task RetornarContentWebSocket(HttpContext context)
        {
            if(context is null) return;
            
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                { 
                     await webSocket.SendAsync(Encoding.UTF8.GetBytes("Mensagem Recebida Com Sucesso"), WebSocketMessageType.Text,true,CancellationToken.None);
                }

            }
        }
    }

    public record ChatMessage (string Content, User UserSender , User UserTarget );
    public record User(int Id, string? Name);
    [JsonSerializable(typeof(ChatMessage[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }

}
