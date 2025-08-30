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

namespace Gentoo;


public class Program
{
    public static DateTime LastUpdate;
    private static System.Timers.Timer UpdateTimer;
    
    public static GitHubClient GitHub
    {
        get; 
        private set;
    }
    
    
    public static void Main(string[] args)
    {
        SetTimer();
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
        UpdateTimer = new System.Timers.Timer(new TimeSpan(0,30,0));
        // Hook up the Elapsed event for the timer. 
        UpdateTimer.Elapsed += UpdateDb;
        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
        UpdateTimer.Start();
    }

    private static void UpdateDb(object? sender, ElapsedEventArgs e)
    {
        var context = new SQLiteContext();
        
        //We switched over months, time to roll over :)
        if (DateTime.Now.Month > LastUpdate.Month)
        {
            var winners = GreetingModule.GetUsersByRankAsync(context).GetAwaiter().GetResult();
            var top10 = winners.Take(10);
            var serializedTop10 = JsonConvert.SerializeObject(top10, Formatting.None);
            context.PreviousWinner.Add(new PreviousWinners
            {
                MonthId = LastUpdate.ToFileTime(),
                UserRankings = serializedTop10
            });
            context.SaveChanges();
        }
        
        LastUpdate = DateTime.Now;

        context.Database.EnsureCreated();
        var users = context.Users.ToList();

        foreach (var user in users)
        {
            user.TotalCommits = CommitMonitoring.GetGentooWikiCommits(user.GentooUsername).GetAwaiter().GetResult() + 
                                CommitMonitoring.GetGitHubCommits(user.GithubUsername).GetAwaiter().GetResult();
        }
        
        context.SaveChanges();
    }
}