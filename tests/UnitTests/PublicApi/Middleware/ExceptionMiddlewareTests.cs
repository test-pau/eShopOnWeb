using System;
using System.IO;
using System.Net;
using System.Text.Json;
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
    public async Task InvokeAsync_DuplicateException_Returns409Conflict()
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
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);

        Assert.NotNull(errorDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, errorDetails.StatusCode);
        Assert.Equal(exceptionMessage, errorDetails.Message);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500InternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "An unexpected error occurred";

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
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);

        Assert.NotNull(errorDetails);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorDetails.StatusCode);
        Assert.Equal(exceptionMessage, errorDetails.Message);
    }

    [Fact]
    public async Task InvokeAsync_DuplicateException_ReturnsCorrectJsonFormat()
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

        // Verify it's valid JSON
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);
        Assert.NotNull(errorDetails);

        // Verify the JSON can be serialized back
        var reserializedJson = JsonSerializer.Serialize(errorDetails);
        Assert.NotEmpty(reserializedJson);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsCorrectJsonFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exceptionMessage = "Generic error";

        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new InvalidOperationException(exceptionMessage);
        };

        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        // Verify it's valid JSON
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(responseBody);
        Assert.NotNull(errorDetails);

        // Verify the JSON can be serialized back
        var reserializedJson = JsonSerializer.Serialize(errorDetails);
        Assert.NotEmpty(reserializedJson);
    }

    [Fact]
    public async Task InvokeAsync_NoException_DoesNotSetStatusCode()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var originalStatusCode = context.Response.StatusCode;

        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(originalStatusCode, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_NoException_DoesNotSetContentType()
    {
        // Arrange
        var context = new DefaultHttpContext();

        RequestDelegate next = (HttpContext ctx) => Task.CompletedTask;

        var middleware = new ExceptionMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Null(context.Response.ContentType);
    }
}
