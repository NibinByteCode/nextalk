using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexTalk.Application.Abstractions;
using NexTalk.Infrastructure.Firebase;
using NexTalk.Infrastructure.SqlServer;

namespace NexTalk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        switch (provider.Trim().ToLowerInvariant())
        {
            case "sqlserver":
                var connection = configuration.GetConnectionString("SqlServer")
                    ?? throw new InvalidOperationException("Missing SQL Server connection string.");
                services.AddDbContext<NexTalkDbContext>(options => options.UseSqlServer(connection));
                services.AddScoped<IChatStore, SqlChatStore>();
                break;
            case "firebase":
                var projectId = configuration["Firebase:ProjectId"]
                    ?? throw new InvalidOperationException("Missing Firebase project ID.");
                var credentialsPath = configuration["Firebase:CredentialsPath"];
                var firestore = string.IsNullOrWhiteSpace(credentialsPath)
                    ? FirestoreDb.Create(projectId)
                    : new FirestoreDbBuilder { ProjectId = projectId, CredentialsPath = credentialsPath }.Build();
                services.AddSingleton(firestore);
                services.AddScoped<IChatStore, FirestoreChatStore>();
                break;
            default:
                throw new InvalidOperationException($"Unsupported Database:Provider '{provider}'.");
        }
        return services;
    }
}
