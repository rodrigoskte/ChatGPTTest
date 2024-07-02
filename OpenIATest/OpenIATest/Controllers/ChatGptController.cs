using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIATest.Settings;
using System.Text;

namespace OpenIATest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatGptController : ControllerBase
    {
        private readonly ChatGPTSettings _chatGPTSettings;

        public ChatGptController(IOptions<ChatGPTSettings> chatGPTSettings)
        {
            _chatGPTSettings = chatGPTSettings.Value;
        }

        [HttpPost]
        public async Task<ActionResult> UseChatGPTWithHttp(string query)
        {
            try
            {
                var requestBody = RequestBody(query);

                using (var httpClient = new HttpClient())
                {
                    // Adiciona a chave de API no cabeçalho de autorização
                    httpClient.DefaultRequestHeaders.Add(
                        $"Authorization", 
                        $"Bearer {_chatGPTSettings.Secret}");

                    // Envia a solicitação POST para a API do OpenAI
                    var response = await httpClient.PostAsync(
                        _chatGPTSettings.EndpointChatGPT,
                        new StringContent(
                            requestBody,
                            Encoding.UTF8,
                            "application/json"));

                    // Verifica se a solicitação foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<dynamic>(responseBody);

                        // Extrai o conteúdo da resposta
                        var content = responseObject.choices[0].message.content;

                        // Retorna o conteúdo da resposta como um resultado OK
                        return Ok(content);
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        return StatusCode(400, $"Algo de errado não está certo \n{response.StatusCode}\n{errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao usar o UseChatGPTWithHttp");
            }
        }
        private string? RequestBody(string query)
        {
            var requestBody = new
            {
                model = _chatGPTSettings.Model, // modelo a ser utilizado 
                messages = new[]
                {
                        new
                        {
                            role = "user",
                            content = query
                        }
                    },
                temperature = 0.7
            };
            return JsonConvert.SerializeObject(requestBody);
        }
    }
}
