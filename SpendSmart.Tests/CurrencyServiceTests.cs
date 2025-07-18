using Moq;
using Moq.Protected;
using SpendSmart.Models.External;
using SpendSmart.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SpendSmart.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetRatesAsync_ShouldReturnExchangeRateResponse()
        {
            // Arrange - fake response
            var fakeResponse = new ExchangeRateResponse
            {
                Base = "USD",
                Date = DateTime.UtcNow,
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85M },
                    { "INR", 82.5M }
                }
            };

            var json = JsonSerializer.Serialize(fakeResponse);

            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new CurrencyService(httpClient);

            // Act
            var result = await service.GetRatesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("USD", result.Base);
            Assert.True(result.Rates.ContainsKey("EUR"));
            Assert.Equal(0.85M, result.Rates["EUR"]);
        }
        [Fact]
        public async Task GetRatesAsync_ShouldThrowException_WhenResponseIsNull()
        {
            // Arrange - mock handler returns null JSON
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null")  // Simulates a null deserialization
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new CurrencyService(httpClient);

            // Act & Assert - should throw exception
            await Assert.ThrowsAsync<Exception>(() => service.GetRatesAsync());
        }
    }
}
