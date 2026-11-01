using System;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Daven.AgentsSdk
{
    

        public class Agent
        {

            public async Task<string> Run()
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var msg = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/v1/chat/completions");
                    msg.Content = new StringContent(
                        JsonConvert.SerializeObject(
                            new ChatRequest()
                            {
                                Messages = new List<Message>()
                                {
                                new Message()
                                {
                                    Role = "user",
                                    Content = "How are you doing today?"
                                }
                                }
                            }
                        ),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var result = await client.SendAsync(msg);

                    var statusCode = result.StatusCode;
                    var contentResult = await result.Content.ReadAsStringAsync();

                    return contentResult;
                }
            }
        }

        public class ChatRequest
        {
            [JsonProperty("model")]
            public string Model { get; set; } = "gemma4:12b";

            [JsonProperty("messages")]
            public List<Message> Messages { get; set; }

            [JsonProperty("stop")]
            public List<string> Stops { get; set; }

            [JsonProperty("temperature")]
            public float Temperature { get; set; } = 1;

            [JsonProperty("top_p")]
            public float TopP { get; set; } = 0.95f;

            [JsonProperty("top_k")]
            public float TopK { get; set; } = 64f;

            [JsonProperty("num_predict")]
            public int Predict { get; set; } = 100;
        }

        public class Message
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        public class Model
        {
            public string model;
            public string input;
            public bool store;
        }

}
