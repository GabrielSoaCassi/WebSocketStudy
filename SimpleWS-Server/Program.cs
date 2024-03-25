using Microsoft.AspNetCore.Builder;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace SimpleWS_Server
{
    public class Program
    {
        private static ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://localhost:6969");
            var app = builder.Build();
            app.UseWebSockets();

            app.Map("/ws/{userId}", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var userId = context.Request.RouteValues["userId"].ToString();
                    var ws = await context.WebSockets.AcceptWebSocketAsync();

                    // Adiciona ou atualiza o usuário conectado no dicionário
                    _clients.AddOrUpdate(userId, ws, (key, existingSocket) => ws);

                    // Agora você pode enviar e receber mensagens usando o WebSocket
                    await ReceiveMessagesAsync(userId, ws);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });

            await app.RunAsync();
        }

        private static async Task ReceiveMessagesAsync(string userId, WebSocket socket)
        {
            var buffer = new byte[1024];
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Aqui você pode processar a mensagem recebida e direcioná-la para o destinatário correto
                await SendMessageToUserAsync(message, userId);

            } while (!result.CloseStatus.HasValue);

            // Remove o usuário do dicionário quando a conexão for fechada
            _clients.TryRemove(userId, out _);
        }

        private static async Task SendMessageToUserAsync(string message, string userId)
        {
            if (_clients.TryGetValue(userId, out var userSocket))
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await userSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, default);
            }
        }
    }
}
