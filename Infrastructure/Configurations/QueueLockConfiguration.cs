using Domain.Entities.QueueLocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class QueueLockConfiguration : IEntityTypeConfiguration<QueueLock>
{
    public void Configure(EntityTypeBuilder<QueueLock> builder)
    {
        builder.HasKey(e => new
        {
            e.QueueName, e.JobName
        });

        builder.Property<DateTime>("CreatedTime")
            .ValueGeneratedOnAdd()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedTime")
            .HasDefaultValueSql("GetUtcDate()");
    }
}