using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<string> FunctionHandler(JObject input, ILambdaContext context)
        {
            context.Logger.LogLine("Received event: " + input.ToString());

            // Extract issue URL
            string issueUrl = input["issue"]?["html_url"]?.ToString();
            if (string.IsNullOrEmpty(issueUrl))
            {
                context.Logger.LogLine("No issue URL found in payload.");
                return "No issue found.";
            }

            // Prepare Slack message
            var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            var slackMessage = new JObject
            {
                ["text"] = $"🐙 New GitHub Issue Created: {issueUrl}"
            };

            // Send to Slack
            var response = await httpClient.PostAsync(
                slackUrl,
                new StringContent(slackMessage.ToString(), Encoding.UTF8, "application/json")
            );

            context.Logger.LogLine($"Posted to Slack: {response.StatusCode}");
            return $"Posted to Slack: {response.StatusCode}";
        }
    }
}
