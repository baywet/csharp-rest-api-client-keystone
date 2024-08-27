using System.Net;
using System.Net.Http.Headers;

using Moq;
using Moq.Protected;

namespace benchmark.Mocks;

public static class FunTranslationClient
{
    private const string ReturnJson =
        """
		{
			"success": {
				"total": 1
			},
			"contents": {
				"translated": "Lost a planet,  master obiwan has.",
				"text": "Master Obiwan has lost a planet.",
				"translation": "yoda"
			}
		}
		""";
    public static HttpClient GetHttpClient()
    {
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(ReturnJson, MediaTypeHeaderValue.Parse("application/json"))
        };
        var mockMessageHandler = new Mock<HttpMessageHandler>();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        mockMessageHandler
        .Protected()
            .Setup("Dispose", ItExpr.IsAny<bool>())
            .Callback(responseMessage.Dispose);

        return new HttpClient(mockMessageHandler.Object);
    }
}