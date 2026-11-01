using BookWiki.Core.Agents.SDK;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Experiments
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //var agent = new Agent(new Kassandra().CharacterDescription);
            //agent.AddTool(new Calculator());
            //agent.Answer.Subscribe(x => {  Console.WriteLine($"{(x.Answer == false ? "я:" : "")}" + x.Message); });
            //agent.Log.Subscribe(x => 
            //{  Console.WriteLine(
            //    $"{x.Category}: {x.Message}"
            //    ); 
            //});
            //await agent.Ask("Посчитаешь мне 3+5? Я знаю, это просто, но ты, пожалуйста, заиспользуй калькулятор, просто для интереса.");

            var agent = new Agent(new Description()
            {
                Identity = new Sairi(),
                Traits = new IAgentTrait[]
                {
                    new JustSpeakNativelyV2(),
                    new BahaveNatively(),
                    new NoLongMonologues()
                },
                Skills = new[]
                {
                    new NameGeneration()
                }
            });
            agent.AddTool(new Calculator());
            agent.Answer.Subscribe(x => { Console.WriteLine($"{(x.Answer == false ? "я:" : "")}" + x.Message); });
            agent.Log.Subscribe(x =>
            {
                Console.WriteLine(
                $"{x.Category}: {x.Message}"
                );
            });

            await agent.Ask("давай знакомиться, я хочу дружить");

            //Console.WriteLine(await new Agent().Run());

            //Console.WriteLine(await new AgentDraft().Run());

            Console.ReadKey();
        }
    }

    public record ChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "gemma4:e4b"; // gemma4:e4b "gemma4-gpu"

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

    public record Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public record ChatRequest2(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<Message> Messages,
        [property: JsonPropertyName("tools")] List<Tool>? Tools = null
    );

    public record Message2(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("tool_calls")] List<ToolCall>? ToolCalls = null,
        [property: JsonPropertyName("tool_call_id")] string? ToolCallId = null
    );

    public record Tool(
        [property: JsonPropertyName("type")] string Type = "function",
        [property: JsonPropertyName("function")] FunctionDefinition? Function = null
    );

    public record FunctionDefinition(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("parameters")] object Parameters
    );

    public record ToolCall(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("function")] ToolFunction Function
    );

    public record ToolFunction(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("arguments")] string Arguments // JSON-строка параметров
    );

    public class Model
    {
        public string model;
        public string input;
        public bool store;
    }
}
