using Enyim.Caching;
using Enyim.Caching.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEnyimMemcached(options =>
{
    options.Servers = new List<Server>()
    {
        new Server(){Address = builder.Configuration.GetValue<string>("Memcached:Address"),Port = builder.Configuration.GetValue<int>("Memcached:Port")}
    };

});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseEnyimMemcached();

app.UseHttpsRedirection();


app.MapPost("addcache", async (
    [FromServices] IMemcachedClient _client,
    string key,
    string value) =>
{
    await _client.SetAsync(key, value, TimeSpan.FromMinutes(1));

});

app.MapGet("getcache", async (
    [FromServices] IMemcachedClient _client,
    string key) =>
{
    return Results.Ok(await _client.GetAsync<string>(key));

});


app.Run();

