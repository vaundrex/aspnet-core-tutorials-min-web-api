using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ToDoApi;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
//  This page is enabled only in the Development environment. 
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 
// The developer exception page is enabled by default in the development environment for minimal API apps
// https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-8.0&amp%3Btabs=visual-studio&tabs=visual-studio

// builder.Services.ConfigureHttpJsonOptions(options => {
//     options.SerializerOptions.WriteIndented = true;
//     // options.SerializerOptions.IncludeFields = true;
// });
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePages();
}
app.Logger.LogInformation($"Application Name: {builder.Environment.ApplicationName}");
app.Logger.LogInformation($"Environment Name: {builder.Environment.EnvironmentName}");
app.Logger.LogInformation($"ContentRoot Path: {builder.Environment.ContentRootPath}");
app.Logger.LogInformation($"WebRootPath: {builder.Environment.WebRootPath}");

app.MapGroup("/todoitems")
    .MapTodoApi()
    .WithTags("Public");

app.MapGet("/", () => "Welcome to the Todo API");
app.MapGet("/throw", () => new InvalidOperationException("Sample exception.")); 

app.Run();
