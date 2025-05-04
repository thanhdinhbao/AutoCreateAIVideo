using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoCreateAIVideo
{
    class AiService
    {
        private readonly string _leonardoApiKey = "YOUR_LEONARDO_API_KEY"; // <-- Thay bằng key thật
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<string>> GenerateImageFromPrompt_LeonardoAsync(string prompt)
        {
            // Bước 1: Gửi request tạo ảnh
            var payload = new
            {
                prompt = prompt,
                modelId = "fd5530c1-5e3c-4b42-b3d3-fcdb5b1d83bd", // ID của model (tùy bạn chọn)
                num_images = 1,
                width = 512,
                height = 512,
                guidance_scale = 7.5,
                promptMagic = true
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://cloud.leonardo.ai/api/rest/v1/generations");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _leonardoApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to create generation: " + json);

            var root = JsonDocument.Parse(json);
            string generationId = root.RootElement.GetProperty("sdGenerationJob").GetProperty("generationId").GetString();

            // Bước 2: Poll trạng thái
            string imageUrl = null;
            for (int i = 0; i < 30; i++) // Poll trong tối đa 30 lần
            {
                await Task.Delay(2000);

                var statusRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"https://cloud.leonardo.ai/api/rest/v1/generations/{generationId}");
                statusRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _leonardoApiKey);

                var statusResponse = await _httpClient.SendAsync(statusRequest);
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                if (!statusResponse.IsSuccessStatusCode) continue;

                var statusRoot = JsonDocument.Parse(statusJson);
                var images = statusRoot.RootElement.GetProperty("generations_by_pk")
                                                   .GetProperty("generated_images");

                if (images.GetArrayLength() > 0)
                {
                    imageUrl = images[0].GetProperty("url").GetString();
                    break;
                }
            }

            return new List<string> { imageUrl };
        }
    }
}
