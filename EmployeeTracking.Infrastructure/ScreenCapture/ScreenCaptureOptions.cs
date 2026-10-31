namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class ScreenCaptureOptions
    {
        public const string Simulated = "Simulated";
        public const string DefaultPrompt =
            "Review these screenshots from one work session. Describe the work visible on the screen in a few sentences. Do not invent activity that is not in the images.";

        public string Storage { get; set; } = Simulated;
        public string Ai { get; set; } = Simulated;
        public string Prompt { get; set; } = DefaultPrompt;
        public AzureStorageOptions Azure { get; set; } = new();
        public AwsStorageOptions Aws { get; set; } = new();
        public AiProviderOptions Grok { get; set; } = new()
        {
            Model = "grok-2-vision-1212",
            Endpoint = "https://api.x.ai/v1/chat/completions"
        };
        public AiProviderOptions Gemini { get; set; } = new()
        {
            Model = "gemini-2.0-flash",
            Endpoint = "https://generativelanguage.googleapis.com/v1beta"
        };
        public AiProviderOptions OpenAi { get; set; } = new()
        {
            Model = "gpt-4o",
            Endpoint = "https://api.openai.com/v1/chat/completions"
        };
    }

    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = "screen-captures";
    }

    public class AwsStorageOptions
    {
        public string Bucket { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }

    public class AiProviderOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}
