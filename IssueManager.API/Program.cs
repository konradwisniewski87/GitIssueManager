using IssueManager.API.Middleware;
using IssueManager.Core.Interfaces;
using IssueManager.Core.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Add services to the container.
//Registration GitHubIssueService

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<RequestTimeLoggingMiddleware>();
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
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeLoggingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();