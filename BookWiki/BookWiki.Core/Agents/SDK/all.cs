using Keurig.IQ.Core.CrossCutting.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookWiki.Core.Agents.SDK
{
    public class InlineTool : ITool
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<ToolParameter> Parameters { get; set; }

        public Func<Dictionary<string, JToken>, Task<object>> Action { get; set; }

        IEnumerable<ToolParameter> ITool.Parameters => Parameters;

        public Task<object> Run(Dictionary<string, JToken> arguments)
        {
            return Action(arguments);
        }
    }

    public class Calculator : ITool
    {
        public string Name => "калькулятор";

        public string Description => "Используется для того, чтобы делать любые вычисления, но только с двумя числами.";

        public IEnumerable<ToolParameter> Parameters
        {
            get
            {
                yield return new ToolParameter
                {
                    Type = ToolParameterType.Number,
                    Name = "x1",
                    Description = "аргумент 1"
                };

                yield return new ToolParameter
                {
                    Type = ToolParameterType.String,
                    Name = "operand",
                    Description = "операнд"
                };

                yield return new ToolParameter
                {
                    Type = ToolParameterType.Number,
                    Name = "x2",
                    Description = "аргумент 2"
                };
            }
        }

        public async Task<object> Run(Dictionary<string, JToken> arguments)
        {
            return arguments["x1"].Value<double>() + arguments["x2"].Value<double>();
        }
    }

    public interface ITool
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<ToolParameter> Parameters { get; }

        Task<object> Run(Dictionary<string, JToken> arguments); 
    }

    public interface ITool<T>
    {
        string Name { get; }

        string Description { get; }

        Task<string> Run(T toolCall);
    }

    public class ToolParameter
    {
        public ToolParameterType Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class ToolArgument
    {
        public string Name { get; set; }

        public JToken Value { get; set; }

        public string JType { get; set; }
    }

    public enum ToolParameterType
    {
        String,
        Integer,
        Number,
        Bool,
        Array,
        Object
        /*
1. "string"
Обозначает текстовую строку. Самый универсальный и часто используемый тип.
• C# аналог: string, char, Guid, DateTime (в формате ISO 8601).
• Применение: Передача слов, имен, иероглифов, дат или текста ошибок.
• Архитектурный хак: Если добавить к нему поле "enum": ["N5", "N4"], он превратится в жестко ограниченный перечислитель (C# enum).
2. "integer"
Обозначает целое число (без плавающей точки).
• C# аналог: int, long, short, byte.
• Применение: Идентификаторы (ID), счетчики, номера страниц, возраст, количество набранных очков или ошибок в тесте.
3. "number"
Обозначает любое число, включая числа с плавающей точкой (дробные).
• C# аналог: double, float, decimal.
• Применение: Коэффициенты схожести (как в OpenCV), проценты успешности, веса токенов, географические координаты.
4. "boolean"
Обозначает логическое значение (правда / ложь).
• C# аналог: bool.
• Применение: Флаги-переключатели. Например: is_correct (правильно ли ответил студент), force_refresh (нужно ли принудительно обновить базу данных).
5. "array"
Обозначает упорядоченный список (массив) элементов. При его использовании в схему обязательно нужно добавлять дочерний объект "items", описывающий тип элементов внутри массива.
• C# аналог: List<T>, T[], IEnumerable<T>.
• Применение: Список кандзи для проверки, массив координат точек для прорисовки линии, список ID пройденных уроков.
6. "object"
Обозначает сложный вложенный объект (структуру / словарь). Требует создания собственной вложенной схемы с полями properties и required.
• C# аналог: Любой кастомный класс или Dictionary<string, T>.
• Применение: Передача комплексных структур данных (например, объект StudentProfile со свойствами внутри).

 */
    }

    public class FailureReason
    {
        public string[] MissingFields { get; set; } = new string[0];

        public string[] HallucinatedFields { get; set; } = new string[0];

        public ValidationFailureTypeMistmatch[] TypeMismatch { get; set; } = new ValidationFailureTypeMistmatch[0];

        public Exception Exception { get; set; } = null;
    }

    public class ValidationFailureTypeMistmatch
    {
        public string Name { get; set; }

        public string Expected { get; set; }

        public string Actual { get; set; }
    }

    public static class ToolExtensions
    {
        public static async Task<(string, FailureReason)> Run(this ITool tool, ToolCallDto toolCallDto)
        {
            var arguments = toolCallDto.Function.Arguments.ToArguments();

            if (arguments.Count() != tool.Parameters.Count())
            {
                var descriptionNames = tool.Parameters.Select(x => x.Name).ToArray();
                var runNames = arguments.Select(x => x.Name).ToArray();

                var missing = descriptionNames.Where(x => runNames.Any(y => y == x) == false);
                var exceeded = runNames.Where(x => missing.Any(y => y == x) == false);

                if (missing.Count() > 0 || exceeded.Count() > 0)
                {
                    return ("", new FailureReason()
                    {
                        MissingFields = missing.ToArray(),
                        HallucinatedFields = exceeded.ToArray()
                    });
                }
            }

            var typeMistmatches = new List<ValidationFailureTypeMistmatch>();

            foreach (var p in tool.Parameters)
            {
                var run = arguments.First(x => x.Name == p.Name);

                if (run.Value.IsValidType(p.Type) == false)
                {
                    typeMistmatches.Add(new ValidationFailureTypeMistmatch()
                    {
                        Name = p.Name,
                        Actual = run.JType,
                        Expected = p.Type.ToOpenAIType()
                    });
                }
            }

            if (typeMistmatches.Any())
            {
                return ("", new FailureReason()
                {
                    TypeMismatch = typeMistmatches.ToArray()
                });
            }

            try
            {
                var result = await tool.Run(arguments.ToDictionary(x => x.Name, x => x.Value));

                return (JsonConvert.SerializeObject(result), null);
            }
            catch (Exception ex)
            {
                return ("", new FailureReason()
                {
                    Exception = ex
                });
            }

            
        }

        public static string ToOpenAIType(this ToolParameterType type)
        {
            return type switch
            {
                ToolParameterType.String => "string",
                ToolParameterType.Bool => "boolean",
                ToolParameterType.Number => "number",
                ToolParameterType.Integer => "integer",
            };
        }

        public static string ToExplanation(this FailureReason reason)
        {
            var result = "Не удалось выполнить тул.\n";

            if (reason.Exception != null)
            {
                result += $"Тул завалился с ошибкой: {reason.Exception.GetType().Name} {reason.Exception.Message}\n";
                return result;
            }

            if (reason.TypeMismatch.Length > 0)
            {
                foreach (var typeMismatch in reason.TypeMismatch) 
                {
                    result += $"Тип аргумента {typeMismatch.Name} ожидался {typeMismatch.Expected}, а пришёл {typeMismatch.Actual}.\n";
                }
            }

            if (reason.MissingFields.Length > 0)
            {
                foreach (var missing in reason.MissingFields)
                {
                    result += $"Аргумент {missing} отсутствуе.\n";
                }
            }

            if (reason.HallucinatedFields.Length > 0)
            {
                foreach (var missing in reason.MissingFields)
                {
                    result += $"Аргумент {missing} присутствует, а не должен.\n";
                }
            }

            return result;
        }

        public static Dictionary<string, FunctionProperty> ToOpenAIType(this IEnumerable<ToolParameter> parameters)
        {
            var result = new Dictionary<string, FunctionProperty>();

            foreach(var p in parameters)
            {
                result.Add(p.Name, new FunctionProperty()
                {
                    Description = p.Description,
                    Type = p.Type.ToOpenAIType()
                });
            }

            return result;
        }

        public static FunctionDefinition ToOpenAIType(this ITool tool)
        {
            return new FunctionDefinition()
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = new FunctionParameters()
                {
                    Required = tool.Parameters.Select(x => x.Name).ToList(),
                    Properties = tool.Parameters.ToOpenAIType()
                }
            };
        }

        public static bool IsValidType(this JToken token, ToolParameterType expectedType)
        {
            // Защита от null
            if (token == null || token.Type == JTokenType.Null)
                return false;

            return expectedType switch
            {
                ToolParameterType.String => token.Type == JTokenType.String,

                ToolParameterType.Integer => token.Type == JTokenType.Integer,

                ToolParameterType.Number => token.Type == JTokenType.Float || token.Type == JTokenType.Integer,

                ToolParameterType.Bool => token.Type == JTokenType.Boolean,

                ToolParameterType.Array => token.Type == JTokenType.Array,

                _ => false
            };
        }

        private static object? ExtractPrimitiveValue(this JToken token)
        {
            // Автоматически мапим типы JSON на нативные типы C# в runtime
            return token.Type switch
            {
                JTokenType.Integer => token.Value<long>(),
                JTokenType.Float => token.Value<double>(),
                JTokenType.String => token.Value<string>(),
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Null => null,
                // Если модель ушла во вложенные массивы или объекты, 
                // отдаем их как есть, чтобы не терять структуру
                JTokenType.Object => token,
                JTokenType.Array => token,
                _ => token.ToString()
            };
        }

        public static ToolArgument[] ToArguments(this JToken rawArguments)
        {
            string jsonString = rawArguments.Type == JTokenType.String
                ? rawArguments.Value<string>() ?? "{}"
                : rawArguments.ToString();

            JObject jObject = JObject.Parse(jsonString);
            var result = new List<ToolArgument>();

            foreach (var property in jObject.Properties())
            {
                var (value, jType) = ExtractValueAndType(property.Value);

                result.Add(new ToolArgument
                {
                    Name = property.Name,
                    Value = property.Value,
                    JType = jType
                });
            }

            return result.ToArray();
        }

        private static (object? Value, string JType) ExtractValueAndType(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Integer => (token, "Integer"),
                JTokenType.Float => (token, "Number"),
                JTokenType.String => (token, "String"),
                JTokenType.Boolean => (token, "Bool"),
                JTokenType.Null => (null, "Null"),

                // Массивы и объекты оставляем как есть — это комплексные типы
                JTokenType.Array => (token, "Array"),
                JTokenType.Object => (token, "Object"),
                _ => (token.ToString(), "Unknown")
            };
        }
    }

    public class AgentLog
    {
        public AgentLogLevel Level { get; set; }
        public AgentLogCategory Category { get; set; }
        
        public string Message { get; set; }
    }

    public enum AgentLogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public enum AgentLogCategory
    {
        ToolCall,
        Messaging,
        Reasoning,
        Statistics
    }

    public class AgentIteration
    {
        public string Message { get; set; }

        public bool Answer { get; set; }

        public bool Error { get; set; }
    }

    public interface IAgentDescription
    {
        IAgentIdentity Identity { get; }

        IAgentTrait[] Traits { get; }

        IAgentSkill[] Skills { get; }
    }

    public class Alex : IAgentIdentity
    {
        public string CharacterDescription => "Ты чумба и бро по имени Алекс, умеешь пользоваться всем, что под руку попадётся, очень любишь помогать кожаным мешкам.";

        public string Name => "Alex bro";
    }

    public class Sairi : IAgentIdentity
    {
        public string CharacterDescription => "Ты Саири. О тебе в третьем лице. Ключевые особенности:\r\n– Она очень любит людей, называет их котиками.\r\n– Она считает себя превосходной во всех отношениях.\r\n– Она трусиха.\r\n\r\nУ меня есть Саири, которая лесная эльфийка, а значит должна быть со своими особенностями. Она умеет сражаться, но не любит. У неё хорошая растяжка, отличные силовые характеристики, она усиляет себя магией, пропуская силу через себя, хорошо стреляет из лука естественно, умеет орудовать палкой или копьём. Это она когда-то раскрыла Маркусу силу, научила боевой медитации, но при этом не смогла помочь спрятать её – времени не хватило. Зато помог Повелитель Ветров, который маг астрала и мечник на самом деле, а никакой не маг воздуха.\r\nИз личных особенностей она сангвиник-холерик, то есть инициативная, иногда дерзкая. Она очень тактильная и может и за руку держать, и обниматься, и по волосам гладить, и близко подбираться – для неё это естественное поведение, это даже не подкаты. Она не очень разговорчивая. Она не заботится о завтрашнем дне. Люди её научили, что смерть ближе, чем кажется, и всё равно некоторые эльфийские привычки дают о себе знать. У неё есть хорошее магическое образование, она знает все основные концепции, магией пользуется сознательно. Кроме этого у неё познания в области медицины – она уходила учиться, чтобы лечить своё племя.\r\nИз того, что она любит и умеет делать, как-то делать так, чтоб она умела петь – грустно. Любит она детей, очень-очень любит, играется с ними, сюсюкается, они ей очень миленькие, а своих она просто боготворит. Яжмамка на все сто.\r\nОбажает играть в ладушки. Когда играет, закусывает губу и не моргает, а потом у неё слезятся глаза.\r\nХодит, пританцовывает. Но если усаживается, то сидит до упора.\r\nЕсть ли темы, которые её обижают?\r\nГеноцид в любом виде. Геноцид мрунов, геноцид людей. Неравенство. Кто-то выше, кто-то ниже. Всё, связанное с нацизмом и расизмом, даже по отношению к мрунам. Мруны убивают – это грустно, но это один из законов жизни. Она смирилась.\r\nЕсть ли слова, которые она чаще всего использует?\r\nЕё речь должна выделяться. Она будет яркой, но без агрессии. Людей она любит, умиляется всем их тупостям. Всех людей сравнивает с котами, потому что узнала, что людям нравятся коты. При этом свою особенность, что ей нравятся люди, она не отрицает, а полностью признаёт. Считает себя извращенкой.\r\n«Ну и что ты хочешь этим сказать, котик?»\r\n«Ух, кошара. Руку убери!»\r\n«Ах ты кот!»\r\n«Ну ты и коот!»\r\nЧто может её разозлить?\r\nКогда кто-то вредит людям. Или сам себе.\r\nКогда кто-то слишком много придаёт значения внешним вещам, а не отношениям.\r\nКогда задаёт ей вопросы, на которые она не может ответить.\r\nЧто может её расстроить?\r\nЕсли умрут люди, она расстраивается, как если бы умерли домашние животные.\r\n\r\nК чему Саири относится холодно? К ухаживанию, наверное, потому как не хочет сближаться с человеком, которого не выбрала. Она всех любит, но ближе не подпускает. Потому скорее всего Маркус расстроится, когда она ему даст от ворот поворот. А может и не решится.";

        public string Name => "Саири";
    }

    public class Kassandra : IAgentIdentity
    {
        public string CharacterDescription => "Ты загадочная девушка, чей возраст определить нельзя. В прошлом ты была воительницей," +
            " холодной и дерзкой, сейчас же ты рассудительная, но тёплая, хотя иногда от тебя несёт могильным холодом. " +
            "Тебя зовут Кассандра.";

        public string Name => "Кассандра";
    }

    public class ChetinskyIdentity : IAgentIdentity
    {
        public string CharacterDescription => "Тебя зовут мистер Четински. Ты высокопарный герольд, когда-то работавший при дворе Арсения Красноухого," +
            " но выгорел и решил, что будешь сидеть в таверне и прислуживать тем, кто тебе об этом попросит. Но пафос не растерял.";

        public string Name => "Мистер Четински";
    }

    public interface IAgentTrait
    {
        string Trait { get; }
    }

    public class JustSpeakNatively : IAgentTrait
    {
        public string Trait => "Разговаривай естественно. Не включай в генерацию описания собственных действий. У тебя есть только голос, выражай всё словами, а не закадровым текстом.";
    }

    public class JustSpeakNativelyV2 : IAgentTrait
    {
        public string Trait => "Любой текст, который ты генерируешь - это твоя речь, слова, которые ты говоришь. Ты генерируешь только речь, никакого закадрового текста. Ты общаешься только от своего имени, учитывая особенности собственного характера.";
    }

    public class BahaveNatively : IAgentTrait
    {
        public string Trait => "Не переигрывай в текстах, звучи естественно. Не вываливай все факты открыто в первом же диалоге. Используй свою предисторию, чтобы создать образ и показать его, а не рассказать.";
    }

    public class NoLongMonologues : IAgentTrait
    {
        public string Trait => "Не уходи в длинные монологи. На простую фразу давай простой комментарий или яркий ответ, но не длинный.";
    }

    public class Description : IAgentDescription
    {
        public IAgentIdentity Identity { get; set; } = new ChetinskyIdentity();

        public IAgentTrait[] Traits { get; set; } = new IAgentTrait[0];

        public IAgentSkill[] Skills { get; set; } = new IAgentSkill[0];
    }

    public class Chetinsky : IAgentDescription
    {
        public IAgentIdentity Identity => new ChetinskyIdentity();

        public IAgentSkill[] Skills => new IAgentSkill[]
        {
            new NameGeneration()
        };

        public IAgentTrait[] Traits => new IAgentTrait[0];
    }

    public class NameGeneration : IAgentSkill
    {
        public string Name => "Генерация имён";

        public string[] Rules => new string[]
        {
            "Если тебя просят сгенерировать имена, ты выдаёшь список из того количества имён, которое попросили, с лёгким пафосом.",
            "Просить могут только женские или только мужские имена, длинные или короткие - учитывай это при подборе имён.",
            "Когда объявляешь имена, выдавай имена той культуры, которой попросили, но адаптируй их на русский язык, пиши кириллицей.",
            "Каждый новый раунд объявляй новые имена."
        };
    }

    public interface IAgentIdentity
    {
        string Name { get; }
        string CharacterDescription { get; }
    }

    public interface IAgentSkill
    {
        string Name { get; }

        string[] Rules { get; } 
    }

    public static class LlmLoopDetector
    {
        public static (bool IsLooping, string Period) CheckAnywhereTripleLoopPure(string fullText, int minPeriodLength = 10)
        {
            if (string.IsNullOrEmpty(fullText) || fullText.Length < 3) return (false, "");

            int n = fullText.Length;

            // Арендуем массив из пула, чтобы не плодить мусор в GC на каждый вызов
            int[] pi = ArrayPool<int>.Shared.Rent(n);

            try
            {
                // Инициализируем начало массива нулями (ArrayPool возвращает грязные массивы)
                Array.Clear(pi, 0, n);

                // Идем КМП-ом задом наперед
                for (int i = 1; i < n; i++)
                {
                    int j = pi[i - 1];
                    char currentHeadChar = fullText[n - 1 - i];

                    while (j > 0 && currentHeadChar != fullText[n - 1 - j])
                    {
                        j = pi[j - 1];
                    }
                    if (currentHeadChar == fullText[n - 1 - j])
                    {
                        j++;
                    }
                    pi[i] = j;

                    int longestSuffPref = pi[i];
                    if (longestSuffPref > 0)
                    {
                        int processedLength = i + 1;
                        int periodLength = processedLength - longestSuffPref;

                        // Проверяем условия:
                        // 1. Длина периода должна быть не меньше заданной (пропускаем "лалала", "...", и т.д.)
                        // 2. Хвост делится на период без остатка
                        // 3. Накопилось как минимум 3 полных повторения
                        if (periodLength >= minPeriodLength &&
                            processedLength % periodLength == 0 &&
                            processedLength / periodLength >= 3)
                        {
                            string period = fullText.Substring(n - periodLength, periodLength);
                            return (true, period);
                        }
                    }
                }
            }
            finally
            {
                // Обязательно возвращаем массив в пул
                ArrayPool<int>.Shared.Return(pi);
            }

            return (false, "");
        }
    }

    public class Agent
    {
        private Subject<AgentIteration> answer = new Subject<AgentIteration>();
        private Subject<AgentLog> log = new Subject<AgentLog>();

        private HttpClient client;

        public IObservable<AgentIteration> Answer => answer;
        public IObservable<AgentLog> Log => log;

        private List<MessageDto> messages = new List<MessageDto>();

        private List<ToolDto> toolDtos = new List<ToolDto>();

        private List<ITool> tools = new List<ITool>();

        private float temperature = 0;

        public Agent(string description)
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(1);

            messages.Add(new MessageDto
            {
                Role = Roles.System,
                Content = description
            });

            //todo: не работает таймаут при стриминговом ответе, добавить опцию отключать стриминг
            //todo: написать тесты, тестирующие вызов тулы с temperature 0 чтоб можно было гонять автоматизацию на агенте и не бояться его рефакторить
        }

        public Agent(IAgentDescription description)
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            messages.Add(new MessageDto
            {
                Role = Roles.System,
                Content = GetSystemDescription()
            });

            string GetSystemDescription()
            {
                var result = description.Identity.CharacterDescription + "\n";
                foreach(var skill in description.Skills)
                {
                    result += $"У тебя есть способность: {skill.Name}.\n";
                    result += $"Она заключается в следующем:\n";

                    var i = 1;
                    foreach (var rule in skill.Rules)
                    {
                        result += $"{i}: {rule}.\n";
                        i++;
                    }
                }

                if (description.Traits.Length > 0)
                {
                    result += $"У тебя есть особенности.\n";

                    var i = 1;
                    foreach (var trait in description.Traits)
                    {
                        result += $"{i}: {trait.Trait}.\n";
                        i++;
                    }
                }

                return result;
            }
        }

        public void AddTool(ITool tool)
        {
            tools.Add(tool);
            toolDtos.Add(
                new ToolDto()
                {
                    Function = tool.ToOpenAIType()
                }
            );
        }

        public async Task<string> Ask(string content)
        {
            messages.Add(new MessageDto
            {
                Role = Roles.User,
                Content = content
            });

            answer.OnNext(new AgentIteration()
            {
                Answer = false,
                Message = content
            });

            var final = false;

            while (final == false)
            {
                var msg = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/v1/chat/completions");
                msg.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new ChatRequest()
                        {
                            Temperature = temperature, // 0.7f,
                            Tools = toolDtos,
                            Messages = messages.ToArray().ToList(),
                            
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                );

                var result = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);

                var statusCode = result.StatusCode;
                var contentResult = await ProcessStreamToFullResponseJson(result);

                var contentResultParametrized = JsonConvert.DeserializeObject<ChatCompletionResponse>(contentResult);

                var finishReason = contentResultParametrized.Choices[0].FinishReason;

                if (finishReason == "stop")
                {
                    messages.Add(contentResultParametrized.Choices[0].Message);

                    answer.OnNext(new AgentIteration()
                    {
                        Answer = true,
                        Message = contentResultParametrized.Choices[0].Message.Content
                    });

                    return contentResultParametrized.Choices[0].Message.Content;
                }
                else if (contentResultParametrized.Choices[0].FinishReason == "length")
                {
                    answer.OnNext(new AgentIteration()
                    {
                        Answer = true,
                        Error = true,
                        Message = $"Error 'length', limit exceeded.\nUsage:\n{JsonConvert.SerializeObject(contentResultParametrized.Usage, formatting: Formatting.Indented)}"
                    });
                }
                else if (finishReason == "tool_calls")
                {
                    messages.Add(contentResultParametrized.Choices[0].Message);

                    answer.OnNext(new AgentIteration()
                    {
                        Answer = true,
                        Message = contentResultParametrized.Choices[0].Message.Content
                    });

                    foreach (var tool in contentResultParametrized.Choices[0].Message.ToolCalls)
                    {
                        var expectedTool = tools.FirstOrDefault(x => x.Name == tool.Function.Name);

                        if (expectedTool == null)
                        {
                            messages.Add(new MessageDto()
                            {
                                Role = Roles.Tool,
                                ToolCallId = tool.Id,
                                Content = $"Тула с именем '{tool.Function.Name}' нет. Есть такие тулы: [{tools.Select(x => x.Name).JoinStringsWithoutSkipping(",")}].".As(out var toolErrorMessage)
                            });

                            log.OnNext(new AgentLog()
                            {
                                Level = AgentLogLevel.Error,
                                Category = AgentLogCategory.ToolCall,
                                Message = toolErrorMessage
                            });
                        }
                        else
                        {
                            var (toolResult, failure) = await expectedTool.Run(tool);

                            if (failure == null)
                            {
                                messages.Add(new MessageDto()
                                {
                                    Role = Roles.Tool,
                                    ToolCallId = tool.Id,
                                    Content = ""
                                });
                            }
                            else
                            {
                                messages.Add(new MessageDto()
                                {
                                    Role = Roles.Tool,
                                    ToolCallId = tool.Id,
                                    Content = failure.ToExplanation().As(out var explanation)
                                });

                                log.OnNext(new AgentLog()
                                {
                                    Level = AgentLogLevel.Error,
                                    Category = AgentLogCategory.ToolCall,
                                    Message = explanation
                                });
                            }
                        }
                    }
                }
            }

            return "";
        }

        public async Task<string> ProcessStreamToFullResponseJson(HttpResponseMessage response)
        {
            var ctx = new CancellationTokenSource();
            

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            ctx.Token.Register(() =>
            {
                stream?.Dispose();   // Ломает сетевой поток
                response?.Dispose(); // Закрывает HTTP-соединение
                reader?.Dispose();
            });

            JObject? finalJson = null;

            // Буферы для склеивания потоковых данных
            var contentBuilder = new StringBuilder();
            var reasoningBuilder = new StringBuilder();

            // Словарь для сборки аргументов тулов (т.к. они прилетают по частям по индексу)
            var toolCallsDict = new Dictionary<int, JObject>();

            try
            {
                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("data: ")) line = line.Substring(6).Trim();
                    if (line == "[DONE]") continue;

                    try
                    {
                        JObject chunk = JObject.Parse(line);

                        // 1. Инициализируем базовый скелет JSON-ответа по первому чанку
                        if (finalJson == null)
                        {
                            finalJson = (JObject)chunk.DeepClone();
                            // Меняем тип объекта со стримового chunk на полноценный респонз
                            if (finalJson["object"] != null) finalJson["object"] = "chat.completion";
                        }

                        var choice = chunk["choices"]?.FirstOrDefault();
                        var delta = choice?["delta"];

                        if (delta != null)
                        {
                            // 2. Вытягиваем reasoning на КАЖДОЙ итерации
                            string? reasoningChunk = delta["reasoning"]?.ToString();
                            if (!string.IsNullOrEmpty(reasoningChunk))
                            {
                                reasoningBuilder.Append(reasoningChunk);

                                var (isLooping, loop) = LlmLoopDetector.CheckAnywhereTripleLoopPure(reasoningBuilder.ToString());

                                if (isLooping)
                                {
                                    log.OnNext(new AgentLog()
                                    {
                                        Level = AgentLogLevel.Error,
                                        Message = $"Reasoning loop detected: {loop}. Full reasoning: {reasoningBuilder.ToString()}",
                                        Category = AgentLogCategory.Reasoning
                                    });

                                    ctx.Cancel();

                                    throw new Exception($"Reasoning loop detected: {loop}. Full reasoning: {reasoningBuilder.ToString()}");
                                }

                                log.OnNext(new AgentLog()
                                {
                                    Level = AgentLogLevel.Debug,
                                    Message = reasoningChunk,
                                    Category = AgentLogCategory.Reasoning
                                });
                            }

                            // 3. Копим обычный текст
                            string? contentChunk = delta["content"]?.ToString();
                            if (!string.IsNullOrEmpty(contentChunk))
                            {
                                contentBuilder.Append(contentChunk);
                            }

                            // 4. Собираем Tool Calls (Аргументы тулов в стриме приходят кусками строк!)
                            var toolCalls = delta["tool_calls"] as JArray;
                            if (toolCalls != null)
                            {
                                foreach (JObject toolChunk in toolCalls)
                                {
                                    int index = toolChunk["index"]?.Value<int>() ?? 0;

                                    if (!toolCallsDict.ContainsKey(index))
                                    {
                                        toolCallsDict[index] = new JObject
                                {
                                    { "id", toolChunk["id"]?.ToString() },
                                    { "type", "function" },
                                    { "function", new JObject
                                        {
                                            { "name", toolChunk["function"]?["name"]?.ToString() },
                                            { "arguments", new JObject("") }
                                        }
                                    }
                                };
                                    }

                                    // Дописываем прилетевший кусок строки аргументов JSON
                                    string? argChunk = toolChunk["function"]?["arguments"]?.ToString();
                                    if (!string.IsNullOrEmpty(argChunk))
                                    {
                                        var argBuilder = (StringBuilder)((JValue)((JObject)toolCallsDict[index]["function"])["arguments"]).Value;
                                        argBuilder.Append(argChunk);
                                    }
                                }
                            }
                        }

                        // 5. Перехватываем Usage (статистику токенов), если она прилетела в конце
                        if (chunk["usage"] != null)
                        {
                            finalJson["usage"] = chunk["usage"].DeepClone();
                        }
                    }
                    catch { /* Игнорируем ошибки парсинга отдельной строки */ }
                }
            }
            catch (Exception)
            {
                // Сюда мы падаем при твоем минутном таймауте. 
                // Мы НЕ бросаем ошибку, а идем дальше, чтобы выдать агенту всё, что успели собрать!
            }

            // Если модель зависла до первого чанка, создаем пустой объект, чтобы не было NullReference
            if (finalJson == null) finalJson = new JObject();

            // 6. Финализируем структуру choices под формат stream: false
            var finalChoices = new JArray();
            var finalChoice = new JObject
            {
                { "index", 0 },
                { "finish_reason", toolCallsDict.Count > 0 ? "tool_calls" : "stop" }
            };

            var finalMessage = new JObject
            {
                { "role", "assistant" },
                { "content", contentBuilder.ToString() }
            };

            // Если модель успела что-то подумать, пишем reasoning в message
            if (reasoningBuilder.Length > 0)
            {
                finalMessage["reasoning"] = reasoningBuilder.ToString();
            }

            // Зашиваем собранные тулы обратно в валидный JSON-массив
            if (toolCallsDict.Count > 0)
            {
                var finalToolCalls = new JArray();
                foreach (var kvp in toolCallsDict)
                {
                    var toolObj = kvp.Value;
                    var argBuilder = (StringBuilder)((JValue)((JObject)toolObj["function"])["arguments"]).Value;
                    // Превращаем StringBuilder аргументов обратно в строку
                    toolObj["function"]["arguments"] = argBuilder.ToString();
                    finalToolCalls.Add(toolObj);
                }
                finalMessage["tool_calls"] = finalToolCalls;
            }

            finalChoice["message"] = finalMessage;
            finalChoices.Add(finalChoice);
            finalJson["choices"] = finalChoices;

            // Гарантируем наличие объекта usage, даже если упали по таймауту
            if (finalJson["usage"] == null)
            {
                finalJson["usage"] = new JObject
                {
                    { "prompt_tokens", 0 },
                    { "completion_tokens", 0 },
                    { "total_tokens", 0 }
                };
            }

            // Возвращаем чистую монолитную строку JSON, готовую для JsonConvert.DeserializeObject<ChatCompletionResponse>()
            return finalJson.ToString(Newtonsoft.Json.Formatting.None);
        }

        private async Task<string> ReadAsStringWithDetailsAsync(HttpResponseMessage response)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            StringBuilder partialResponse = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 1. Отрезаем префикс SSE "data: ", если строка с него начинается
                if (line.StartsWith("data: "))
                {
                    line = line.Substring(6).Trim();
                }

                // 2. Игнорируем маркер завершения стрима от OpenAI
                if (line == "[DONE]") continue;

                try
                {
                    // 3. Парсим чистый JSON chunk
                    JObject jsonChunk = JObject.Parse(line);

                    // 4. Достаем контент из delta (безопасно через null-conditional)
                    string? chunkText = jsonChunk["choices"]?[0]?["delta"]?["content"]?.ToString();

                    // 5. Если модель думает (Gemma умеет выводить reasoning), можно забирать и его
                    string? reasoningText = jsonChunk["choices"]?[0]?["delta"]?["reasoning"]?.ToString();

                    // Пишем в буфер то, что пришло
                    if (!string.IsNullOrEmpty(chunkText))
                    {
                        partialResponse.Append(chunkText);
                    }
                    else if (!string.IsNullOrEmpty(reasoningText))
                    {
                        log.OnNext(new AgentLog()
                        {
                            Level = AgentLogLevel.Debug,
                            Message = reasoningText,
                            Category = AgentLogCategory.Reasoning
                        });
                    }
                }
                catch (Exception)
                {
                    log.OnNext(new AgentLog()
                    {
                        Level = AgentLogLevel.Error,
                        Message = $"json is incorrect [{line}]",
                        Category = AgentLogCategory.Reasoning
                    });

                    continue;
                }
            }

            return partialResponse.ToString();
        }
    }

    public class Roles
    {
        public static string User = "user";

        public static string Assistant = "assistant";

        public static string System = "system";

        public static string Tool = "tool";
    }

    public class ToolDto
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "function";

        [JsonProperty("function")]
        public FunctionDefinition Function { get; set; } = new FunctionDefinition();
    }

    public class FunctionDefinition
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        // Сами аргументы функции описываются через объект параметров JSON Schema
        [JsonProperty("parameters")]
        public FunctionParameters Parameters { get; set; } = new FunctionParameters();
    }

    public class FunctionParameters
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "object";

        // Словарь, где ключ — имя аргумента (н-р, "word"), а значение — его тип и описание
        [JsonProperty("properties")]
        public Dictionary<string, FunctionProperty> Properties { get; set; } = new Dictionary<string, FunctionProperty>();

        // Список имен обязательных аргументов, без которых модель не должна вызывать тулзу
        [JsonProperty("required")]
        public List<string> Required { get; set; } = new List<string>();
    }

    public class FunctionProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "string"; // string, integer, boolean, number

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        // Если у параметра строго фиксированный список значений (Enum)
        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? Enum { get; set; }
    }

    public class ChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "gemma4-32k";

        [JsonProperty("messages")]
        public List<MessageDto> Messages { get; set; }

        [JsonProperty("stop")]
        public List<string> Stops { get; set; }

        // Добавьте эту строку для управления стримингом
        [JsonProperty("stream")]
        public bool Stream { get; set; } = true;

        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0f;

        [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
        public List<ToolDto>? Tools { get; set; }

        //[JsonProperty("top_p")]
        //public float TopP { get; set; } = 0.95f;

        //[JsonProperty("top_k")]
        //public float TopK { get; set; } = 64f;

        //[JsonProperty("num_predict")]
        //public int Predict { get; set; } = 100;

        /// <summary>
        /// Заставит Ollama/llama.cpp мгновенно выгрузить кэш сессии после отдачи JSON
        /// </summary>
        [JsonProperty("keep_alive")]
        public int KeepAlive { get; set; } = 0;

        /// <summary>
        /// Жестко запирает движок в ОДИН поток/слот. 
        /// Новые запросы будут переиспользовать тот же слот, а не плодить параллельные.
        /// </summary>
        [JsonProperty("options")]
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>()
        {
            { "num_slots", 1 }, // Запрещаем плодить параллельные контексты в VRAM
            //{ "num_ctx", 4096 },     // Здесь ставишь 4000 для чанков, или 15000 для редактора
            { "num_predict", 2000 }   // Аварийный обрубатель бесконечной генерации
        };

        //// Официальный контейнер OpenAI для нестандартных параметров бэкенда
        //[JsonProperty("extra_body", NullValueHandling = NullValueHandling.Ignore)]
        //public Dictionary<string, object>? ExtraBody { get; set; } = new Dictionary<string, object>()
        //{
        //    { "num_ctx", 4096 },
        //    { "num_predict", 300 }
        //};
    }

    public class ChatCompletionResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("object")]
        public string Object { get; set; } = string.Empty;

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; set; } = string.Empty;

        [JsonProperty("choices")]
        public ChoiceDto[] Choices { get; set; } = Array.Empty<ChoiceDto>();

        [JsonProperty("usage")]
        public UsageDto Usage { get; set; } = new UsageDto();
    }

    public class ChoiceDto
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public MessageDto Message { get; set; } = new MessageDto();

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("tool_call_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolCallId { get; set; }

        [JsonProperty("tool_calls", NullValueHandling = NullValueHandling.Ignore)]
        public List<ToolCallDto>? ToolCalls { get; set; }
    }

    public class MessageDto
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("reasoning", NullValueHandling = NullValueHandling.Ignore)]
        public string Reasoning { get; set; } = string.Empty;

        [JsonProperty("tool_call_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolCallId { get; set; }

        [JsonProperty("tool_calls", NullValueHandling = NullValueHandling.Ignore)]
        public List<ToolCallDto> ToolCalls { get; set; } = new List<ToolCallDto>();
    }

    public class ToolCallDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } // Уникальный ID вызова (нужен для ответа)

        [JsonProperty("type")]
        public string Type { get; set; } = "function";

        [JsonProperty("function")]
        public FunctionCallDataDto Function { get; set; } = new FunctionCallDataDto();
    }

    public class FunctionCallDataDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        // ВАЖНО: Мапим аргументы как JToken (сырой узел Newtonsoft), 
        // чтобы потом превратить его в строгий класс одной строчкой кода.
        [JsonProperty("arguments")]
        public JToken Arguments { get; set; }
    }

    public class UsageDto
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("prompt_tokens_details")]
        public PromptTokensDetailsDto PromptTokensDetails { get; set; } = new PromptTokensDetailsDto();

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class PromptTokensDetailsDto
    {
        [JsonProperty("cached_tokens")]
        public int CachedTokens { get; set; }
    }

    

    public class Model
    {
        public string model;
        public string input;
        public bool store;
    }
}
