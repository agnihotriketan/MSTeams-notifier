using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using Newtonsoft.Json;

// Authentication abstraction
public interface IAuthProvider
{
    Task<string> GetAccessTokenAsync();
}

public class MsalAuthProvider : IAuthProvider
{
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string[] _scopes;
    private readonly string _redirectUri;
    public MsalAuthProvider(string tenantId, string clientId, string[] scopes, string redirectUri)
    {
        _tenantId = tenantId;
        _clientId = clientId;
        _scopes = scopes;
        _redirectUri = redirectUri;
    }
    public async Task<string> GetAccessTokenAsync()
    {
        var app = PublicClientApplicationBuilder.Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithRedirectUri(_redirectUri)
            .Build();
        var result = await app.AcquireTokenInteractive(_scopes).ExecuteAsync();
        return result.AccessToken;
    }
}

// Teams message service abstraction
public interface ITeamsMessageService
{
    Task<HttpResponseMessage> SendMessageAsync(String chatId, Object messageObj);
}

public class TeamsMessageService : ITeamsMessageService
{
    private readonly HttpClient _httpClient;
    public TeamsMessageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<HttpResponseMessage> SendMessageAsync(String chatId, Object messageObj)
    {
        var json = JsonConvert.SerializeObject(messageObj);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var endpoint = $"https://graph.microsoft.com/v1.0/chats/{chatId}/messages";
        return await _httpClient.PostAsync(endpoint, content);
    }
}

// Adaptive card helper
public static class AdaptiveCardFactory
{
    public static string CreateOptionCard(string prompt)
    {
        // Use verbatim interpolated string with doubled braces for JSON braces
        return $@"{{
  ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
  ""type"": ""AdaptiveCard"",
  ""version"": ""1.5"",
  ""body"": [
    {{
      ""type"": ""TextBlock"",
      ""text"": ""{prompt}"",
      ""wrap"": true
    }}
  ],
  ""actions"": [
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Approve"",
      ""data"": {{ ""choice"": ""approve"" }}
    }},
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Reject"",
      ""data"": {{ ""choice"": ""reject"" }}
    }},
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Details"",
      ""data"": {{ ""choice"": ""details"" }}
    }}
  ]
}}";
    }

    public static string CreateCargoDemoCard(string shipmentId, string status, string eta, string origin, string destination)
    {
        // More interactive, visually appealing, and business-relevant adaptive card for cargo company
        return $@"{{
  ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
  ""type"": ""AdaptiveCard"",
  ""version"": ""1.5"",
  ""body"": [
    {{
      ""type"": ""ColumnSet"",
      ""columns"": [
        {{
          ""type"": ""Column"",
          ""width"": ""auto"",
          ""items"": [
            {{
              ""type"": ""Image"",
              ""url"": ""https://img.icons8.com/fluency/96/000000/cargo-ship.png"",
              ""size"": ""Medium""
            }}
          ]
        }},
        {{
          ""type"": ""Column"",
          ""width"": ""stretch"",
          ""items"": [
            {{
              ""type"": ""TextBlock"",
              ""text"": ""Shipment #{shipmentId}"",
              ""weight"": ""Bolder"",
              ""size"": ""Large""
            }},
            {{
              ""type"": ""TextBlock"", 
              ""text"": ""Status: 🟡 In Transit"",
              ""color"": ""Warning"",
              ""weight"": ""Bolder"",
              ""wrap"": true
            }}, 
            {{
              ""type"": ""FactSet"",
              ""facts"": [
                {{ ""title"": ""ETA"", ""value"": ""{eta}"" }},
                {{ ""title"": ""Origin"", ""value"": ""{origin}"" }},
                {{ ""title"": ""Destination"", ""value"": ""{destination}"" }}
              ]
            }}
          ]
        }}
      ]
    }},
    {{
      ""type"": ""Input.Text"",
      ""id"": ""feedback"",
      ""placeholder"": ""Add delivery feedback or report an issue..."",
      ""isMultiline"": true
    }}
  ],
  ""actions"": [
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Mark as Delivered"",
      ""data"": {{ ""action"": ""markDelivered"", ""shipmentId"": ""{shipmentId}"" }}
    }},
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Report Delay"",
      ""data"": {{ ""action"": ""reportDelay"", ""shipmentId"": ""{shipmentId}"" }}
    }},
    {{
      ""type"": ""Action.OpenUrl"",
      ""title"": ""Track Live"",
      ""url"": ""https://www.bing.com/maps?q={destination}"",
      ""msoffice365"": true
    }},
    {{
      ""type"": ""Action.Submit"",
      ""title"": ""Contact Support"",
      ""data"": {{ ""action"": ""contactSupport"", ""shipmentId"": ""{shipmentId}"" }}
    }}
  ]
}}";
    }
}

class Program
{
    static async Task Main()
    {
        var tenantId = "3055cc57-059d-400d-b146-c4ca8457c912";
        var clientId = "1dc7c10d-8d66-463e-9bb3-0aebe0b95c96";
        var chatId = "19:97ebbb3e0ea843a5b7f5dad4b93f7074@thread.v2";
        var scopes = new[] { "Chat.ReadWrite", "User.Read" };
        var redirectUri = "http://localhost:5000";

        IAuthProvider authProvider = new MsalAuthProvider(tenantId, clientId, scopes, redirectUri);
        string accessToken = null;
        try
        {
            Console.WriteLine("Please sign in...");
            accessToken = await authProvider.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("❌ Access token was not acquired.");
                return;
            }
        }
        catch (MsalException msalEx)
        {
            Console.WriteLine($"MSAL error: {msalEx.Message}");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception: {ex.Message}");
            return;
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        ITeamsMessageService teamsService = new TeamsMessageService(httpClient);

        Console.WriteLine("Type your message to send to Teams (type 'exit' to quit):");
        while (true)
        {
            Console.Write("Message: ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput) || userInput.Trim().ToLower() == "exit")
                break;

            // Demo shipment data
            var shipmentId = "CARGO123456";
            var status = "In Transit";
            var eta = DateTime.UtcNow.AddHours(5).ToString("yyyy-MM-dd HH:mm") + " UTC";
            var origin = "Shanghai";
            var destination = "Rotterdam";

            var greetMsg = new { body = new { content = userInput } };
            var attachmentId = Guid.NewGuid().ToString();
            var adaptiveCardJson = AdaptiveCardFactory.CreateCargoDemoCard(shipmentId, status, eta, origin, destination);
            var messageObj = new
            {
                body = new
                {
                    contentType = "html",
                    content = $"<attachment id=\"{attachmentId}\"></attachment>"
                },
                attachments = new[]
                {
                    new
                    {
                        id = attachmentId,
                        contentType = "application/vnd.microsoft.card.adaptive",
                        content = adaptiveCardJson
                    }
                }
            };

            // Send user message
            var response1 = await teamsService.SendMessageAsync(chatId, greetMsg);
            if (response1.IsSuccessStatusCode)
                Console.WriteLine("✅ Text message sent successfully!");
            else
            {
                var err = await response1.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to send text message: {err}");
            }

            // Send adaptive card
            var response = await teamsService.SendMessageAsync(chatId, messageObj);
            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ Cargo demo adaptive card sent successfully!");
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to send adaptive card: {err}");
            }
        }
        Console.WriteLine("Exiting. Press any key to close...");
        Console.ReadKey();
    }
}
