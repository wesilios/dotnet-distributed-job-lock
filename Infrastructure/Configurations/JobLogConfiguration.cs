using Domain.Entities.JobLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class JobLogConfiguration : IEntityTypeConfiguration<JobLog>
{
    public void Configure(EntityTypeBuilder<JobLog> builder)
    {
        builder.Property(e => e.Status)
            .HasConversion(
                x => x.ToString(),
                x => (JobLogStatus)Enum.Parse(typeof(JobLogStatus), x))
            .HasDefaultValue(JobLogStatus.Started);

        
        builder.Property(x => x.CreatedTime)
            .ValueGeneratedOnAdd()
            .HasColumnType("datetime2")
            .HasColumnName("CreatedTime")
            .HasDefaultValueSql("GetUtcDate()");
        
        builder.Property(x => x.UpdatedTime)
            .ValueGeneratedOnAdd()
            .HasColumnType("datetime2")
            .HasColumnName("UpdatedTime")
            .HasDefaultValueSql("GetUtcDate()");
    }
}