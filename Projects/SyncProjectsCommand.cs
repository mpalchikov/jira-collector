namespace JiraCollector.Projects;

public record SyncProjectsCommand();

public sealed class SyncProjectsCommandHandler
{
    public Task Handle(SyncProjectsCommand command)
    {
        return Task.CompletedTask;
    }
}
