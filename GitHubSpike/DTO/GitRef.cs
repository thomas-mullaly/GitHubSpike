namespace GitHubSpike.DTO
{
    public class GitRef
    {
        public string @ref { get; set; }
        public string url { get; set; }
        public GitObjectInfo @object { get; set; }
    }
}