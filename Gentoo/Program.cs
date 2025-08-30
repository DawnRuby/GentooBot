using Gentoo.Database;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using Octokit;
using ProductHeaderValue = Octokit.GraphQL.ProductHeaderValue;
using Gentoo.Functions;
using Octokit.Internal;

namespace Gentoo;


public class Program
{
    public static GitHubClient GitHub
    {
        get; 
        private set;
    }
    
    
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        GitHub = new GitHubClient(
            Octokit.ProductHeaderValue.Parse("Gentoo"), 
            new InMemoryCredentialStore(new Credentials(configuration["GhSecret"]))
            );
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services
            .AddDiscordGateway()
            .AddApplicationCommands();

        var commits = CommitMonitoring.GetGentooWikiCommits("immolo");
        var commits2 = CommitMonitoring.GetGitHubCommits("immolo").GetAwaiter().GetResult();
        var prs = CommitMonitoring.GetGitHubPullRequests("immolo").GetAwaiter().GetResult();
        
        var host = builder.Build();
        host.AddModules(typeof(Program).Assembly);
        host.UseGatewayHandlers();
        host.RunAsync().GetAwaiter().GetResult();
    }
    
}