using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.JobLogs;

[Table("JobLogs")]
public partial class JobLog
{
    [Key] [Column("Id")] public long Id { get; set; }

    [Column("AppId")] public Guid AppId { get; set; }

    [Column("JobName", TypeName = "nvarchar(100)")]
    public string JobName { get; set; }

    [Column("Status", TypeName = "nvarchar(35)")]
    public JobLogStatus Status { get; set; }
    
    [Column("Remark", TypeName = "nvarchar(max)")]
    public string Remark { get; set; }

    [Column("CreatedTime", TypeName = "datetime2")]
    public DateTime CreatedTime { get; set; }

    [Column("UpdatedTime", TypeName = "datetime2")]
    public DateTime UpdatedTime { get; set; }
}