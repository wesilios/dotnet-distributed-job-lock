using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.QueueLocks;

[Table("QueueLocks")]
public partial class QueueLock
{
    [Column("QueueName", TypeName = "nvarchar(100)")]
    public string QueueName { get; set; }
    
    [Column("JobName", TypeName = "nvarchar(100)")]
    public string JobName { get; set; }
    
    [Column("CreatedTime", TypeName = "datetime2")]
    public DateTime CreatedTime { get; set; }
}