using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

namespace ClientWebSocketStudy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatConsumerController : ControllerBase
    {

        private readonly ILogger<ChatConsumerController> _logger;
        private readonly IConfiguration _configuration;
        public ChatConsumerController(ILogger<ChatConsumerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensagem([FromBody] ChatMessage chatMessage)
        {
            try
            {
                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(_configuration.GetValue<string>("ChatEndPoint")), CancellationToken.None);
                // Convertendo o objeto ChatMessage em JSON
                string jsonMessage = JsonSerializer.Serialize(chatMessage);
                // Convertendo a mensagem JSON em bytes
                byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);
                // Enviando a mensagem por WebSocket
                await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                // Criando um buffer para receber a resposta do WebSocket
                byte[] buffer = new byte[1024];
                using (var memoryStream = new MemoryStream())
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        // Recebendo a resposta do WebSocket de forma assíncrona
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        // Escrevendo os dados recebidos no MemoryStream
                        await memoryStream.WriteAsync(buffer, 0, result.Count, CancellationToken.None);
                    } while (!result.EndOfMessage);
                    // Convertendo os dados recebidos de volta para uma string
                    string resposta = Encoding.UTF8.GetString(memoryStream.ToArray());

                    // Você pode fazer algo com a resposta aqui, por exemplo, retorná-la como parte da resposta HTTP
                    return Ok(resposta);
                }
            }
            catch (Exception ex)
            {
                // Tratamento de erros
                return StatusCode(500, $"Ocorreu um erro: {ex.Message}");
            }
        }


    }
}
