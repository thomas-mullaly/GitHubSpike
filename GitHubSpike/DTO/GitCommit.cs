using System.Collections.Generic;

namespace GitHubSpike.DTO
{
    public class GitCommit : GitObject
    {
        public string sha { get; set; }
        public string url { get; set; }
        public string message { get; set; }

        public GitAuthor author { get; set; }
        public GitAuthor committer { get; set; }

        public GitObject tree { get; set; }
        public List<GitObject> parents { get; set; }
    }
}