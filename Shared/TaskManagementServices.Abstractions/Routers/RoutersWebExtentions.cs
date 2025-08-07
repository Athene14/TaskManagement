using System.Net.Http.Json;

namespace TaskManagementServices.Shared.Routers
{
    internal static class RoutersWebExtentions
    {
        public static async Task<T> PostAndReadResponseAsync<T>(this HttpClient client, string url, object data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            return await ReadResponse<T>(response);
        }

        public static async Task PostAndReadResponseAsync(this HttpClient client, string url, object data)
        {
            var response = await client.PostAsJsonAsync(url, data);
            await HandleResponse(response);
        }

        public static async Task<T> GetAndReadResponseAsync<T>(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            return await ReadResponse<T>(response);
        }

        public static async Task<T> PutAndReadResponseAsync<T>(this HttpClient client, string url, object data)
        {
            var response = await client.PutAsJsonAsync(url, data);
            return await ReadResponse<T>(response);
        }

        public static async Task PutAndReadResponseAsync(this HttpClient client, string url, object data)
        {
            var response = await client.PutAsJsonAsync(url, data);
            await HandleResponse(response);
        }

        public static async Task<T> DeleteAndReadResponseAsync<T>(this HttpClient client, string url)
        {
            var response = await client.DeleteAsync(url);
            return await ReadResponse<T>(response);
        }

        public static async Task DeleteAndReadResponseAsync(this HttpClient client, string url)
        {
            var response = await client.DeleteAsync(url);
            await HandleResponse(response);
        }

        // Когда-то отвечал за обработку ошибок от сервисов. Сейчас этим занимается Gateway. Оставил на всякий слуай
        private static async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorContent, null, response.StatusCode);
            }
        }

        private static async Task<T> ReadResponse<T>(HttpResponseMessage response)
        {
            await HandleResponse(response);

            return await response.Content.ReadFromJsonAsync<T>();
        }


    }
}
