using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoginForm.Services
{
    public class CaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CaptchaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var secretKey = _configuration["Turnstile:SecretKey"];

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("secret", secretKey),
            new KeyValuePair<string, string>("response", token)
        });

            var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
            if (!response.IsSuccessStatusCode) return false;

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TurnstileResponse>(jsonString);

            return result?.Success ?? false;
        }

        private class TurnstileResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
    }
}
