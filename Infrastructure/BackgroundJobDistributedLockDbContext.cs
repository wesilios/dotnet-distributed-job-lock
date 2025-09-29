using Domain.Entities.JobLogs;
using Domain.Entities.QueueLocks;
using Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class BackgroundJobDistributedLockDbContext : DbContext
{
    public DbSet<QueueLock> QueueLocks { get; set; }
    public DbSet<JobLog> JobLogs { get; set; }
    
    public BackgroundJobDistributedLockDbContext(DbContextOptions<BackgroundJobDistributedLockDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new QueueLockConfiguration());
        modelBuilder.ApplyConfiguration(new JobLogConfiguration());
    }
}