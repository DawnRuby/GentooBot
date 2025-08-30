using Gentoo.Database;
using Gentoo.Functions;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace Gentoo.Modules;

public class GreetingModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("register", "Registers you into the commit challange on discord")]
    public async Task<string> Register(User user, string githubUsername, string gentooContribName)
    {
        PgSQLContext context = new PgSQLContext();
        if (context.Users.Any(x => x.Id == user.Id))
        {
            return "You already linked your account. Stop cheating :3";
        }
        
        
        await context.Users.AddAsync(new Contributor()
        {
            Id = user.Id,
            LastUpdate = DateTime.UtcNow,
            GithubUsername = githubUsername,
            ContributionURL = gentooContribName,
            Name = user.Username
        });
        await context.SaveChangesAsync();
        
        //Get our commits and combine them :)
        var commits = CommitMonitoring.GetGentooWikiCommits(gentooContribName).Result;
        var ghCommits = CommitMonitoring.GetGitHubCommits(githubUsername).Result;
        var overallCommits = commits + ghCommits;
        var rank = context.Ranks.FirstOrDefault(x => x.Commits < overallCommits);
        
        return "Thanks for signing up, judging by your last commits you're currently placed <null>";
    }
    
    
    [SlashCommand("checkdb", "Checks if the database exists")]
    public string CheckDatabaseStatus() 
    {
        PgSQLContext context = new PgSQLContext();
        var created = context.Database.EnsureCreated();
        return created.ToString();
    }
}