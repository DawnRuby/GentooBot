using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Gentoo.Database;

public class SQLiteContext : DbContext
{
    private DbSet<User> Users { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Gentoo.dat");
    }
}

public class User
{
    /// <summary>
    /// The Discord User Id
    /// </summary>
    public ulong DiscordUserId { get; set; }
 
    /// <summary>
    /// The total commits for the month
    /// </summary>
    public int TotalCommits { get; set; }
    
    /// <summary>
    /// The Name of the discord user
    /// </summary>
    public string DiscordUsername { get; set; }
    
    /// <summary>
    /// The users github name
    /// </summary>
    public string GithubUsername { get; set; }
    
    /// <summary>
    /// The users gentoo username
    /// </summary>
    public string GentooUsername { get; set; }
}