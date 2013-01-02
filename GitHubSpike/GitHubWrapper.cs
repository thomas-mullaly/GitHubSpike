using System;
using GitHubSpike.DTO;
using RestSharp;

namespace GitHubSpike
{
    public class GitHubWrapper
    {
        public string BranchName { get; private set; }
        public RestClient RestClient { get; private set; }
        public GitHubRequestBuilder HubRequestBuilder { get; private set; }

        public GitHubWrapper(string userName, string password, string branchName, GitHubRequestBuilder gitHubRequestBuilder)
        {
            BranchName = branchName;
            HubRequestBuilder = gitHubRequestBuilder;

            RestClient = new RestClient
            {
                BaseUrl = "https://api.github.com/",
                Authenticator = new HttpBasicAuthenticator(userName, password),
            };
        }

        public GitHubCommitter CreateCommitter()
        {
            // this is the name of the main head we're dealing with.
            string mainHead = "heads/" + BranchName;

            // first, get the reference to the master by its name.
            var getMasterRefRequest = HubRequestBuilder.CreateRequest("refs", Method.GET, "/{ref}", null);
            getMasterRefRequest.AddUrlSegment("ref", mainHead);
            var getMasterRefResponse = RestClient.Execute<GitRef>(getMasterRefRequest);

            // using the reference to the master, get the commit that that head is pointing to.
            var getMasterCommitRequest = HubRequestBuilder.CreateRequest("commits", Method.GET, "/{sha}", null);
            getMasterCommitRequest.AddUrlSegment("sha", getMasterRefResponse.Data.@object.sha);
            var getMasterCommitResponse = RestClient.Execute<GitCommit>(getMasterCommitRequest);

            // using the commit that the head is pointing to, get the tree that we will be updating.
            var getMasterTreeRequest = HubRequestBuilder.CreateRequest("trees", Method.GET, "/{sha}", null);
            getMasterTreeRequest.AddUrlSegment("sha", getMasterCommitResponse.Data.tree.sha);
            var getMasterTreeResponse = RestClient.Execute<GitTree>(getMasterTreeRequest);

            return new GitHubCommitter(this, getMasterTreeResponse.Data, getMasterRefResponse.Data);
        }

        public void RepointHeadTo(GitCommit commit)
        {
            throw new NotImplementedException();
        }

        public void MergeIn(GitCommit commit, string message)
        {
            var mergeRequest = HubRequestBuilder.CreateMergeRequest(BranchName, commit.sha, message);
            var mergeInResponse = RestClient.Execute(mergeRequest);
        }
    }
}