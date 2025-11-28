using Jobeats.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobeats.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Jobeats
/// </summary>
public class JobeatsDbContext : DbContext
{
    public JobeatsDbContext(DbContextOptions<JobeatsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<Ping> Pings => Set<Ping>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Flip> Flips => Set<Flip>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<CheckChannel> CheckChannels => Set<CheckChannel>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Check entity configuration
        modelBuilder.Entity<Check>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Slug).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Timezone).HasMaxLength(50);
            entity.Property(e => e.CronExpression).HasMaxLength(100);
            entity.Property(e => e.LastRunId).HasMaxLength(50);
            
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.AlertAfterAt);
            entity.HasIndex(e => e.ProjectId);
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Checks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Ping entity configuration
        modelBuilder.Entity<Ping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Body).HasMaxLength(10000);
            entity.Property(e => e.ExternalBodyPath).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RemoteIp).HasMaxLength(45);
            entity.Property(e => e.Method).HasMaxLength(10);
            entity.Property(e => e.RunId).HasMaxLength(50);
            
            entity.HasIndex(e => e.CheckId);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Check)
                .WithMany(c => c.Pings)
                .HasForeignKey(e => e.CheckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Project entity configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ApiKey).HasMaxLength(64).IsRequired();
            entity.Property(e => e.BadgeKey).HasMaxLength(20).IsRequired();
            
            entity.HasIndex(e => e.ApiKey).IsUnique();
            entity.HasIndex(e => e.OwnerId);
            
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Channel entity configuration
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Configuration).HasMaxLength(5000);
            entity.Property(e => e.LastError).HasMaxLength(1000);
            
            entity.HasIndex(e => e.ProjectId);
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Channels)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.ApiToken).HasMaxLength(64).IsRequired();
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(100);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ApiToken).IsUnique();
        });
        
        // Flip entity configuration
        modelBuilder.Entity<Flip>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.CheckId);
            entity.HasIndex(e => e.ProcessedAt);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Check)
                .WithMany(c => c.Flips)
                .HasForeignKey(e => e.CheckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ProjectMember entity configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
            
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // CheckChannel junction table configuration
        modelBuilder.Entity<CheckChannel>(entity =>
        {
            entity.HasKey(e => new { e.CheckId, e.ChannelId });
            
            entity.HasOne(e => e.Check)
                .WithMany()
                .HasForeignKey(e => e.CheckId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.CheckChannels)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
