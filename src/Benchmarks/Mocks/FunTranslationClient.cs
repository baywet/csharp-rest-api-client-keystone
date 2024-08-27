using System.Net;
using System.Net.Http.Headers;

using Moq;
using Moq.Protected;

namespace Benchmarks.Mocks;

public static class FunTranslationClient
{
    internal const string ReturnJson =
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
#pragma warning disable CA2000 // Dispose objects before losing scope
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(ReturnJson, MediaTypeHeaderValue.Parse("application/json"))
        };
#pragma warning restore CA2000 // Dispose objects before losing scope
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