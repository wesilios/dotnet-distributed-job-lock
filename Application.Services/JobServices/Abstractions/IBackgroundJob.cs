namespace Application.Services.JobServices.Abstractions;

public interface IBackgroundJob
{
    Task InvokeAsync(CancellationToken cancellationToken);
}