# Background Job Distributed Lock System

A demonstration and reference implementation of distributed locking for background jobs in .NET applications, ensuring that scheduled jobs run only once across multiple application instances.

## 🎯 Problem Statement

In distributed systems with multiple application instances, background jobs can execute simultaneously across different instances, leading to:
- Duplicate processing
- Resource conflicts
- Data inconsistency
- Performance degradation

This system provides a robust solution using database-based distributed locking.

## 🏗️ Architecture

### System Overview
```
┌─────────────────┐    ┌─────────────────┐
│  Instance One   │    │  Instance Two   │
│                 │    │                 │
│ ┌─────────────┐ │    │ ┌─────────────┐ │
│ │ Hangfire    │ │    │ │ Hangfire    │ │
│ │ Jobs        │ │    │ │ Jobs        │ │
│ └─────────────┘ │    │ └─────────────┘ │
│ ┌─────────────┐ │    │ ┌─────────────┐ │
│ │ Coravel     │ │    │ │ Coravel     │ │
│ │ Jobs        │ │    │ │ Jobs        │ │
│ └─────────────┘ │    │ └─────────────┘ │
└─────────────────┘    └─────────────────┘
         │                       │
         └───────────┬───────────┘
                     │
         ┌─────────────────────┐
         │   Shared Database   │
         │                     │
         │ ┌─────────────────┐ │
         │ │   QueueLocks    │ │
         │ │   (Composite    │ │
         │ │   Primary Key)  │ │
         │ └─────────────────┘ │
         │ ┌─────────────────┐ │
         │ │    JobLogs      │ │
         │ └─────────────────┘ │
         └─────────────────────┘
```

### Project Structure
```
├── Application.InstanceOne/          # Demo instance 1
├── Application.InstanceTwo/          # Demo instance 2
├── Application.MigrationApp/         # Database migrations
├── Application.Services/             # Business logic & job services
│   ├── JobServices/                  # Background job implementations
│   │   ├── HeartbeatHangFireJob.cs  # Hangfire job with distributed lock
│   │   └── HeartbeatCoravelJob.cs   # Coravel job with distributed lock
│   ├── EnqueueService.cs            # Job enqueueing service
│   └── HangfireHostedService.cs     # Hangfire lifecycle management
├── Domain/                          # Domain entities and contracts
│   ├── Entities/
│   │   ├── QueueLocks/              # Distributed lock entity
│   │   └── JobLogs/                 # Job execution logging
│   └── Repositories/                # Repository interfaces
└── Infrastructure/                  # Data access implementation
    ├── Repositories/                # EF Core & Dapper repositories
    ├── Configurations/              # Entity configurations
    └── Migrations/                  # Database migrations
```

## 🔧 Core Components

### 1. Distributed Lock Entity
```csharp
public class QueueLock
{
    public string QueueName { get; set; }    // Queue identifier
    public string JobName { get; set; }      // Job identifier
    public DateTime CreatedTime { get; set; } // Lock timestamp
}
// Composite Primary Key: (QueueName, JobName)
```

### 2. Job Execution Logging
```csharp
public class JobLog
{
    public long Id { get; set; }
    public Guid AppId { get; set; }          // Instance identifier
    public string JobName { get; set; }
    public JobLogStatus Status { get; set; }  // Processing, Completed, Exited
    public string Remark { get; set; }       // Execution details
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
}
```

### 3. Lock Acquisition Algorithm
1. **Attempt Lock Creation**: Insert record with unique constraint
2. **Handle Conflicts**: Catch unique constraint violations
3. **Check Existing Locks**: Verify if lock exists and check timeout
4. **Timeout Cleanup**: Remove stale locks (>5 minutes)
5. **Execute Job**: Process business logic
6. **Release Lock**: Delete lock record on completion/failure

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Environment & Dependencies
- **Target Framework**: .NET 8.0
- **Hangfire Version**: 1.8.14
- **Coravel Version**: 5.0.3
- **Entity Framework Core**: 8.0.0
- **Dapper**: 2.1.35
- **Serilog**: 4.0.1

### Supported Platforms
- Windows 10/11, Windows Server 2019/2022
- Linux (Ubuntu 20.04+, RHEL 8+, Alpine 3.17+)
- macOS 12.0+ (Monterey)
- Docker containers (linux/amd64, linux/arm64)

### Setup Instructions

1. **Clone Repository**
   ```bash
   git clone <repository-url>
   cd background-job-distributed-lock-demo
   ```

2. **Database Setup**
   ```bash
   # Update connection strings in appsettings.json files
   # Run migrations
   dotnet run --project Application.MigrationApp
   ```

3. **Run Multiple Instances**
   ```bash
   # Terminal 1 - Instance One (Port 5001)
   dotnet run --project Application.InstanceOne

   # Terminal 2 - Instance Two (Port 5002)
   dotnet run --project Application.InstanceTwo
   ```

4. **Access Applications**
   - Instance One: `https://localhost:5001`
   - Instance Two: `https://localhost:5002`
   - Hangfire Dashboard: `https://localhost:5001/hangfire`

## 📋 Usage Examples

### Manual Job Triggering
```bash
# Trigger Hangfire job from Instance One
curl -X POST https://localhost:5001/enqueue-hangfire-heartbeat-jb

# Trigger Coravel job from Instance Two
curl -X POST https://localhost:5002/enqueue-coravel-heartbeat-jb
```

### Scheduled Jobs
- **Hangfire v1.8.14**: Configured to run hourly via `Cron.Hourly` with SQL Server persistence
- **Coravel v5.0.3**: Configured to run hourly via `.Hourly()` with in-memory scheduling

### Monitoring
- Check logs for lock acquisition/release messages
- Monitor `QueueLocks` table for active locks
- Review `JobLogs` table for execution history

## 🔍 Key Features

### ✅ Distributed Lock Management
- Database-based atomic lock acquisition
- Automatic timeout and cleanup (5-minute default)
- Graceful handling of concurrent access attempts

### ✅ Multi-Framework Support
- **Hangfire v1.8.14**: Enterprise-grade background job processing with SQL Server storage
- **Coravel v5.0.3**: Lightweight .NET job scheduling with in-memory queuing

### ✅ Comprehensive Logging
- Job execution tracking with status
- Instance identification for debugging
- Detailed error and conflict reporting

### ✅ Fault Tolerance
- Handles database connection failures
- Manages application shutdown scenarios
- Prevents orphaned locks with timeout mechanism

### ✅ Performance Optimized
- Dapper for high-performance database operations
- Minimal lock duration
- Efficient unique constraint utilization

## 🛠️ Configuration

### Connection Strings
```json
{
  "ConnectionStrings": {
    "DbConnection": "Server=(localdb)\\mssqllocaldb;Database=BackgroundJobDistributedLock;Trusted_Connection=true;"
  }
}
```

### Queue Configuration
```csharp
public static class QueueName
{
    public const string HangFireQueue = "hangfire-queue";
    public const string CoravelQueue = "coravel-queue";
}
```

### Lock Timeout
```csharp
private const int QueueLockMaximumLifeTimeInMinutes = 5;
```

## 📊 Database Schema

### QueueLocks Table
| Column | Type | Description |
|--------|------|-------------|
| QueueName | nvarchar(100) | Queue identifier (PK) |
| JobName | nvarchar(100) | Job identifier (PK) |
| CreatedTime | datetime2 | Lock creation timestamp |

### JobLogs Table
| Column | Type | Description |
|--------|------|-------------|
| Id | bigint | Primary key |
| AppId | uniqueidentifier | Application instance ID |
| JobName | nvarchar(100) | Job identifier |
| Status | nvarchar(35) | Processing status |
| Remark | nvarchar(max) | Execution details |
| CreatedTime | datetime2 | Log creation time |
| UpdatedTime | datetime2 | Last update time |

## 🧪 Testing Distributed Behavior

1. **Start Both Instances**: Run Instance One and Instance Two simultaneously
2. **Trigger Same Job**: Execute the same job type from both instances
3. **Observe Logs**: Only one instance should process the job
4. **Check Database**: Verify lock creation and cleanup in `QueueLocks` table
5. **Review Job Logs**: Examine execution history in `JobLogs` table

## 🔧 Troubleshooting

### Common Issues
- **Connection String**: Ensure SQL Server is accessible
- **Port Conflicts**: Modify ports in `launchSettings.json` if needed
- **Lock Timeouts**: Adjust `QueueLockMaximumLifeTimeInMinutes` for longer jobs
- **Migration Errors**: Ensure database permissions for schema changes

### Debugging Tips
- Enable detailed logging in `serilogs.json`
- Monitor Hangfire dashboard for job status
- Query database directly to inspect lock states
- Check application logs for constraint violation messages

## 📚 Technical Details

### Lock Implementation Strategy
- **Optimistic Locking**: Attempt lock creation first
- **Unique Constraints**: Database enforces atomicity
- **Timeout Mechanism**: Prevents indefinite locks
- **Cleanup Strategy**: Automatic and manual lock removal

### Performance Considerations
- **Dapper Usage**: High-performance data access for critical operations
- **Connection Management**: Proper disposal and error handling
- **Minimal Lock Duration**: Quick lock acquisition and release
- **Index Optimization**: Composite primary key for efficient lookups

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
