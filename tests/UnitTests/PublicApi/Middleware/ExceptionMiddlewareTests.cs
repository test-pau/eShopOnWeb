using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.PublicApi.Middleware;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.PublicApi.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
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
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenDuplicateException_ReturnsConflictStatus()
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
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);

        Assert.NotNull(errorDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, errorDetails.StatusCode);
        Assert.Equal(exceptionMessage, errorDetails.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ReturnsInternalServerError()
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
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);

        Assert.NotNull(errorDetails);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorDetails.StatusCode);
        Assert.Equal(exceptionMessage, errorDetails.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenDuplicateException_WritesCorrectJsonFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Test duplicate";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new DuplicateException(exceptionMessage);
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        // Verify it's valid JSON
        Assert.NotEmpty(responseBody);
        var exception = Record.Exception(() => JsonSerializer.Deserialize<ErrorDetails>(responseBody));
        Assert.Null(exception);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_WritesCorrectJsonFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Test error";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new InvalidOperationException(exceptionMessage);
        };
        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        // Verify it's valid JSON
        Assert.NotEmpty(responseBody);
        var exception = Record.Exception(() => JsonSerializer.Deserialize<ErrorDetails>(responseBody));
        Assert.Null(exception);
    }
}
