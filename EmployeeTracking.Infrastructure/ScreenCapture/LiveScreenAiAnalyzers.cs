using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeTracking.Application.ScreenCapture;
using EmployeeTracking.Domain.Common;
using Microsoft.Extensions.Options;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class GrokScreenAiAnalyzer : LiveScreenAiAnalyzer
    {
        public GrokScreenAiAnalyzer(HttpClient http, IOptions<ScreenCaptureOptions> options, IScreenImageStorage storage)
            : base(http, options.Value.Grok, options.Value.Prompt, storage, "Grok") { }

        protected override async Task<string> CompleteAsync(IReadOnlyList<byte[]> images, CancellationToken ct)
        {
            using var response = await Http.SendAsync(OpenAiStyleRequest(_provider.Endpoint, images), ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new DomainException("Grok did not return a screen review.");
            return ReadChatContent(body);
        }
    }

    public class OpenAiScreenAiAnalyzer : LiveScreenAiAnalyzer
    {
        public OpenAiScreenAiAnalyzer(HttpClient http, IOptions<ScreenCaptureOptions> options, IScreenImageStorage storage)
            : base(http, options.Value.OpenAi, options.Value.Prompt, storage, "GPT") { }

        protected override async Task<string> CompleteAsync(IReadOnlyList<byte[]> images, CancellationToken ct)
        {
            using var response = await Http.SendAsync(OpenAiStyleRequest(_provider.Endpoint, images), ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new DomainException("GPT did not return a screen review.");
            return ReadChatContent(body);
        }
    }

    public class GeminiScreenAiAnalyzer : LiveScreenAiAnalyzer
    {
        public GeminiScreenAiAnalyzer(HttpClient http, IOptions<ScreenCaptureOptions> options, IScreenImageStorage storage)
            : base(http, options.Value.Gemini, options.Value.Prompt, storage, "Gemini") { }

        protected override async Task<string> CompleteAsync(IReadOnlyList<byte[]> images, CancellationToken ct)
        {
            var parts = new List<object> { new { text = Prompt } };
            parts.AddRange(images.Select(image => new
            {
                inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(image) }
            }));
            var url = $"{_provider.Endpoint.TrimEnd('/')}/models/{_provider.Model}:generateContent?key={_provider.ApiKey}";
            using var response = await Http.PostAsJsonAsync(url, new { contents = new[] { new { parts } } }, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new DomainException("Gemini did not return a screen review.");

            using var json = JsonDocument.Parse(body);
            var text = json.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new DomainException("Gemini returned an empty screen review.");
            return text.Trim();
        }
    }

    public abstract class LiveScreenAiAnalyzer : IScreenAiAnalyzer
    {
        private readonly IScreenImageStorage _storage;
        protected readonly HttpClient Http;
        protected readonly AiProviderOptions _provider;
        protected readonly string Prompt;

        protected LiveScreenAiAnalyzer(
            HttpClient http,
            AiProviderOptions provider,
            string prompt,
            IScreenImageStorage storage,
            string name)
        {
            Http = http;
            _provider = provider;
            Prompt = string.IsNullOrWhiteSpace(prompt) ? ScreenCaptureOptions.DefaultPrompt : prompt;
            _storage = storage;
            if (string.IsNullOrWhiteSpace(provider.ApiKey) || string.IsNullOrWhiteSpace(provider.Model) || string.IsNullOrWhiteSpace(provider.Endpoint))
                throw new DomainException($"{name} screen analysis is not configured.");
            if (!name.Equals("Gemini", StringComparison.Ordinal))
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);
        }

        public async Task<IReadOnlyList<string>> AnalyzeAsync(ScreenAiRequest request, CancellationToken ct = default)
        {
            var images = new List<byte[]>();
            foreach (var image in request.Images.TakeLast(6))
            {
                var bytes = await _storage.ReadAsync(image.StorageKey, ct);
                if (bytes is { Length: > 0 }) images.Add(bytes);
            }

            if (images.Count == 0)
                return ["No screen snapshots were captured, so there is nothing to review."];

            var text = await CompleteAsync(images, ct);
            return [text];
        }

        protected abstract Task<string> CompleteAsync(IReadOnlyList<byte[]> images, CancellationToken ct);

        protected HttpRequestMessage OpenAiStyleRequest(string endpoint, IReadOnlyList<byte[]> images)
        {
            var content = new List<object> { new { type = "text", text = Prompt } };
            content.AddRange(images.Select(image => new
            {
                type = "image_url",
                image_url = new { url = $"data:image/jpeg;base64,{Convert.ToBase64String(image)}" }
            }));
            var payload = new
            {
                model = _provider.Model,
                messages = new object[]
                {
                    new { role = "user", content }
                }
            };
            return new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };
        }

        protected static string ReadChatContent(string body)
        {
            using var json = JsonDocument.Parse(body);
            var text = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new DomainException("The screen review was empty.");
            return text.Trim();
        }
    }
}
