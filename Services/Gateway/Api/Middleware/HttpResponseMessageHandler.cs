namespace Gateway.Api.Middleware
{
    /// <summary>
    /// Хэндлер для проброса ошибок от сервисов к gateway
    /// </summary>
    public class HttpResponseMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ex = new HttpRequestException(content, new Exception(), statusCode: response.StatusCode);
                throw ex;
            }

            return response;
        }
    }
}
