using System.Globalization;
using Gentoo.Database;
using Gentoo.Functions;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Octokit;
using User = Gentoo.Database.User;

namespace Gentoo.Modules;

public class GreetingModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("leaderboard", "Gets the leaderboard of the month", 
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> Leaderboard()
    {
        var context = new SQLiteContext();
        var users = await GetUsersByRankMonthAsync(context);
        var guild = await Context.Guild.GetAsync();
        
        var returnString = $"**Top 10 Wiki & PR commits to Gentoo for {DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}**\n\n";
        for (var i = 0; i < users.Count; i++)
        {
            if (i >= 10) { break; }
            var user = users[i];
            
            var discordUser = await guild.GetUserAsync(user.DiscordUserId);
            returnString += $"**{i + 1}.** - {discordUser.Username} - Commit Count: {user.TotalCommitsMonth}";
            returnString += "\n";
        }
        
        return returnString;
    }
    
    
    [SlashCommand("alltimeleaderboard", "Gets the leaderboard of the month", 
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> AllTimeLeaderboard()
    {
        var context = new SQLiteContext();
        var users = await context.Users.ToListAsync();
        users.Sort(new Comparison<User>((user1, user2) => user2.OverallCommits.CompareTo(user1.OverallCommits)));
        var guild = await Context.Guild.GetAsync();
        
        var returnString = $"**Top 10 Wiki & PR commits to Gentoo in all time**\n\n";
        for (var i = 0; i < users.Count; i++)
        {
            if (i >= 10) { break; }
            var user = users[i];
            
            var discordUser = await guild.GetUserAsync(user.DiscordUserId);
            returnString += $"**{i + 1}.** - {discordUser.Username} - Commit Count: {user.OverallCommits}";
            returnString += "\n";
        }
        
        return returnString;
    }
    
    internal static async Task<List<User>> GetUsersByRankMonthAsync(SQLiteContext context)
    {
        var users = await context.Users.ToListAsync();
        users.Sort(new Comparison<User>((user1, user2) => user2.TotalCommitsMonth.CompareTo(user1.TotalCommitsMonth)));
        return users;
    }
    
    [SlashCommand("register", "Registers you into the commit challange on discord", 
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> Register(string githubUsername, string gentooContribName)
    {
        SQLiteContext context = new SQLiteContext();
        if (context.Users.Any(x => x.DiscordUserId == Context.User.Id))
        {
            return "You already linked your account. Stop cheating :3";
        }

        if (context.Users.Any(x => x.GithubUsername == githubUsername || x.GentooUsername == gentooContribName))
        {
            return "There is already an account linked to your github or gentoo username. " +
                   "If someone else claimed it please let moderators know to remove it.";
        }
        
        var errors = string.Empty;
        
        await context.Users.AddAsync(new User()
        {
            DiscordUserId = Context.User.Id,
            GithubUsername = githubUsername,
            GentooUsername = gentooContribName,
            DiscordUsername = Context.User.Username
        });
        
        await context.SaveChangesAsync();
        
        return "Thanks for signing up, updates run every half hour :)";
    }

    [SlashCommand("getrank", "Gets your current rank from the last update", 
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> GetRank()
    {
        var context = new SQLiteContext();
        if (!context.Users.Any(x => x.DiscordUserId == Context.User.Id))
        {
            return "You are not yet registered please use the Register command to register";
        }

        var users = await GetUsersByRankMonthAsync(context);
        var ourUser = users.FirstOrDefault(x => x.DiscordUserId == Context.User.Id);
        if (ourUser == null)
        {
            return "You are not yet registered please use the Register command to register";
        }
        
        var rank = users.IndexOf(ourUser);
        return $"Your current rank is {rank + 1} of {users.Count}. The Last update was done {(DateTime.Now - Program.LastUpdate).TotalMinutes} minutes ago.";
    }
    
    [SlashCommand("unlink", "Unlinks your account",  
    Contexts = [InteractionContextType.Guild])]
    public async Task<string> Unlink() 
    {
        var context = new SQLiteContext();
        var dbUser = context.Users.FirstOrDefault(x => x.DiscordUserId == Context.User.Id);
        if (dbUser == null)
        {
            return "We don't have you in our database";
        }
        
        context.Users.Remove(dbUser);
        await context.SaveChangesAsync();

        return "Unlinked user, deleted your data :)";
    }
    
    [SlashCommand("unlink", "Unlinks a users account",     
        DefaultGuildUserPermissions = Permissions.ManageGuild,
    Contexts = [InteractionContextType.Guild])]
    public async Task<string> UnlinkUser(NetCord.User user) 
    {
        var context = new SQLiteContext();
        var dbUser = context.Users.FirstOrDefault(x => x.DiscordUserId == user.Id);
        if (dbUser == null)
        {
            return "User does not exist in our database";
        }
        
        context.Users.Remove(dbUser);
        await context.SaveChangesAsync();

        return "Unlinked user";
    }
    
    [SlashCommand("userinfo", "Gets the information of a user",     
        DefaultGuildUserPermissions = Permissions.ManageGuild,
    Contexts = [InteractionContextType.Guild])]
    public async Task<string> UserInfo(NetCord.User user) 
    {
        SQLiteContext context = new SQLiteContext();
        var contributorInformation = context.Users.FirstOrDefault(x => x.DiscordUserId == user.Id);
        if (user == null)
        {
            return "User does not exist in our database";
        }

        return $"Users github: {contributorInformation.GithubUsername}, Users Gentoo Username: {contributorInformation.GentooUsername}";
    }
    
    [SlashCommand("checkdb", "Checks if the database exists", 
    DefaultGuildUserPermissions = Permissions.ManageGuild,
    Contexts = [InteractionContextType.Guild])]
    public async Task<string> CheckDatabaseStatus() 
    {
        SQLiteContext context = new SQLiteContext();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();    
        }

        return "Database is up to date";
    }
}