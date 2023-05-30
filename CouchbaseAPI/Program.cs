using Couchbase.Extensions.DependencyInjection;
using Cache.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var couchbaseConfiguration = builder.Configuration.GetSection("Couchbase");
builder.Services.AddCouchbase(couchbaseConfiguration);

builder.Services.AddCacheService();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStopped.Register(() =>
{
    app.Services.GetService<ICouchbaseLifetimeService>()?.Close();
});

app.Run();
