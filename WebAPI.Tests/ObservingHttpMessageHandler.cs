using System.Reflection;

namespace WebAPI.Tests
{
    class ObservingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpClient testClient;

        public ObservingHttpMessageHandler(HttpClient testClient)
        {
            this.testClient = testClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // reset send status so same message could be delegated to actual client
                request.GetType().GetField("_sendStatus", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(request, 0);
                var response =  await testClient.SendAsync(request, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}