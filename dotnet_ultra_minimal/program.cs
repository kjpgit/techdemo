// Main entry point

using Microsoft.AspNetCore.Http.Json;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    // Fix bad JSON defaults
    // Don't mess with my attribute names, please
    // (The default changes them to camelCase)
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// This is so nice.  Wrap all our requests in a try/catch.
// This handles authentication, authorization, or any other
// failures we want to map to a nice HTTP response.
// Of course I didn't stumble upon this wonderful feature in the official docs...
app.Use(async (ctx, next) =>
{
    try {
        await next();
    }
    catch(MyWebException ex) {
        ctx.Response.StatusCode = ex.m_code;
        await ctx.Response.WriteAsync(ex.Message);
    }
});

var controller = new MyController();

app.MapPost("/api/AddJob", controller.AddJob);
app.MapPost("/api/TakeJob", controller.TakeJob);

app.Run();


//
// Business Code Below
// In real life these classes would be in different files
//

class MyController
{
    ConcurrentDictionary<string, string> m_jobs_pending = new();
    ILogger _logger;

    public MyController()
    {
        // In real life you'd init the AWS dynamodb client here :-)

        // Unlike the built in console logger that delays output:
        // https://stackoverflow.com/questions/50221983/cant-get-console-logging-to-work-in-net-core-2-0
        _logger = new SimpleLogger("MyController", min_level: LogLevel.Trace);
        _logger.LogInformation("example log information message");
    }

    //
    // Demo API that puts and takes a job
    //
    public async Task<IResult> AddJob(Job job, MyUserContext ctx) {
        bool ret = m_jobs_pending.TryAdd(job.Name, job.Command);
        await Task.Delay(1);  // pretend we did real work
        // This request is idempotent, always return success, but add a message for debugging
        // If it wasn't idmpotent, you could return an error like Conflict.
        return Results.Ok(ret ? "created" : "not overwriting");
    }

    public async Task<TakeJobResponse> TakeJob(TakeJobRequest request, MyUserContext ctx) {
        string? value;
        bool ret = m_jobs_pending.TryRemove(request.Name, out value);
        await Task.Delay(1);  // pretend we did real work
        if (ret) {
            return new TakeJobResponse(Command: value!, ExecutionId: "123");
        } else {
            // Throwing an exception may be more overhead than returning an IResult,
            // so do your own diligence and perf testing.
            // Also, you should probably make the TakeJobResponse able to
            // indicate all common failures.
            throw new MyWebException(404, "Job not found");
        }
    }
}

// Handle required auth + required headers at the same time.
public record MyUserContext(string env)
{
    public static ValueTask<MyUserContext?> BindAsync(HttpContext httpContext)
    {
        // Obviously you would pull this from an env var or service in real life
        const string required_token = "bearer 12345";
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (authHeader != required_token) {
            throw new MyWebException(401, "Invalid Authorization");
        }
        var envHeader = httpContext.Request.Headers["X-My-Environment"].ToString();
        if (envHeader == "") {
            throw new MyWebException(400, "Invalid X-My-Environment");
        }
        var result = new MyUserContext(env: envHeader);
        return ValueTask.FromResult<MyUserContext?>(result);  // This is a lot of ugly ceremony if you ask me
    }
}