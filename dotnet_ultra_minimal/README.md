# Ultra Minimal ASP.NET Core Webservice (on Linux)

Are you tired of builders and factories and dependency injection frameworks,
which themselves are actually the useless dependencies that break on you with
each annual "upgrade"?

Are you tired of byzantine, poorly documented "helper" frameworks that are
written by interns, which they don't even consume, to solve problems they have
imagined, in the most over-engineered fashion to get a promotion?

Well, you don't HAVE to use them!  This project just uses the new "minimal"
ASP.NET webapis (a very welcome step in the right direction), but nothing else.

If you want to make a microservice that just runs for years, without care and
feeding, to solve business needs, look at the source files in this repo for
ideas, starting with [program.cs](program.cs).

To run this example, install dotnet 6 SDK on linux and then run:

```
make && make run
```

Then in another shell, test it:

```
make test

# Produces this output:

## Adding invalid job
too long: Name
Response: 400
## Adding good job
"created"
Response: 200
## Adding good job - idempotent retry
"not overwriting"
Response: 200
## Taking job
{"Command":"test123","ExecutionId":"123"}
Response: 200
## Taking job a 2nd time (does not exist)
Job not found
Response: 404
```


This was my initialization for this project:

```
dotnet new webapi -minimal --no-https --no-openapi --exclude-launch-settings
rm -vf appsettings*.json
mv Program.cs program.cs
dos2unix server.csproj program.cs   # Seriously WTF!!!
```

