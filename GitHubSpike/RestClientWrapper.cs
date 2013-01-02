using RestSharp;

namespace GitHubSpike
{
    public class RestClientWrapper
    {
        private readonly RestClient _restClient;

        public RestClientWrapper(string userName, string password)
        {
            _restClient = new RestClient
            {
                BaseUrl = "https://api.github.com/",
                Authenticator = new HttpBasicAuthenticator(userName, password),
            };
        }

        public RestResponse<T> Execute<T>(RestRequest request)
            where T : new()
        {
            RestResponse<T> response = _restClient.Execute<T>(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response;
        }
    }
}