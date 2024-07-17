using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIATest.Model;
using OpenIATest.Settings;
using System.Text;

namespace OpenIATest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatGptController : ControllerBase
    {
        private readonly ChatGPTSettings _chatGPTSettings;
        private readonly ChatGPTSettingsCvp _chatGPTSettingsCvp;

        public ChatGptController(
            IOptions<ChatGPTSettings> chatGPTSettings,
            IOptions<ChatGPTSettingsCvp> chatGPTSettingsCvp)
        {
            _chatGPTSettings = chatGPTSettings.Value;
            _chatGPTSettingsCvp = chatGPTSettingsCvp.Value;
        }

        [HttpPost]
        [Route("/UseChatGPTWithHttp")]
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

        [HttpPost]
        [Route("/UseChatCvp")]
        public async Task<ActionResult> UseChatCvp([FromBody] string query)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage request = GetHeader();
                
                request.Content = BuildContent(query);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        var jsonBuilder = new StringBuilder();
                        jsonBuilder.Append("[");

                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.StartsWith("data: "))
                            {
                                if (jsonBuilder.Length > 1)
                                {
                                    jsonBuilder.Append(",");
                                }
                                jsonBuilder.Append(line.Substring(6));
                            }
                        }

                        jsonBuilder.Append("]");
                        var responseBody = jsonBuilder.ToString();
                        responseBody = responseBody.Replace(",[DONE]", "");
                        Console.WriteLine("Response Body: " + responseBody);

                        try
                        {
                            var eventResponses = JsonConvert.DeserializeObject<List<Event>>(responseBody);
                            return Ok(eventResponses);
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine("JSON Exception: " + ex.Message);
                            Console.WriteLine("Response Body: " + responseBody);
                            return BadRequest("Invalid JSON format");
                        }
                    }
                }
                else
                    return BadRequest("Failed to retrieve data from the API");
            }
        }

        private StringContent BuildContent(string query)
        {
            return new StringContent("{\r\n    \"data_sources\": [\r\n        {\r\n            \"type\": \"azure_search\",\r\n            \"parameters\": {\r\n                \"endpoint\": \"https://srch-analysis-cvp.search.windows.net\",\r\n                \"index_name\": \"vector-1718365345101\",\r\n                \"semantic_configuration\": \"vector-1718365345101-semantic-configuration\",\r\n                \"query_type\": \"semantic\",\r\n                \"fields_mapping\": {},\r\n                \"in_scope\": true,\r\n                \"role_information\": \"You are an AI assistant that helps people find information.\",\r\n                \"filter\": null,\r\n                \"strictness\": 3,\r\n                \"top_n_documents\": 5,\r\n                \"authentication\": {\r\n                    \"type\": \"api_key\",\r\n                    \"key\": \""+_chatGPTSettingsCvp.BodyKey+"\"\r\n                }\r\n            }\r\n        }\r\n    ],\r\n    \"messages\": [\r\n        {\r\n            \"role\": \"system\",\r\n            \"content\": \"You are an AI assistant that helps people find information.\"\r\n        },\r\n        {\r\n            \"role\": \"user\",\r\n            \"content\": \""+ query + "\"\r\n        }\r\n    ],\r\n    \"deployment\": \"CVP-ChatGPT-35-16k\",\r\n    \"temperature\": 0,\r\n    \"top_p\": 1,\r\n    \"max_tokens\": 800,\r\n    \"stop\": null,\r\n    \"stream\": true\r\n}", null, "text/plain");
        }

        private HttpRequestMessage GetHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _chatGPTSettingsCvp.EndpointChatGPT);
            request.Headers.Add("accept", "*/*");
            request.Headers.Add("accept-language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7,es;q=0.6,la;q=0.5");
            request.Headers.Add("api-key", _chatGPTSettingsCvp.Secret);
            request.Headers.Add("dnt", "1");
            request.Headers.Add("origin", "http://localhost:3000");
            request.Headers.Add("priority", "u=1, i");
            request.Headers.Add("referer", "http://localhost:3000/");
            request.Headers.Add("sec-ch-ua", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Google Chrome\";v=\"126\"");
            request.Headers.Add("sec-ch-ua-mobile", "?1");
            request.Headers.Add("sec-ch-ua-platform", "\"Android\"");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "cross-site");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Mobile Safari/537.36");
            request.Headers.Add("x-ms-useragent", "AzureOpenAI.Studio/1.0.02743.1994");
            return request;
        }
    }
}
