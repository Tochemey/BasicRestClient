using System.Threading.Tasks;
using BasicRestClient.RestClient;
using HttpMock;
using Xunit;

namespace BasicHttpClient.Test
{
    public class RestClientTest
    {
        public RestClientTest() { _server = HttpMockRepository.At(BaseUrl); }

        private readonly IHttpServer _server;
        private const string BaseUrl = "http://localhost:9191";

        [Fact]
        public async Task GetAsyncShouldReturnSuccessfully()
        {
            const string expected = "<xml>response>Hello World</response></xml>";
            _server.Stub(x => x.Get("/endpoint")).Return(expected).OK();

            var client = new RestClient(BaseUrl);
            var response = await client.GetAsync("/endpoint",
                "application/xml");
            Assert.NotNull(response);
            Assert.True(response.Status == 200);
        }


        [Fact]
        public void GetShouldReturnSuccessfully()
        {
            const string expected = "<xml>response>Hello World</response></xml>";
            _server.Stub(x => x.Get("/endpoint")).Return(expected).OK();

            var client = new RestClient(BaseUrl);
            var response = client.Get("/endpoint");
            Assert.NotNull(response);
            Assert.True(response.Status == 200);
        }

        [Fact]
        public async Task PostRequestShouldReturnSuccessfully()
        {
            const string expected = "<xml>response>Hello World</response></xml>";
            _server.Stub(x => x.Post("/endpoint")).Return(expected).OK();

            var client = new RestClient(BaseUrl);
            var parameters = new ParameterMap();
            parameters.Set("From",
                "Arsene").Set("To",
                "+233248067917").Set("Content",
                "Hello ").Set("RegisteredDelivery",
                "true");

            var response = await client.PostAsync("/endpoint",
                "application/xml",
                parameters);
            Assert.NotNull(response);
            Assert.True(response.Status == 200);
        }
    }
}