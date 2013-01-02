using RestSharp;

namespace GitHubSpike
{
    public class GitHubRequestBuilder
    {
        private readonly string _ownerName;
        private readonly string _repositoryName;

        public GitHubRequestBuilder(string ownerName, string repositoryName)
        {
            _ownerName = ownerName;
            _repositoryName = repositoryName;
        }

        public RestRequest CreateRequest(string resource, Method method, object body)
        {
            return CreateRequest(resource, method, "", body);
        }

        public RestRequest CreateRequest(string resource, Method method, string path, object body)
        {
            var request = new RestRequest("/repos/{owner}/{repository}/git/{resource}" + path, method);
            SetStandardParameters(request);
            request.AddUrlSegment("resource", resource);
            if (body != null)
            {
                request.AddBody(body);
            }
            return request;
        }

        public RestRequest CreateMergeRequest(string destination, string source, string commitMessage)
        {
            var request = new RestRequest("/repos/{owner}/{repository}/merges", Method.POST);
            SetStandardParameters(request);
            request.AddBody(new
            {
                @base = destination,
                head = source,
                commit_message = commitMessage
            });
            return request;
        }

        public void SetStandardParameters(RestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment("owner", _ownerName);
            request.AddUrlSegment("repository", _repositoryName);
        }
    }
}