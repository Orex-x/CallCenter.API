using Microsoft.EntityFrameworkCore;

namespace MyLongPolling.Models;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Connection> Connections { get; set; }
    
    public DbSet<Client> Clients { get; set; }
    public DbSet<Event> Events { get; set; }
    
    public DbSet<Call> Calls { get; set; }
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}