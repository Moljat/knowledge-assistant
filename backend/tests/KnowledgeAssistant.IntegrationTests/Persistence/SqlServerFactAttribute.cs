namespace KnowledgeAssistant.IntegrationTests.Persistence;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string ConnectionVariable = "KNOWLEDGE_ASSISTANT_TEST_CONNECTION";

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
        {
            Skip = $"Set {ConnectionVariable} to run physical SQL Server tests.";
        }
    }
}
