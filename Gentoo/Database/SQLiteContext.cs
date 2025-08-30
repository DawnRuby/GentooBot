using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NetCord.Gateway;
using Newtonsoft.Json;

namespace Gentoo.Database;

public class SQLiteContext : DbContext
{
    internal DbSet<User> Users { get; set; }
    internal DbSet<PreviousWinners> PreviousWinner { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Gentoo.dat");
    }
}

public class PreviousWinners
{
    [Key]
    public long MonthId { get; set; }
    
    public string UserRankings { get; set; }
}

public class User
{
    /// <summary>
    /// The Discord User Id
    /// </summary>
    [Key]
    public ulong DiscordUserId { get; set; }
 
    /// <summary>
    /// The total commits for the month
    /// </summary>
    public int TotalCommitsMonth { get; set; }
    
    /// <summary>
    /// ALL commits for the user.
    /// </summary>
    public int OverallCommits { get; set; }
    
    /// <summary>
    /// The Name of the discord user
    /// </summary>
    public string DiscordUsername { get; set; }
    
    /// <summary>
    /// The users GitHub name
    /// </summary>
    public string GithubUsername { get; set; }
    
    /// <summary>
    /// The users gentoo username
    /// </summary>
    public string GentooUsername { get; set; }
}