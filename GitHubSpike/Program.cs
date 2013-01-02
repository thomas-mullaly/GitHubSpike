using System;
using System.Collections.Generic;
using RestSharp;

namespace GitHubSpike
{
    public class GitObject
    {
        public string url { get; set; }
        public string sha { get; set; }
    }

    public class Author
    {
        public DateTime date { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class Commit : GitObject
    {
        public string sha { get; set; }
        public string url { get; set; }
        public string message { get; set; }

        public Author author { get; set; }
        public Author committer { get; set; }

        public GitObject tree { get; set; }
        public List<GitObject> parents { get; set; }
    }

    public class ObjectInfo : GitObject
    {
        public string type { get; set; }
    }

    public class GitRef
    {
        public string @ref { get; set; }
        public string url { get; set; }
        public ObjectInfo @object { get; set; }
    }

    public class GitFile : GitObject
    {
        public string path { get; set; }
        public string mode { get; set; }
        public string type { get; set; }
        public int size { get; set; }
    }

    public class GitTree : GitObject
    {
        public List<GitFile> tree { get; set; }
    }

    public class Committer
    {
        private readonly string _ownerName;
        private readonly string _repositoryName;

        private readonly RestClient _restClient;

        private readonly IList<object> _pendingCommits;

        public Committer(string userName, string password, string ownerName, string repositoryName)
        {
            _repositoryName = repositoryName;
            _ownerName = ownerName;

            _restClient = new RestClient
            {
                BaseUrl = "https://api.github.com/",
                Authenticator = new HttpBasicAuthenticator(userName, password),
            };

            _pendingCommits = new List<object>();
        }

        public void CreateFile(string path, string content)
        {
            _pendingCommits.Add(new { path, content, mode = "100644", type = "blob" });
        }

        public void UpdateFile(string path, string content)
        {
            _pendingCommits.Add(new {path, content });
        }

        public void SubmitCommit(string name, string email, string message)
        {
            SubmitCommit(name, email, message, DateTime.UtcNow);
        }

        public void SubmitCommit(string name, string email, string message, DateTime date)
        {
            // this is the name of the main head we're dealing with.
            const string mainBranch = "master";
            const string mainHead = "heads/" + mainBranch;

            // first, get the reference to the master by its name.
            var getMasterRefRequest = CreateGitRequest("refs", Method.GET, "/{ref}", null);
            getMasterRefRequest.AddUrlSegment("ref", mainHead);
            var getMasterRefResponse = _restClient.Execute<GitRef>(getMasterRefRequest);

            // using the reference to the master, get the commit that that head is pointing to.
            var getMasterCommitRequest = CreateGitRequest("commits", Method.GET, "/{sha}", null);
            getMasterCommitRequest.AddUrlSegment("sha", getMasterRefResponse.Data.@object.sha);
            var getMasterCommitResponse = _restClient.Execute<Commit>(getMasterCommitRequest);

            // using the commit that the head is pointing to, get the tree that we will be updating.
            var getMasterTreeRequest = CreateGitRequest("trees", Method.GET, "/{sha}", null);
            getMasterTreeRequest.AddUrlSegment("sha", getMasterCommitResponse.Data.tree.sha);
            var getMasterTreeResponse = _restClient.Execute<GitTree>(getMasterTreeRequest);

            // we create a new tree based on the tree specified in base_tree.
            // the tree object contains the modifications to make to that tree.
            // changes to files are specified in the "content" field.
            var createTreeRequest = CreateGitRequest("trees", Method.POST, new
            {
                base_tree = getMasterTreeResponse.Data.sha,
                tree = _pendingCommits
            });
            RestResponse<GitTree> createTreeResponse = _restClient.Execute<GitTree>(createTreeRequest);

            // create a new commit pointing to the tree we've just created.
            var addCommitRequest = CreateGitRequest("commits", Method.POST, new
            {
                message = message,
                author = new { name, email, date },
                parents = new[] { getMasterRefResponse.Data.@object.sha },
                tree = createTreeResponse.Data.sha
            });
            RestResponse<Commit> addCommitResponse = _restClient.Execute<Commit>(addCommitRequest);

            // point the head to the commit we just made.
            //var repointHeadRequest = CreateRequest("refs", Method.PATCH, "/{ref}", new
            //{
            //    sha = addCommitResponse.Data.sha
            //});
            //repointHeadRequest.AddUrlSegment("ref", mainBranch);
            //var repointHeadResponse = _restClient.Execute(repointHeadRequest);

            // merge our commit back into master
            var mergeRequest = CreateMergeRequest(mainBranch, addCommitResponse.Data.sha, "Merged changes into master");
            var mergeResponse = _restClient.Execute(mergeRequest);

            _pendingCommits.Clear();
        }

        private RestRequest CreateMergeRequest(string destination, string source, string commitMessage)
        {
            var request = new RestRequest("/repos/{owner}/{repository}/merges", Method.POST);
            SetStandardParameters(request);
            request.AddBody(new {
                @base = destination,
                head = source,
                commit_message = commitMessage
            });
            return request;
        }

        private RestRequest CreateGitRequest(string resource, Method method, object body)
        {
            return CreateGitRequest(resource, method, "", body);
        }

        private RestRequest CreateGitRequest(string resource, Method method, string path, object body)
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
  
        private void SetStandardParameters(RestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment("owner", _ownerName);
            request.AddUrlSegment("repository", _repositoryName);
        }
    }

    class Program
    {
        private static readonly PasswordEntry PasswordEntry = new PasswordEntry();

        static void Main(string[] args)
        {
            const string ownerName = "cawagner";
            const string repositoryName = "testing-github-api";
            
            Console.Write("User name: ");
            string userName = Console.ReadLine();
            Console.Write("Password: ");
            string password = PasswordEntry.ReadPassword();

            var committer = new Committer(userName, password, ownerName, repositoryName);
            committer.CreateFile(DateTime.Now + ".txt", "this is a test file");
            committer.UpdateFile("nested/file.txt", "I've eaten it!");
            committer.SubmitCommit("Bert", "bert@bop-bag.org", "This is a test commit.");

            Console.WriteLine("Press enter to stop the world");
            Console.ReadLine();
        }
    }
}
