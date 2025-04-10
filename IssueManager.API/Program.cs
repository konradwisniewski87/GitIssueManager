using IssueManager.Core.Interfaces;
using IssueManager.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Add services to the container.
//Registration GitHubIssueService
builder.Services.AddSingleton<GitHubIssueService>(provider =>
    new GitHubIssueService(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient(),
        builder.Configuration["GitHub:Token"]!
    ));

//Registration GitLabIssueService
builder.Services.AddSingleton<GitLabIssueService>(provider =>
    new GitLabIssueService(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient(),
        builder.Configuration["GitLab:Token"]!
    ));

builder.Services.AddSingleton<IIssueServiceFactory, IssueServiceFactory>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();