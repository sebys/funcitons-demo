#r "Newtonsoft.Json"

using System;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{    
    string jsonContent = await req.Content.ReadAsStringAsync();
    var data = JsonConvert.DeserializeObject<RequestModel>(jsonContent);

    if (string.IsNullOrEmpty(data.Comment)) {
        return req.CreateResponse(HttpStatusCode.BadRequest, new {
            error = "Please pass comment property in the input object"
        });
    }

    // Notificar a slack
    const string slackUrl = "https://hooks.slack.com/services/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";
    
    using (var client = new HttpClient())
    {
        var argsAsJson = JsonConvert.SerializeObject(new { text = data.Comment, username = data.User, icon_url = data.Avatar });
        HttpContent contentPost = new StringContent(argsAsJson, Encoding.UTF8, "application/json");
        
        var slackPayload = new SlackPayload
        {
            Text = $"_Issue:_ *{data.Title}*\n _Comment:_ {data.Comment}",
            User = data.User,
            Icon = data.Avatar
        };

        await client.PostAsJsonAsync(slackUrl, slackPayload);
    }

    return req.CreateResponse(HttpStatusCode.OK, new {
        greeting = $"Okey baby!"
    });
}

public class RequestModel
{
    public string Title { get; set; }

    public string User { get; set; }

    public string Avatar { get; set; }

    public string Comment { get; set; }
}

public class SlackPayload
{
    [JsonProperty("username")]
    public string User { get; set; }

    [JsonProperty("icon_url")]
    public string Icon { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}