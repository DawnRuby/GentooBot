using HtmlAgilityPack;
using Octokit;

namespace Gentoo.Functions;

public class CommitMonitoring
{
    private static DateTimeOffset monthOffset =
        new DateTimeOffset(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Utc));  
    
    
    public static async Task<int> GetGitHubCommits(string ghusername, DateTimeOffset? offset = null)
    {
        if (offset == null)
        {
            offset = monthOffset;
        }
        
        var commitRequestFilter = new CommitRequest()
        {
            Author = ghusername,
            Since = offset
        };
        
        var commits = await Program.GitHub.Repository.Commit.GetAll("gentoo", "gentoo", commitRequestFilter);
        return commits.Count();
    }

    public static async Task<int> GetGentooWikiCommits(string username, DateTimeOffset? offset = null)
    {
        if (offset == null) {
            offset = monthOffset;
        }
        
        var webSite = $"https://wiki.gentoo.org/index.php?title=Special:Contributions/{username}&offset=&limit=1000000&target={username}";
        HtmlWeb web = new HtmlWeb();
        var htmlDoc = await web.LoadFromWebAsync(webSite);
        var node = htmlDoc.DocumentNode.Descendants(0).Where(x => x.HasClass("mw-contributions-list"));
        var contribCount = 0;
        
        foreach (var children in node.First().ChildNodes) {
            var textArray = children.InnerText.Split('\n');
            DateTime.TryParse(textArray[0], out DateTime dateTime);
            
            if (dateTime > offset) {
                contribCount++;
            }
        }

        return contribCount;
    }
}