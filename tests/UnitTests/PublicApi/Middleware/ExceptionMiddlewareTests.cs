using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.PublicApi.Middleware;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.PublicApi.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoException_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DuplicateException_ReturnsConflictStatus()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Duplicate item found";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new DuplicateException(exceptionMessage);
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains(exceptionMessage, responseBody);
        Assert.Contains("409", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Something went wrong";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new Exception(exceptionMessage);
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains(exceptionMessage, responseBody);
        Assert.Contains("500", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_DuplicateException_ReturnsCorrectErrorDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Duplicate entry";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new DuplicateException(exceptionMessage);
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        // The response should be valid JSON with StatusCode and Message properties
        Assert.Contains("\"StatusCode\"", responseBody);
        Assert.Contains("\"Message\"", responseBody);
        Assert.Contains(exceptionMessage, responseBody);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionInNext_SetsContentTypeToJson()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new Exception("Test exception");
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }
}
