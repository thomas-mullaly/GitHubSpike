namespace GitHubSpike.DTO
{
    public class GitFile : GitObject
    {
        public string path { get; set; }
        public string mode { get; set; }
        public string type { get; set; }
        public int size { get; set; }
    }
}