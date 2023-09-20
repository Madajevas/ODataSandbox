using Default;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Client;

using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace WebAPI.Tests
{
    public class Tests
    {
        private WebApplicationFactory<Program> factory;
        private HttpClient testClient;
        private ObservingHttpMessageHandler observationHandler;
        private HttpClient observingClient;
        private Container testOdataContainer;

        [OneTimeSetUp]
        public void Setup()
        {
            factory = new WebApplicationFactory<Program>();
            testClient = factory.CreateClient();
            observationHandler = new ObservingHttpMessageHandler(testClient);
            observingClient = new HttpClient(observationHandler);           // to inspect what actually gets returned
            observingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "dG9tYXM6c2VjdXJl");

            testOdataContainer = new Container(new Uri(testClient.BaseAddress!, "odata"));
            // testOdataContainer.Credentials = new NetworkCredential("tomas", "top-secret");   // does not work like this
            testOdataContainer.HttpRequestTransportMode = HttpRequestTransportMode.HttpClient;
            testOdataContainer.Configurations.RequestPipeline.OnMessageCreating = (args) =>
            {
                var message = new HttpClientRequestMessage(args);

                message.GetType().GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .SetValue(message, observingClient);

                foreach (var header in args.Headers)
                {
                    message.SetHeader(header.Key, header.Value);
                }

                return message;
            };
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            observingClient.Dispose();
            observationHandler.Dispose();
            testClient.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task CanRetrieveTemperatures()
        {
            var temperatures = testOdataContainer.WeatherForecast
                .Select(wf => new { wf.TemperatureC });

            Assert.That(temperatures, Is.Not.Empty);
        }

        [Test]
        public async Task CanBatchRequests()
        {
            var batchResult = await testOdataContainer.ExecuteBatchAsync(
                testOdataContainer.WeatherForecast,
                testOdataContainer.WeatherForecast);

            Assert.That(batchResult.BatchStatusCode, Is.EqualTo(200));
            Assert.That(batchResult.All(res => res.StatusCode == 200));
        }
    }
}
