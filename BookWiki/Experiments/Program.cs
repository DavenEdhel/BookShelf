using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

namespace Experiments
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //System.Web.HttpUtility.UrlDecode("%u05d9%u05e8%u05d5%u05e9%u05dc%u05d9%u05dd")

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var message =
                "\u0412\u043e\u0442 \u043d\u0435\u0441\u043a\u043e\u043b\u044c\u043a\u043e \u043f\u0440\u0438\u043c\u0435\u0440\u043e\u0432 \u0438\u043c\u0435\u043d \u0438 \u0444\u0430\u043c\u0438\u043b\u0438\u0439 \u043d\u0430 \u0438\u0441\u043f\u0430\u043d\u0441\u043a\u0438\u0439 \u043c\u0430\u043d\u0435\u0440:\n\n1. \u041b\u0430\u0443\u0440\u0430 \u0421\u0443\u0430\u0440\u0435\u0441\n2. \u041f\u0430\u0431\u043b\u043e \u0413\u043e\u043c\u0435\u0441\n3. \u0418\u0437\u0430\u0431\u0435\u043b\u044c \u041c\u0430\u0440\u0442\u0438\u043d\u0435\u0441\n4. \u0425\u0430\u0432\u044c\u0435\u0440 \u0420\u043e\u0434\u0440\u0438\u0433\u0435\u0441\n5. \u0421\u043e\u0444\u0438\u044f \u041f\u0435\u0440\u0435\u0441\n\n\u0415\u0441\u043b\u0438 \u043d\u0443\u0436\u043d\u043e \u0431\u043e\u043b\u044c\u0448\u0435 \u0432\u0430\u0440\u0438\u0430\u043d\u0442\u043e\u0432 \u0438\u043b\u0438 \u043a\u0430\u043a\u043e\u0433\u043e-\u0442\u043e \u043a\u043e\u043d\u043a\u0440\u0435\u0442\u043d\u043e\u0433\u043e \u0441\u0442\u0438\u043b\u044f, \u0434\u0430\u0439\u0442\u0435 \u0437\u043d\u0430\u0442\u044c!";

            var m2 =
                "\u0412\u043e\u0442 \u043f\u0440\u0438\u043c\u0435\u0440 \u0438\u043c\u0435\u043d\u0438 \u0438 \u0444\u0430\u043c\u0438\u043b\u0438\u0438 \u043d\u0430 \u0438\u0441\u043f\u0430\u043d\u0441\u043a\u0438\u0439 \u043c\u0430\u043d\u0435\u0440 \u0432 \u0444\u043e\u0440\u043c\u0430\u0442\u0435 JSON:\n\n```json\n{\n    \"nombre\": \"Sof\u00eda\",\n    \"apellido\": \"Gonz\u00e1lez\"\n}\n```\n\n\u0415\u0441\u043b\u0438 \u043d\u0443\u0436\u043d\u043e \u0431\u043e\u043b\u044c\u0448\u0435 \u043f\u0440\u0438\u043c\u0435\u0440\u043e\u0432 \u0438\u043b\u0438 \u0434\u0440\u0443\u0433\u0438\u0445 \u0438\u043c\u0435\u043d, \u0434\u0430\u0439\u0442\u0435 \u0437\u043d\u0430\u0442\u044c!";

            var m3 =
                "Sure! Below is a simple representation of a JSON file containing a list of first names and last names in Russian (Cyrillic script) that could be found in Spain.\n\n```json\n{\n  \"people\": [\n    {\n      \"first_name\": \"\u0410\u043b\u0435\u043a\u0441\u0430\u043d\u0434\u0440\",\n      \"last_name\": \"\u0418\u0432\u0430\u043d\u043e\u0432\"\n    },\n    {\n      \"first_name\": \"\u041c\u0430\u0440\u0438\u044f\",\n      \"last_name\": \"\u041f\u0435\u0442\u0440\u043e\u0432\u0430\"\n    },\n    {\n      \"first_name\": \"\u0421\u0432\u0435\u0442\u043b\u0430\u043d\u0430\",\n      \"last_name\": \"\u0421\u0438\u0434\u043e\u0440\u043e\u0432\u0430\"\n    },\n    {\n      \"first_name\": \"\u0414\u043c\u0438\u0442\u0440\u0438\u0439\",\n      \"last_name\": \"\u0424\u0451\u0434\u043e\u0440\u043e\u0432\"\n    },\n    {\n      \"first_name\": \"\u0415\u043a\u0430\u0442\u0435\u0440\u0438\u043d\u0430\",\n      \"last_name\": \"\u041a\u0443\u0437\u043d\u0435\u0446\u043e\u0432\u0430\"\n    },\n    {\n      \"first_name\": \"\u0418\u0433\u043e\u0440\u044c\",\n      \"last_name\": \"\u0415\u0433\u043e\u0440\u043e\u0432\"\n    },\n    {\n      \"first_name\": \"\u041e\u043b\u044c\u0433\u0430\",\n      \"last_name\": \"\u0421\u043c\u0438\u0440\u043d\u043e\u0432\u0430\"\n    },\n    {\n      \"first_name\": \"\u041d\u0438\u043a\u043e\u043b\u0430\u0439\",\n      \"last_name\": \"\u0412\u0430\u0441\u0438\u043b\u044c\u0435\u0432\"\n    },\n    {\n      \"first_name\": \"\u0410\u043d\u043d\u0430\",\n      \"last_name\": \"\u041c\u043e\u0440\u043e\u0437\u043e\u0432\u0430\"\n    },\n    {\n      \"first_name\": \"\u041a\u043e\u043d\u0441\u0442\u0430\u043d\u0442\u0438\u043d\",\n      \"last_name\": \"\u0420\u043e\u043c\u0430\u043d\u043e\u0432\"\n    }\n  ]\n}\n```\n\nYou can save this JSON data to a file with a `.json` extension. If you need a different format or more names, just let me know!";

            var m4 =
                "Sure! Here\u2019s an example of a JSON file containing a list of first names and last names in Russian (\u0440\u0443\u0441\u0441\u043a\u0438\u0435 \u0438\u043c\u0435\u043d\u0430) that could be associated with individuals from Spain:\n\n```json\n{\n  \"people\": [\n    {\n      \"first_name\": \"\u0410\u043b\u0435\u043a\u0441\u0435\u0439\",\n      \"last_name\": \"\u0413omez\"\n    },\n    {\n      \"first_name\": \"\u041c\u0430\u0440\u0438\u044f\",\n      \"last_name\": \"\u0421\u0438\u043c\u043e\u043d\u043e\u0432\"\n    },\n    {\n      \"first_name\": \"\u0418\u0432\u0430\u043d\",\n      \"last_name\": \"\u041b\u043e\u043f\u0435\u0437\"\n    },\n    {\n      \"first_name\": \"\u0415\u043a\u0430\u0442\u0435\u0440\u0438\u043d\u0430\",\n      \"last_name\": \"\u041c\u0430\u0440\u0442\u0438\u043d\u0435\u0441\"\n    },\n    {\n      \"first_name\": \"\u0414\u043c\u0438\u0442\u0440\u0438\u0439\",\n      \"last_name\": \"\u041f\u0435\u0440\u0438\"\n    },\n    {\n      \"first_name\": \"\u0410\u043d\u043d\u0430\",\n      \"last_name\": \"\u0421\u0435\u0440\u0440\u0430\u043d\u043e\"\n    }\n  ]\n}\n```\n\nYou can save this content into a JSON file by copying it into a text editor and saving the file with a `.json` extension. Let me know if you need any adjustments or additional entries!";

            Console.WriteLine(m3);

            Console.WriteLine(m4);

            return;

            using (var client = new HttpClient())
            {
                var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
                msg.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new Model()
                        {
                            model = "gpt-4o-mini",
                            input = "generate json file with list of first name (ru) and second name (ru) from Spain",
                            store = true
                        }
                    ),
                    Encoding.UTF32,
                    new MediaTypeHeaderValue("application/json")
                );
                //JsonContent.Create(

                //);
                //msg.Headers.Add("Content-Type", "application/json");
                var result = client.Send(msg);

                var statusCode = result.StatusCode;
                var contentResult = result.Content.ReadAsStringAsync().Result;

                Console.WriteLine($"Status: {statusCode}");
                Console.WriteLine(contentResult);
            }
        }
    }

    public class Model
    {
        public string model;
        public string input;
        public bool store;
    }
}
