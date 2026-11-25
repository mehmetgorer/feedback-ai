using CustomerFeedback.Api.Analysis;
using CustomerFeedback.Api.Data;
using CustomerFeedback.Api.Middleware;
using CustomerFeedback.Api.Options;
using CustomerFeedback.Api.Services;
using CustomerFeedback.Api.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFeedbackRequestValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFeedbackService, FeedbackService>();

builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection(GroqOptions.SectionName));
builder.Services.AddHttpClient<IFeedbackAnalysisService, GroqFeedbackAnalysisService>((sp, client) =>
{
    var groq = sp.GetRequiredService<IOptions<GroqOptions>>().Value;
    var baseUrl = groq.BaseUrl.TrimEnd('/') + "/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(groq.TimeoutSeconds, 5, 120));
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var migrateOnStartup = app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
if (app.Environment.IsDevelopment() || migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;