var builder = WebApplication.CreateBuilder(args);

// set up configuration
builder.Configuration.SetBasePath( Environment.CurrentDirectory );
builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders()
    .AddSystemdConsole( options =>
    {
        options.IncludeScopes = false;
        options.TimestampFormat = "HH:mm:ss ";
    } )
    .SetMinimumLevel( LogLevel.Information );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// middleware and web host
var hostPort = builder.Configuration.GetValue<int>( "port", 9000 );
var healthPort = builder.Configuration.GetValue<int>( "healthcheck-port", 9001 );

var urls = builder.Configuration["ASPNETCORE_URLS"];

if ( urls == null )
{
    // only set urls if the env var is not present
    builder.WebHost.UseUrls( $"http://*:{hostPort};http://*:{healthPort}" );
}

builder.Services.AddCors( options =>
{
    options.AddPolicy( "Public", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader() );
} );

// build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet( "/", () =>
{
    return "/ OK\n";
} )
.RequireHost( $"*:{hostPort}" );;

app.MapGet( "/healthz", () =>
{
    return "/healthz OK\n";
} )
.RequireHost( $"*:{healthPort}" );;

app.UseCors( "Public" );

app.Run();
