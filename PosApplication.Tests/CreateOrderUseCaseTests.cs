using System.Threading.Tasks;
using Moq;
using PosApplication.UseCases.Orders;
using PosDomain;
using PosDomain.Entities;
using PosDomain.Interfaces;
using Xunit;

namespace PosApplication.Tests;

public class CreateOrderUseCaseTests
{
    [Fact]
    public async Task CreateOrderAsync_ShouldInitializeCorrectly_AndSaveToRepository()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var useCase = new CreateOrderUseCase(mockRepo.Object);

        // Act
        var result = await useCase.CreateOrderAsync("test-tenant", "user-123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("test-tenant", result.Value.TenantId);
        Assert.Equal("user-123", result.Value.CreatedById);
        Assert.Equal(OrderStatus.Open, result.Value.Status);

        mockRepo.Verify(r => r.AddAsync(It.Is<Order>(o => o.TenantId == "test-tenant" && o.CreatedById == "user-123")), Times.Once);
    }

    [Fact]
    public async Task AddItemToOrderAsync_ShouldReturnFailure_WhenOrderDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);
        var useCase = new CreateOrderUseCase(mockRepo.Object);
        var product = new Product { Id = 1, Price = 10m };

        // Act
        var result = await useCase.AddItemToOrderAsync(999, product, 2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found.", result.Error);
    }

    [Fact]
    public async Task AddItemToOrderAsync_ShouldAddProductToOrder_AndSaveToRepository()
    {
        // Arrange
        var order = new Order { Id = 1, Status = OrderStatus.Open, TenantId = "test" };
        var product = new Product { Id = 10, Price = 15m, TenantId = "test" };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        var useCase = new CreateOrderUseCase(mockRepo.Object);

        // Act
        var result = await useCase.AddItemToOrderAsync(1, product, 3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(order.Items);
        Assert.Equal(10, order.Items[0].ProductId);
        Assert.Equal(3, order.Items[0].Quantity);
        mockRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task CompleteOrderAsync_ShouldReturnFailure_WhenOrderDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);
        var useCase = new CreateOrderUseCase(mockRepo.Object);

        // Act
        var result = await useCase.CompleteOrderAsync(999, "authorized-user");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found.", result.Error);
    }

    [Fact]
    public async Task CompleteOrderAsync_ShouldComplete_AndSaveToRepository()
    {
        // Arrange
        var order = new Order { Id = 1, Status = OrderStatus.Open };
        var product = new Product { Id = 5, Price = 20m };
        order.AddItem(product, 1);

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        var useCase = new CreateOrderUseCase(mockRepo.Object);

        // Act
        var result = await useCase.CompleteOrderAsync(1, "authorized-user");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Closed, order.Status);
        Assert.Equal("authorized-user", order.AuthorizedBy);
        mockRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }
}
