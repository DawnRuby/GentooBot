using HtmlAgilityPack;
using Octokit;

namespace Gentoo.Functions;

public class CommitMonitoring
{
    public static async Task<int> GetGitHubCommits(string ghusername)
    {
        var repo = await Program.GitHub.Repository.Get("gentoo", "gentoo");


        var commitRequestFilter = new CommitRequest()
        {
            Author = ghusername,
            Since = new DateTimeOffset(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Utc))
        };
        
        var commits = await Program.GitHub.Repository.Commit.GetAll("gentoo", "gentoo", commitRequestFilter);
        return commits.Count(commit => commit.Author.Login == ghusername);
    }

    public static async Task<int> GetGitHubPullRequests(string ghUsername)
    {
        var pullRequestFilter = new PullRequestRequest()
        {
            State = (ItemStateFilter)ItemState.Closed,
            SortDirection = SortDirection.Descending,
            Head = $"{ghUsername}"
        };
        
        var prs = await Program.GitHub.PullRequest.GetAllForRepository("gentoo", "gentoo", pullRequestFilter);
        return prs.Count(prs => prs.User.Login == ghUsername);
    }

    public static async Task<int> GetGentooWikiCommits(string username)
    {
        var webSite = $"https://wiki.gentoo.org/index.php?title=Special:Contributions/{username}&offset=&limit=10000&target={username}";
        HtmlWeb web = new HtmlWeb();
        var htmlDoc = await web.LoadFromWebAsync(webSite);
        var node = htmlDoc.DocumentNode.Descendants(0).Where(x => x.HasClass("mw-contributions-list"));
        var contribCount = 0;
        
        foreach (var children in node.First().ChildNodes) {
            var textArray = children.InnerText.Split('\n');
            DateTime.TryParse(textArray[0], out DateTime dateTime);
            
            if (dateTime.Month == DateTime.Now.Month && dateTime.Year == DateTime.Now.Year) {
                contribCount++;
            }
        }

        return contribCount;
    }
}