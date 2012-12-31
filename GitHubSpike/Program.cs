using System;
using System.Collections.Generic;
using NGitHub;
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
        private readonly string _userName;
        private readonly string _repositoryName;

        private readonly RestClient _restClient;
        private readonly GitHubClient _githubClient;

        public Committer(string userName, string password, string repositoryName)
        {
            _repositoryName = repositoryName;
            _userName = userName;

            _restClient = new RestClient()
            {
                BaseUrl = "https://api.github.com/",
                Authenticator = new HttpBasicAuthenticator(userName, password),
            };

            _githubClient = new GitHubClient()
            {
                Authenticator = new HttpBasicAuthenticator(userName, password)
            };
        }

        public void PleaseWork()
        {
            // this is the name of the main head we're dealing with.
            const string mainBranch = "heads/master";

            // first, get the reference to the master by its name.
            var getMasterRefRequest = CreateRequest("refs", Method.GET, "/{ref}", null);
            getMasterRefRequest.AddUrlSegment("ref", mainBranch);
            var getMasterRefResponse = _restClient.Execute<GitRef>(getMasterRefRequest);

            // using the reference to the master, get the commit that that head is pointing to.
            var getMasterCommitRequest = CreateRequest("commits", Method.GET, "/{sha}", null);
            getMasterCommitRequest.AddUrlSegment("sha", getMasterRefResponse.Data.@object.sha);
            var getMasterCommitResponse = _restClient.Execute<Commit>(getMasterCommitRequest);

            // using the commit that the head is pointing to, get the tree that we will be updating.
            var getMasterTreeRequest = CreateRequest("trees", Method.GET, "/{sha}", null);
            getMasterTreeRequest.AddUrlSegment("sha", getMasterCommitResponse.Data.tree.sha);
            var getMasterTreeResponse = _restClient.Execute<GitTree>(getMasterTreeRequest);

            // we create a new tree based on the tree specified in base_tree.
            // the tree object contains the modifications to make to that tree.
            // changes to files are specified in the "content" field.
            var createTreeRequest = CreateRequest("trees", Method.POST, new
            {
                base_tree = getMasterTreeResponse.Data.sha,
                tree = new object[] {
                    new {
                        path = DateTime.Now.Ticks + ".txt",
                        mode = "100644",
                        type = "blob",
                        content = "blob content"
                    },
                    new {
                        path = "nested/file.txt",
                        content = "I'VE EATEN IT"
                    }
                }
            });
            RestResponse<GitTree> createTreeResponse = _restClient.Execute<GitTree>(createTreeRequest);

            // create a new commit pointing to the tree we've just created.
            var addCommitRequest = CreateRequest("commits", Method.POST, new
            {
                message = "This is a test commit.",
                author = new
                {
                    name = "Bert",
                    email = "bop-bag@example.com",
                    date = DateTime.UtcNow
                },
                parents = new string[] { getMasterRefResponse.Data.@object.sha },
                tree = createTreeResponse.Data.sha
            });
            RestResponse<Commit> addCommitResponse = _restClient.Execute<Commit>(addCommitRequest);

            // point the head to the commit we just made.
            var repointHeadRequest = CreateRequest("refs", Method.PATCH, "/{ref}", new
            {
                sha = addCommitResponse.Data.sha
            });
            repointHeadRequest.AddUrlSegment("ref", mainBranch);
            var repointHeadResponse = _restClient.Execute(repointHeadRequest);
        }

        private RestRequest CreateRequest(string resource, Method method, object body)
        {
            return CreateRequest(resource, method, "", body);
        }

        private RestRequest CreateRequest(string resource, Method method, string path, object body)
        {
            var request = new RestRequest("/repos/{owner}/{repository}/git/{resource}" + path, method);
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment("owner", _userName);
            request.AddUrlSegment("repository", _repositoryName);
            request.AddUrlSegment("resource", resource);
            if (body != null)
            {
                request.AddBody(body);
            }
            return request;
        }
    }

    class Program
    {
        private static readonly PasswordEntry passwordEntry = new PasswordEntry();

        static void Main(string[] args)
        {
            const string repositoryName = "testing-github-api";
            
            Console.Write("User name: ");
            string userName = Console.ReadLine();
            Console.Write("Password: ");
            string password = passwordEntry.ReadPassword();

            var committer = new Committer(userName, password, repositoryName);
            committer.PleaseWork();

            Console.WriteLine("Press enter to stop the world");
            Console.ReadLine();
        }
    }
}
