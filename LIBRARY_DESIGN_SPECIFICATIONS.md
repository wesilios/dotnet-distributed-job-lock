# 📚 Distributed Lock Library Design Specifications

## 🎯 **Target Environment & Compatibility**

### **Framework Support**
- **.NET 8.0**: Primary target framework
- **.NET 7.0**: Supported with compatibility package
- **.NET 6.0**: Long-term support version
- **.NET Framework 4.8**: Legacy support (limited features)

### **Platform Compatibility**
- **Windows**: Windows 10/11, Windows Server 2019/2022
- **Linux**: Ubuntu 20.04+, RHEL 8+, Alpine 3.17+, Debian 11+
- **macOS**: macOS 12.0+ (Monterey and later)
- **Containers**: Docker (linux/amd64, linux/arm64), Kubernetes

### **Database Support Matrix**
| Database | Version | Provider Package | Status |
|----------|---------|------------------|---------|
| SQL Server | 2019+ | DistributedLock.SqlServer | ✅ Production Ready |
| PostgreSQL | 13+ | DistributedLock.PostgreSQL | ✅ Production Ready |
| MySQL | 8.0+ | DistributedLock.MySQL | 🚧 Planned |
| Redis | 6.0+ | DistributedLock.Redis | ✅ Production Ready |
| MongoDB | 5.0+ | DistributedLock.MongoDB | 🚧 Planned |

### **Framework Integration Versions**
| Framework | Supported Versions | Package | Status |
|-----------|-------------------|---------|---------|
| Hangfire | 1.8.0+ (Tested: 1.8.14) | DistributedLock.Hangfire | ✅ Ready |
| Coravel | 5.0.0+ (Tested: 5.0.3) | DistributedLock.Coravel | ✅ Ready |
| Quartz.NET | 3.6.0+ (Tested: 3.8.0) | DistributedLock.Quartz | 🚧 Planned |
| ASP.NET Core | 6.0+ | DistributedLock.AspNetCore | 🚧 Planned |

## 🏗️ **Library Architecture Overview**

### **Package Structure**
```
DistributedLock/
├── DistributedLock.Core/                    # Core abstractions and implementations
├── DistributedLock.SqlServer/               # SQL Server storage provider
├── DistributedLock.PostgreSQL/              # PostgreSQL storage provider  
├── DistributedLock.Redis/                   # Redis storage provider
├── DistributedLock.Hangfire/                # Hangfire integration
├── DistributedLock.Coravel/                 # Coravel integration
├── DistributedLock.Quartz/                  # Quartz.NET integration
├── DistributedLock.AspNetCore/              # ASP.NET Core integration
└── DistributedLock.Testing/                 # Testing utilities
```

---

## 🎯 **Core Library (DistributedLock.Core)**

### **1. Core Abstractions**

```csharp
// Primary lock manager interface
public interface IDistributedLockManager
{
    Task<ILockResult> AcquireLockAsync(string lockKey, LockOptions? options = null, CancellationToken cancellationToken = default);
    Task<ILockResult> AcquireLockAsync(string lockKey, TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<bool> ReleaseLockAsync(string lockKey, string lockToken, CancellationToken cancellationToken = default);
    Task<bool> IsLockedAsync(string lockKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<LockInfo>> GetActiveLocksAsync(CancellationToken cancellationToken = default);
    Task CleanupExpiredLocksAsync(CancellationToken cancellationToken = default);
}

// Storage provider abstraction
public interface ILockStorageProvider
{
    Task<LockAcquisitionResult> TryAcquireLockAsync(LockRequest request, CancellationToken cancellationToken = default);
    Task<bool> ReleaseLockAsync(string lockKey, string lockToken, CancellationToken cancellationToken = default);
    Task<LockInfo?> GetLockInfoAsync(string lockKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<LockInfo>> GetActiveLocksAsync(string? prefix = null, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredLocksAsync(DateTime expiredBefore, CancellationToken cancellationToken = default);
    Task<bool> ExtendLockAsync(string lockKey, string lockToken, TimeSpan extension, CancellationToken cancellationToken = default);
}

// Job execution logging
public interface IJobExecutionLogger
{
    Task<long> LogJobStartAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
    Task LogJobCompletionAsync(long logId, JobExecutionResult result, CancellationToken cancellationToken = default);
    Task LogJobFailureAsync(long logId, Exception exception, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobExecutionLog>> GetJobHistoryAsync(string jobName, int take = 100, CancellationToken cancellationToken = default);
}
```

### **2. Core Models**

```csharp
public class LockOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxRetryAttempts { get; set; } = 3;
    public string? Scope { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool AutoExtend { get; set; } = false;
    public TimeSpan? AutoExtendInterval { get; set; }
}

public interface ILockResult : IDisposable, IAsyncDisposable
{
    bool IsAcquired { get; }
    string LockKey { get; }
    string? LockToken { get; }
    DateTime AcquiredAt { get; }
    DateTime ExpiresAt { get; }
    LockFailureReason? FailureReason { get; }
    Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default);
    Task<bool> ReleaseAsync(CancellationToken cancellationToken = default);
}

public class JobExecutionContext
{
    public string JobName { get; set; } = string.Empty;
    public string InstanceId { get; set; } = Environment.MachineName;
    public string? QueueName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}

public enum LockFailureReason
{
    AlreadyLocked,
    Timeout,
    StorageError,
    InvalidConfiguration,
    Cancelled
}

public enum JobExecutionStatus
{
    Started,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    TimedOut
}
```

### **3. Configuration System**

```csharp
public class DistributedLockConfiguration
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);
    public string DefaultScope { get; set; } = "default";
    public bool EnableJobLogging { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableHealthChecks { get; set; } = true;
    public RetryPolicy RetryPolicy { get; set; } = new();
    public Dictionary<string, object> ProviderSettings { get; set; } = new();
}

public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public double BackoffMultiplier { get; set; } = 2.0;
    public bool EnableJitter { get; set; } = true;
}
```

---

## 🔌 **Storage Providers**

### **1. SQL Server Provider (DistributedLock.SqlServer)**

```csharp
public class SqlServerLockStorageProvider : ILockStorageProvider
{
    private readonly string _connectionString;
    private readonly SqlServerProviderOptions _options;

    // Optimized SQL operations using stored procedures
    // Handles unique constraint violations gracefully
    // Supports lock extension and cleanup
}

public class SqlServerProviderOptions
{
    public string TableName { get; set; } = "DistributedLocks";
    public string SchemaName { get; set; } = "dbo";
    public bool CreateTableIfNotExists { get; set; } = true;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxPoolSize { get; set; } = 100;
}
```

### **2. Redis Provider (DistributedLock.Redis)**

```csharp
public class RedisLockStorageProvider : ILockStorageProvider
{
    private readonly IDatabase _database;
    private readonly RedisProviderOptions _options;

    // Uses Redis SET with NX and EX options
    // Implements Redlock algorithm for Redis clusters
    // Supports pub/sub for lock notifications
}

public class RedisProviderOptions
{
    public string KeyPrefix { get; set; } = "distributed-lock:";
    public bool UseRedlock { get; set; } = false;
    public TimeSpan LockExtensionInterval { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableNotifications { get; set; } = false;
}
```

---

## 🎛️ **Framework Integrations**

### **1. Hangfire Integration (DistributedLock.Hangfire)**

**Supported Versions**: Hangfire 1.8.0+ (Tested with 1.8.14)
**Dependencies**: Hangfire.Core, Hangfire.SqlServer, Hangfire.AspNetCore

```csharp
// Attribute-based approach
[DistributedLock("my-unique-job", TimeoutMinutes = 10)]
public class MyBackgroundJob
{
    public async Task Execute()
    {
        // Job logic - automatically protected by distributed lock
    }
}

// Filter-based approach
public class DistributedLockJobFilter : IServerFilter
{
    public void OnPerforming(PerformingContext context)
    {
        // Acquire lock before job execution
    }

    public void OnPerformed(PerformedContext context)
    {
        // Release lock after job completion
    }
}

// Extension methods
public static class HangfireExtensions
{
    public static IGlobalConfiguration UseDistributedLock(
        this IGlobalConfiguration configuration,
        Action<DistributedLockConfiguration> configureOptions)
    {
        // Setup distributed lock filter globally
    }
}
```

### **2. Coravel Integration (DistributedLock.Coravel)**

**Supported Versions**: Coravel 5.0.0+ (Tested with 5.0.3)
**Dependencies**: Coravel, Coravel.Queuing

```csharp
// Decorator pattern
public class DistributedLockJobDecorator<T> : IInvocable where T : IInvocable
{
    private readonly T _innerJob;
    private readonly IDistributedLockManager _lockManager;
    private readonly string _lockKey;

    public async Task Invoke()
    {
        using var lockResult = await _lockManager.AcquireLockAsync(_lockKey);
        if (lockResult.IsAcquired)
        {
            await _innerJob.Invoke();
        }
    }
}

// Extension methods
public static class CoravelExtensions
{
    public static IServiceCollection AddDistributedLockForCoravel(
        this IServiceCollection services,
        Action<DistributedLockConfiguration> configure)
    {
        // Register decorator and lock manager
    }

    public static IScheduleInterval WithDistributedLock(
        this IScheduleInterval schedule,
        string lockKey,
        TimeSpan? timeout = null)
    {
        // Wrap scheduled job with distributed lock
    }
}
```

---

## 🎯 **Key Features Specification**

### **1. Multi-Storage Backend Support**
- **SQL Server**: Production-ready with connection pooling
- **PostgreSQL**: Full feature parity with SQL Server
- **MySQL**: Community-requested support
- **Redis**: High-performance, supports clustering
- **MongoDB**: Document-based storage option
- **In-Memory**: For testing and development

### **2. Advanced Locking Features**
```csharp
// Hierarchical locks
await lockManager.AcquireLockAsync("parent-job");
await lockManager.AcquireLockAsync("parent-job/child-task-1");

// Scoped locks (tenant isolation)
var options = new LockOptions 
{ 
    Scope = "tenant-123",
    Timeout = TimeSpan.FromMinutes(10)
};
await lockManager.AcquireLockAsync("data-processing", options);

// Lock with automatic extension
var options = new LockOptions 
{ 
    AutoExtend = true,
    AutoExtendInterval = TimeSpan.FromMinutes(2)
};
```

### **3. Monitoring & Observability**
```csharp
// Built-in metrics
public interface IDistributedLockMetrics
{
    void RecordLockAcquisition(string lockKey, TimeSpan duration, bool successful);
    void RecordLockRelease(string lockKey, TimeSpan heldDuration);
    void RecordLockTimeout(string lockKey, TimeSpan waitDuration);
    void RecordLockContention(string lockKey, int waitingCount);
}

// Health checks
public class DistributedLockHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Verify storage provider connectivity
        // Check for excessive lock contention
        // Validate cleanup process health
    }
}
```

### **4. Testing Support**
```csharp
// In-memory provider for unit tests
services.AddDistributedLock(options =>
{
    options.UseInMemoryStorage();
    options.EnableTestMode(); // Deterministic behavior
});

// Test utilities
public class DistributedLockTestHarness
{
    public async Task<bool> WaitForLockAsync(string lockKey, TimeSpan timeout);
    public Task SimulateLockContentionAsync(string lockKey, int concurrentAttempts);
    public Task<LockInfo[]> GetAllActiveLocksAsync();
    public Task ClearAllLocksAsync();
}
```

---

## 🚀 **Usage Examples**

### **Simple Setup**
```csharp
// Program.cs
services.AddDistributedLock(options =>
{
    options.UseSqlServer(connectionString);
    options.DefaultTimeout = TimeSpan.FromMinutes(5);
    options.EnableJobLogging = true;
    options.EnableMetrics = true;
});
```

### **Advanced Configuration**
```csharp
services.AddDistributedLock(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.TableName = "CustomLockTable";
        sqlOptions.SchemaName = "locks";
        sqlOptions.CommandTimeout = TimeSpan.FromSeconds(60);
    });

    options.RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 5,
        InitialDelay = TimeSpan.FromSeconds(2),
        BackoffMultiplier = 1.5
    };

    options.EnableHealthChecks = true;
    options.CleanupInterval = TimeSpan.FromMinutes(5);
});
```

### **Manual Lock Management**
```csharp
public class DataProcessingService
{
    private readonly IDistributedLockManager _lockManager;

    public async Task ProcessCriticalDataAsync()
    {
        var options = new LockOptions
        {
            Timeout = TimeSpan.FromMinutes(30),
            Scope = "data-processing",
            AutoExtend = true
        };

        using var lockResult = await _lockManager.AcquireLockAsync("critical-data-job", options);

        if (!lockResult.IsAcquired)
        {
            _logger.LogWarning("Could not acquire lock: {Reason}", lockResult.FailureReason);
            return;
        }

        try
        {
            // Process critical data
            await ProcessDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data");
            throw;
        }
        // Lock automatically released via using statement
    }
}
```

### **Quartz.NET Integration (DistributedLock.Quartz)**

**Supported Versions**: Quartz.NET 3.6.0+ (Tested with 3.8.0)
**Dependencies**: Quartz, Quartz.Extensions.Hosting

```csharp
// Job wrapper approach
[DistributedLock("data-sync-job", TimeoutMinutes = 15)]
public class DataSyncJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Job logic - automatically protected by distributed lock
        await SyncDataAsync();
    }
}

// Manual integration
public class ManualQuartzJob : IJob
{
    private readonly IDistributedLockManager _lockManager;

    public async Task Execute(IJobExecutionContext context)
    {
        using var lockResult = await _lockManager.AcquireLockAsync("manual-job");
        if (lockResult.IsAcquired)
        {
            await DoWorkAsync();
        }
    }
}
```

### **ASP.NET Core Integration (DistributedLock.AspNetCore)**

```csharp
// Controller action protection
[HttpPost("process-data")]
[DistributedLock("api-data-processing", TimeoutSeconds = 30)]
public async Task<IActionResult> ProcessData([FromBody] DataRequest request)
{
    // Only one instance can process this endpoint at a time
    await ProcessDataAsync(request);
    return Ok();
}

// Background service integration
public class DataProcessingBackgroundService : BackgroundService
{
    private readonly IDistributedLockManager _lockManager;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var lockResult = await _lockManager.AcquireLockAsync("background-processing");
            if (lockResult.IsAcquired)
            {
                await ProcessBatchAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

---

## 📊 **Performance & Scalability Considerations**

### **1. Database Optimization**
- **Composite Primary Keys**: Efficient lookups using (LockKey, Scope) composite keys
- **Proper Indexing**: Covering indexes for common query patterns
- **Connection Pooling**: Optimized connection management with retry policies
- **Bulk Operations**: Batch cleanup operations for expired locks
- **Stored Procedures**: Pre-compiled SQL for critical operations

### **2. Redis Optimization**
- **Pipeline Operations**: Batch multiple Redis commands for better throughput
- **Lua Scripts**: Atomic operations using server-side scripting
- **Cluster Support**: Redis Cluster and Sentinel support for high availability
- **Memory Efficiency**: Optimized data structures and expiration policies
- **Pub/Sub Integration**: Real-time lock notifications and events

### **3. Monitoring & Alerting**
- **Lock Contention Metrics**: Track high-contention locks and bottlenecks
- **Performance Counters**: Average lock acquisition time, hold duration
- **Failure Rate Monitoring**: Track failed acquisitions and timeouts
- **Storage Health**: Monitor storage provider connectivity and performance
- **Custom Dashboards**: Integration with Grafana, Application Insights

### **4. Scalability Features**
- **Horizontal Scaling**: Support for multiple application instances
- **Load Balancing**: Distribute lock requests across storage nodes
- **Partitioning**: Shard locks across multiple storage instances
- **Caching**: In-memory caching for frequently accessed lock metadata
- **Circuit Breaker**: Automatic failover and recovery mechanisms

---

## 🔧 **Migration & Compatibility**

### **1. Migration Tools**
```csharp
public interface ILockMigrationTool
{
    Task MigrateFromLegacySystemAsync(string legacyConnectionString);
    Task ValidateMigrationAsync();
    Task GenerateMigrationReportAsync(string outputPath);
    Task<MigrationStatus> GetMigrationStatusAsync();
}

public class MigrationOptions
{
    public bool PreserveLockHistory { get; set; } = true;
    public bool ValidateDataIntegrity { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
}
```

### **2. Backward Compatibility**
- **Schema Evolution**: Support for existing table schemas with migration paths
- **Version Compatibility**: Maintain compatibility across major versions
- **Gradual Migration**: Zero-downtime migration strategies
- **Legacy Support**: Adapters for existing distributed lock implementations

### **3. Breaking Change Management**
- **Semantic Versioning**: Clear versioning strategy with breaking change indicators
- **Deprecation Warnings**: Advanced notice for deprecated features
- **Migration Guides**: Step-by-step upgrade documentation
- **Compatibility Matrix**: Clear support matrix for different versions

---

## 🎯 **Advanced Features**

### **1. Lock Hierarchies & Dependencies**
```csharp
// Parent-child lock relationships
public interface IHierarchicalLockManager : IDistributedLockManager
{
    Task<ILockResult> AcquireParentLockAsync(string parentKey, string[] childKeys, LockOptions? options = null);
    Task<ILockResult> AcquireChildLockAsync(string parentKey, string childKey, LockOptions? options = null);
    Task<bool> HasChildLocksAsync(string parentKey);
    Task<string[]> GetChildLockKeysAsync(string parentKey);
}

// Lock dependencies
public class LockDependency
{
    public string LockKey { get; set; }
    public string[] RequiredLocks { get; set; }
    public LockDependencyMode Mode { get; set; } = LockDependencyMode.All;
}

public enum LockDependencyMode
{
    All,    // All required locks must be acquired
    Any,    // At least one required lock must be acquired
    None    // No required locks (independent)
}
```

### **2. Distributed Events & Notifications**
```csharp
public interface IDistributedLockEvents
{
    event EventHandler<LockAcquiredEventArgs> LockAcquired;
    event EventHandler<LockReleasedEventArgs> LockReleased;
    event EventHandler<LockTimeoutEventArgs> LockTimeout;
    event EventHandler<LockContentionEventArgs> LockContention;
}

public class LockAcquiredEventArgs : EventArgs
{
    public string LockKey { get; set; }
    public string LockToken { get; set; }
    public string InstanceId { get; set; }
    public DateTime AcquiredAt { get; set; }
    public TimeSpan Timeout { get; set; }
}

// Real-time notifications via SignalR
public interface ILockNotificationHub
{
    Task NotifyLockAcquired(string lockKey, string instanceId);
    Task NotifyLockReleased(string lockKey, string instanceId);
    Task NotifyLockContention(string lockKey, int waitingCount);
}
```

### **3. Circuit Breaker & Resilience**
```csharp
public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan OpenTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan HalfOpenTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableFallback { get; set; } = true;
}

public interface IResiliencePolicy
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
    Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
```

### **4. Multi-Tenant Support**
```csharp
public interface ITenantAwareLockManager : IDistributedLockManager
{
    Task<ILockResult> AcquireLockAsync(string tenantId, string lockKey, LockOptions? options = null);
    Task<IEnumerable<LockInfo>> GetTenantLocksAsync(string tenantId);
    Task<bool> ReleaseTenantLocksAsync(string tenantId);
}

public class TenantLockOptions : LockOptions
{
    public string TenantId { get; set; } = string.Empty;
    public TenantIsolationMode IsolationMode { get; set; } = TenantIsolationMode.Strict;
}

public enum TenantIsolationMode
{
    Strict,     // Complete isolation between tenants
    Shared,     // Allow cross-tenant lock visibility
    Hierarchical // Tenant hierarchy support
}
```

---

## 📈 **Metrics & Observability**

### **1. Built-in Metrics**
```csharp
public interface IDistributedLockMetrics
{
    // Performance metrics
    void RecordLockAcquisitionTime(string lockKey, TimeSpan duration);
    void RecordLockHoldTime(string lockKey, TimeSpan duration);
    void RecordLockWaitTime(string lockKey, TimeSpan duration);

    // Success/failure metrics
    void IncrementLockAcquisitionSuccess(string lockKey);
    void IncrementLockAcquisitionFailure(string lockKey, LockFailureReason reason);
    void IncrementLockTimeout(string lockKey);

    // Contention metrics
    void RecordLockContention(string lockKey, int waitingCount);
    void RecordLockRetryAttempt(string lockKey, int attemptNumber);

    // Storage metrics
    void RecordStorageOperation(string operation, TimeSpan duration, bool success);
}
```

### **2. Health Checks**
```csharp
public class DistributedLockHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var checks = new List<HealthCheckItem>
        {
            await CheckStorageConnectivity(),
            await CheckLockAcquisition(),
            await CheckCleanupProcess(),
            await CheckMetricsCollection()
        };

        var unhealthyChecks = checks.Where(c => !c.IsHealthy).ToList();

        if (unhealthyChecks.Any())
        {
            return HealthCheckResult.Unhealthy(
                $"Failed checks: {string.Join(", ", unhealthyChecks.Select(c => c.Name))}",
                data: checks.ToDictionary(c => c.Name, c => (object)c.Status));
        }

        return HealthCheckResult.Healthy("All distributed lock checks passed",
            data: checks.ToDictionary(c => c.Name, c => (object)c.Status));
    }
}
```

### **3. Logging Integration**
```csharp
public class DistributedLockLogger
{
    private readonly ILogger<DistributedLockLogger> _logger;

    public void LogLockAcquisitionAttempt(string lockKey, LockOptions options)
    {
        _logger.LogDebug("Attempting to acquire lock {LockKey} with timeout {Timeout}",
            lockKey, options.Timeout);
    }

    public void LogLockAcquired(string lockKey, string lockToken, TimeSpan acquisitionTime)
    {
        _logger.LogInformation("Lock {LockKey} acquired with token {LockToken} in {AcquisitionTime}ms",
            lockKey, lockToken, acquisitionTime.TotalMilliseconds);
    }

    public void LogLockContention(string lockKey, int waitingCount, TimeSpan waitTime)
    {
        _logger.LogWarning("Lock contention detected for {LockKey}: {WaitingCount} waiting, waited {WaitTime}ms",
            lockKey, waitingCount, waitTime.TotalMilliseconds);
    }
}
```

---

## 🧪 **Testing & Quality Assurance**

### **1. Unit Testing Support**
```csharp
// Test doubles and mocks
public class InMemoryLockStorageProvider : ILockStorageProvider
{
    private readonly ConcurrentDictionary<string, LockInfo> _locks = new();

    public Task<LockAcquisitionResult> TryAcquireLockAsync(LockRequest request, CancellationToken cancellationToken = default)
    {
        // In-memory implementation for testing
    }
}

// Test utilities
public static class DistributedLockTestExtensions
{
    public static IServiceCollection AddDistributedLockTesting(this IServiceCollection services)
    {
        services.AddSingleton<ILockStorageProvider, InMemoryLockStorageProvider>();
        services.AddSingleton<IDistributedLockTestHarness, DistributedLockTestHarness>();
        return services;
    }
}
```

### **2. Integration Testing**
```csharp
public class DistributedLockIntegrationTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Should_Prevent_Concurrent_Execution()
    {
        // Arrange
        var lockManager = CreateLockManager();
        var tasks = new List<Task<bool>>();

        // Act - Start multiple concurrent lock attempts
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(TryAcquireLockAsync(lockManager, "test-lock"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - Only one should succeed
        Assert.Single(results.Where(r => r));
    }
}
```

### **3. Performance Testing**
```csharp
public class DistributedLockPerformanceTests
{
    [Fact]
    public async Task Should_Handle_High_Throughput()
    {
        var lockManager = CreateLockManager();
        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;

        var tasks = Enumerable.Range(0, 1000)
            .Select(async i =>
            {
                using var lockResult = await lockManager.AcquireLockAsync($"lock-{i % 100}");
                if (lockResult.IsAcquired)
                {
                    Interlocked.Increment(ref successCount);
                    await Task.Delay(10); // Simulate work
                }
            });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        Assert.True(successCount > 0);
        Assert.True(stopwatch.ElapsedMilliseconds < 30000); // Should complete within 30 seconds
    }
}
```

---

## 📚 **Documentation & Developer Experience**

### **1. Comprehensive Documentation**
- **Getting Started Guide**: Quick setup and basic usage examples
- **API Reference**: Complete API documentation with examples
- **Architecture Guide**: Deep dive into library architecture and design decisions
- **Performance Guide**: Best practices for optimal performance
- **Troubleshooting Guide**: Common issues and solutions
- **Migration Guide**: Upgrading from other distributed lock solutions

### **2. Code Examples & Samples**
- **Sample Applications**: Complete working examples for different scenarios
- **Framework Integration Examples**: Specific examples for Hangfire, Coravel, Quartz
- **Performance Benchmarks**: Comparative performance analysis
- **Best Practices**: Recommended patterns and anti-patterns

### **3. Developer Tools**
- **Visual Studio Extension**: IntelliSense support and code snippets
- **CLI Tools**: Command-line utilities for lock management and diagnostics
- **Debugging Tools**: Lock state inspection and troubleshooting utilities
- **Performance Profiler**: Built-in profiling for lock performance analysis

---

## 🔒 **Security Considerations**

### **1. Authentication & Authorization**
```csharp
public interface ILockAuthorizationProvider
{
    Task<bool> CanAcquireLockAsync(string lockKey, ClaimsPrincipal user);
    Task<bool> CanReleaseLockAsync(string lockKey, string lockToken, ClaimsPrincipal user);
    Task<bool> CanViewLockAsync(string lockKey, ClaimsPrincipal user);
}

public class LockSecurityOptions
{
    public bool RequireAuthentication { get; set; } = false;
    public bool EnableAuditLogging { get; set; } = true;
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();
    public TimeSpan TokenExpiration { get; set; } = TimeSpan.FromHours(1);
}
```

### **2. Audit Logging**
```csharp
public interface ILockAuditLogger
{
    Task LogLockOperationAsync(LockAuditEvent auditEvent);
    Task<IEnumerable<LockAuditEvent>> GetAuditTrailAsync(string lockKey, DateTime? from = null, DateTime? to = null);
}

public class LockAuditEvent
{
    public string LockKey { get; set; }
    public string Operation { get; set; } // Acquire, Release, Extend, etc.
    public string UserId { get; set; }
    public string InstanceId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

---

## 🚀 **Deployment & Operations**

### **1. Container Support**
```dockerfile
# Example Dockerfile for applications using the library
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app

# Health check endpoint for distributed locks
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health/distributed-locks || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### **2. Kubernetes Integration**
```yaml
# Example Kubernetes deployment with distributed lock health checks
apiVersion: apps/v1
kind: Deployment
metadata:
  name: background-job-app
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: app
        image: myapp:latest
        livenessProbe:
          httpGet:
            path: /health/distributed-locks
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 10
```

### **3. Monitoring & Alerting**
```csharp
// Prometheus metrics integration
public class PrometheusDistributedLockMetrics : IDistributedLockMetrics
{
    private readonly Counter _lockAcquisitionCounter;
    private readonly Histogram _lockAcquisitionDuration;
    private readonly Gauge _activeLocks;

    public PrometheusDistributedLockMetrics()
    {
        _lockAcquisitionCounter = Metrics.CreateCounter(
            "distributed_lock_acquisitions_total",
            "Total number of lock acquisitions",
            new[] { "lock_key", "result" });

        _lockAcquisitionDuration = Metrics.CreateHistogram(
            "distributed_lock_acquisition_duration_seconds",
            "Time taken to acquire locks",
            new[] { "lock_key" });

        _activeLocks = Metrics.CreateGauge(
            "distributed_lock_active_total",
            "Number of currently active locks");
    }
}
```

This comprehensive specification document covers all aspects of the distributed lock library design, from core architecture to advanced features, testing, security, and deployment considerations. The library would provide a robust, scalable, and developer-friendly solution for distributed locking in .NET applications.
