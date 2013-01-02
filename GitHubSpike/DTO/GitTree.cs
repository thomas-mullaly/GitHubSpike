using System.Collections.Generic;

namespace GitHubSpike.DTO
{
    public class GitTree : GitObject
    {
        public List<GitFile> tree { get; set; }
    }
}