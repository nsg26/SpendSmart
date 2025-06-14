using SpendSmart.Models.External;

namespace SpendSmart.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://open.er-api.com/v6/latest/USD";

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExchangeRateResponse> GetRatesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>(BaseUrl);
            if (response == null)
                throw new Exception("Failed to retrieve exchange rates.");
            return response;
        }
    }
}
