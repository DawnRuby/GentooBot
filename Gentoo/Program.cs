using System.Collections.Immutable;
using System.Timers;
using Gentoo.Database;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using Octokit;
using ProductHeaderValue = Octokit.GraphQL.ProductHeaderValue;
using Gentoo.Functions;
using Gentoo.Modules;
using Newtonsoft.Json;
using Octokit.Internal;
using User = Gentoo.Database.User;

namespace Gentoo;


public class Program
{
    public static DateTime LastUpdate;
    private static System.Timers.Timer UpdateTimer;
#if DEBUG
    public const ulong LoggingChannelId = 1411474180939059361;
#else
    public const ulong LoggingChannelId = 0;
#endif
    
    
    public static GitHubClient GitHub
    {
        get; 
        private set;
    }
    
    
    public static void Main(string[] args)
    {
        UpdateDb(null, null);
        //SetTimer();
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        GitHub = new GitHubClient(
            Octokit.ProductHeaderValue.Parse("Gentoo"), 
            new InMemoryCredentialStore(new Credentials(configuration["GhSecret"]))
            );
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services
            .AddDiscordGateway()
            .AddApplicationCommands();
        
        var host = builder.Build();
        host.AddModules(typeof(Program).Assembly);
        host.UseGatewayHandlers();
        host.RunAsync().GetAwaiter().GetResult();
    }
    private static void SetTimer()
   {
        // Create a timer with a two second interval.
        UpdateTimer = new System.Timers.Timer(2000000);
        // Hook up the Elapsed event for the timer. 
        UpdateTimer.Elapsed += UpdateDb;
        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
        //UpdateTimer.Start();
    }

    private static void UpdateDb(object? sender, ElapsedEventArgs e)
    {
        var context = new SQLiteContext();
        
        //We switched over months, time to roll over :)
        if (DateTime.Now.Month > LastUpdate.Month)
        {
            var winners = GreetingModule.GetUsersByRankMonthAsync(context).GetAwaiter().GetResult();
            var winnerList = new List<User>();
            for (var index = 0; index < winners.Count; index++)
            {
                if (index >= 10) { break; }
                winnerList.Add(winners[index]);
            }

            var serializedTop10 = JsonConvert.SerializeObject(winnerList, Formatting.None);
            context.PreviousWinner.Add(new PreviousWinners
            {
                MonthId = LastUpdate.ToBinary(),
                UserRankings = serializedTop10
            });
            context.SaveChanges();
        }
        
        LastUpdate = DateTime.Now;

        context.Database.EnsureCreated();
        var users = context.Users.ToList();

        foreach (var user in users)
        {
            var commits = 0;
            var overallCommits = 0;
            var baseStartTime = new DateTimeOffset(DateTime.MinValue);
            try {
                commits += CommitMonitoring.GetGentooWikiCommits(user.GentooUsername).GetAwaiter().GetResult();
                overallCommits += CommitMonitoring.GetGentooWikiCommits(user.GentooUsername, baseStartTime).GetAwaiter().GetResult();
            }
            catch (Exception) {
                // ignored
            }

            try {
                commits += CommitMonitoring.GetGitHubCommits(user.GithubUsername).GetAwaiter().GetResult();
                overallCommits += CommitMonitoring.GetGitHubCommits(user.GithubUsername, baseStartTime).GetAwaiter().GetResult();
            }
            catch (Exception) {
                // ignored
            }

            user.TotalCommitsMonth = commits;
            user.OverallCommits = overallCommits;
            context.Update(user);
        }
        
        Console.WriteLine($"Update script finished running, updating {users.Count} users...");
        context.SaveChanges();
    }
}