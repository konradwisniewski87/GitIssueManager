using IssueManager.Core.Interfaces;
using IssueManager.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Registration GitHubIssueService
builder.Services.AddHttpClient<GitHubIssueService>();
builder.Services.AddSingleton<GitHubIssueService>(provider =>
    new GitHubIssueService(
        provider.GetRequiredService<HttpClient>(),
        builder.Configuration["GitHub:Token"]!
    ));

//Registration GitLabIssueService
builder.Services.AddHttpClient<GitLabIssueService>();
builder.Services.AddSingleton<GitLabIssueService>(provider =>
    new GitLabIssueService(
        provider.GetRequiredService<HttpClient>(),
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
