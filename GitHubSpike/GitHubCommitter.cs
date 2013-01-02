using System;
using System.Collections.Generic;
using GitHubSpike.DTO;
using RestSharp;

namespace GitHubSpike
{
    public class GitHubCommitter
    {
        private readonly GitHubWrapper _wrapper;
        private readonly GitTree _masterTree;
        private readonly GitRef _masterRef;
        private readonly IList<object> _pendingChanges;
        private bool _used;

        public GitHubCommitter(GitHubWrapper wrapper, GitTree masterTree, GitRef masterRef)
        {
            _wrapper = wrapper;
            _masterTree = masterTree;
            _masterRef = masterRef;
            _pendingChanges = new List<object>();
        }

        public void CreateFile(string path, string content)
        {
            UsedGuard();
            _pendingChanges.Add(new { path, content, mode = "100644", type = "blob" });
        }

        public void UpdateFile(string path, string content)
        {
            UsedGuard();
            _pendingChanges.Add(new {path, content });
        }

        public GitCommit Commit(string name, string email, string message)
        {
            return Commit(name, email, message, DateTime.UtcNow);
        }

        public GitCommit Commit(string name, string email, string message, DateTime date)
        {
            UsedGuard();

            // we create a new tree based on the tree specified in base_tree.
            // the tree object contains the modifications to make to that tree.
            // changes to files are specified in the "content" field.
            var createTreeRequest = _wrapper.HubRequestBuilder.CreateRequest("trees", Method.POST, new
                {
                    base_tree = _masterTree.sha,
                    tree = _pendingChanges
                });
            RestResponse<GitTree> createTreeResponse = _wrapper.RestClient.Execute<GitTree>(createTreeRequest);

            // create a new commit pointing to the tree we've just created.
            var addCommitRequest = _wrapper.HubRequestBuilder.CreateRequest("commits", Method.POST, new
            {
                message = message,
                author = new { name, email, date },
                parents = new[] { _masterRef.@object.sha },
                tree = createTreeResponse.Data.sha
            });
            RestResponse<GitCommit> addCommitResponse = _wrapper.RestClient.Execute<GitCommit>(addCommitRequest);

            _used = true;
            return addCommitResponse.Data;
        }

        private void UsedGuard()
        {
            if (_used)
            {
                throw new InvalidOperationException("Commit has already been submitted.");
            }
        }
    }
}