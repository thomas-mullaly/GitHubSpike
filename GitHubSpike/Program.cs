using System;
using GitHubSpike.DTO;

namespace GitHubSpike
{
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

            var gitRequestBuilder = new GitHubRequestBuilder(ownerName, repositoryName);
            var gitHub = new GitHubWrapper(userName, password, "master", gitRequestBuilder);

            GitHubCommitter committer = gitHub.CreateCommitter();
            committer.CreateFile(DateTime.Now + ".txt", "this is a test file");
            committer.UpdateFile("nested/file.txt", "I've eaten it!");
            GitCommit commit = committer.SubmitCommit("Bert", "bert@bop-bag.org", "This is a test commit.");
            
            gitHub.MergeIn(commit, "Merged it in");

            Console.WriteLine("Press enter to stop the world");
            Console.ReadLine();
        }
    }
}
