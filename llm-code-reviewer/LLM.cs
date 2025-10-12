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
    public enum LlmInitError
    {
        None,
        NoApiKey,
        NoConnection,
        NetworkError,
        Unknown
    }
    public class LLM
    {
        private readonly HttpClient _httpClient = new();
        public string model;
        public LlmInitError ErrorCode { get; }
        public bool IsReady => ErrorCode == LlmInitError.None;
        public string Placeholder { get; }
        
        public LLM(string model = "")
        {
            var key = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY");

            if (string.IsNullOrWhiteSpace(key))
            {
                ErrorCode = LlmInitError.NoApiKey;
                Placeholder = "OpenAI API key not found.";
                return;
            }

            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.openai.com/") };
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", key);

            try
            {
                var resp = _httpClient.GetAsync("v1/models").Result;
                if (!resp.IsSuccessStatusCode)
                {
                    ErrorCode = LlmInitError.NoConnection;
                    Placeholder = "Unable to reach OpenAI servers.";
                    return;
                }
            }
            catch
            {
                ErrorCode = LlmInitError.NetworkError;
                Placeholder = "fNetwork unavailable.";
                return;
            }

            this.model = model;
            ErrorCode = LlmInitError.None;
            Placeholder = "";
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

            var response = await _httpClient.PostAsync("v1/chat/completions", content);
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