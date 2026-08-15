using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PosApplication.Models;
using PosCore.Services;
using PosDomain.Entities;
using Xunit;

namespace PosCore.Tests.Services;

public class ApiServiceRouteTests
{
    [Fact]
    public async Task SyncOrderAsync_Should_Post_To_Versioned_Orders_Endpoint()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        var success = await service.SyncOrderAsync(new Order());

        Assert.True(success);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/v1/orders", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task LoginAsync_Should_Post_To_Versioned_Auth_Endpoint()
    {
        var responseJson = JsonSerializer.Serialize(new
        {
            token = "token-1",
            tenantId = "tenant-1",
            role = "Admin"
        });
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        });
        var service = CreateService(handler);

        var response = await service.LoginAsync("admin", "secret");

        Assert.NotNull(response);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/v1/auth/login", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task SyncInventoryMovementAsync_Should_Post_To_Versioned_InventoryMovements_Endpoint()
    {
        var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        var success = await service.SyncInventoryMovementAsync(InventoryMovement.ProductSale(1, 1, "tenant-1", "order-1"));

        Assert.True(success);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/v1/inventorymovements", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    private static ApiService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new AppSettings
        {
            ApiSettings = new ApiSettings
            {
                BaseUrl = "https://pos.example.com/"
            }
        });

        return new ApiService(httpClient, settings);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public CapturingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }
    }
}
