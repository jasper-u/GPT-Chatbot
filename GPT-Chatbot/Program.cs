using Newtonsoft.Json;
using Spectre.Console;

//Use your own OpenAI API key: https://platform.openai.com/account/api-keys
const string key = "your key";
const string url = "https://api.openai.com/v1/chat/completions";

var messages = new List<dynamic>
{
    new {role = "system", content = "You are chatGPT, a large language " +
                                    "model trained by OpenAI. " +
                                    "Answer as concisely as possible.  " +
                                    "Tell a fun fact about cats every few lines (but not to much) just to spice things up."},
    new {role = "assistant", content = "How can I help you?"}
};
AnsiConsole.MarkupLine($"[purple]MACHINE:[/] [blue]{Markup.Escape(messages[1].content)}[/]");

while (true)
{
    var userMessage = AnsiConsole.Ask<string>($"[purple]USER:[/] ");
    messages.Add(new {role = "user", content = userMessage} );

    var request = new
    {
        messages,
        model = "gpt-3.5-turbo",
        max_tokens = 300,
    };
    //Request and response
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
    var requestJson = JsonConvert.SerializeObject(request);
    var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8,"application/json");
    var httpResponseMessage = await httpClient.PostAsync(url, requestContent);
    var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
    var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
    {
        choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
        error = new { message = string.Empty }
    });

    if (!string.IsNullOrEmpty(responseObject?.error?.message)) 
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(responseObject.error.message)}[/]");
    }
    else
    {
        // Add the message
        var messageObject = responseObject?.choices[0].message;
        messages.Add(messageObject);
        AnsiConsole.MarkupLine($"[purple]MACHINE:[/] [blue]{Markup.Escape(messageObject.content)}[/]");
    }
}