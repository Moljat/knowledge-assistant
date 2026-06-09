using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeAssistant.Infrastructure.Persistence;

public sealed class KnowledgeAssistantDbContextFactory
    : IDesignTimeDbContextFactory<KnowledgeAssistantDbContext>
{
    private const string DefaultLocalConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=KnowledgeAssistant;"
        + "Trusted_Connection=True;TrustServerCertificate=True";

    public KnowledgeAssistantDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString(args);
        var options = new DbContextOptionsBuilder<KnowledgeAssistantDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsAssembly(
                    typeof(KnowledgeAssistantDbContext).Assembly.FullName))
            .Options;

        return new KnowledgeAssistantDbContext(options);
    }

    private static string GetConnectionString(string[] args)
    {
        var argument = args.FirstOrDefault(value =>
            value.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase));

        if (argument is not null)
        {
            return argument["--connection=".Length..];
        }

        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? DefaultLocalConnection;
    }
}
