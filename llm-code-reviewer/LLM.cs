using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LLMCodeReviewer
{
    public class LLM
    {
        public string prompt;
        private readonly HttpClient __httpClient = new();
        public string model;
        public LLM(string model = "")
        {
            prompt = "";
            var __apiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY")
                      ?? throw new InvalidOperationException("Environment variable OPEN_AI_API_KEY not set!");

            this.model = model ?? "gpt-5";
            
            __httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/")
            };
            __httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", __apiKey);
        }
        
        public async Task<string> AskAsync(string prompt)
        {
            var body = new
            {
                model = this.model, 
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await __httpClient.PostAsync("v1/chat/completions", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {responseText}");

            using var doc = JsonDocument.Parse(responseText);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
    }
}