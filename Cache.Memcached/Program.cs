using Enyim.Caching;
using Enyim.Caching.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

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



app.MapPost("add-cache", async (
    [FromServices] IMemcachedClient _memclient,
    [FromBody] CacheModel cacheModel) =>
{
     var addResult =  await _memclient.AddAsync(
          key: cacheModel.Key,
          value: cacheModel.Value,
          timeSpan: TimeSpan.FromSeconds(cacheModel.ExpirationSecond));

    if (addResult)
        return Results.Ok();

    return Results.BadRequest("The data to be cached already exists !");

});

app.MapPost("set-cache", async (
    [FromServices] IMemcachedClient _client,
    [FromBody] CacheModel cacheModel) =>
{
    await _client.SetAsync(cacheModel.Key, cacheModel.Value, TimeSpan.FromSeconds(cacheModel.ExpirationSecond));

});

app.MapPut("replace-cache", async (
    [FromServices] IMemcachedClient _client,
    string key,
    string value) =>
{
    await _client.SetAsync(key, value, TimeSpan.FromMinutes(1));

});

app.MapGet("get-cache-by-name", async (
    [FromServices] IMemcachedClient _client,
    string key) =>
{
    return Results.Ok(await _client.GetAsync<string>(key));

});


app.Run();


static byte[] GetBytes(string value) => Encoding.UTF8.GetBytes(value);

internal record CacheModel(string Key, string Value, double ExpirationSecond);
internal record CacheIncrementOrDecrementModel(string Key, ulong DefaultValue, ulong IncrementOrDecrementValue);
internal record CacheAppendOrPrependModel(string key, string value);