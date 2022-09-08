using Moq;
using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;
using VendingMachineApi.Services;

namespace VendingMachineTest;

public class ProductServiceTest
{
    private readonly IProductService productService;
    private readonly Mock<IEntityStore<Product>> productStoreMock;

    public ProductServiceTest()
    {
        productStoreMock = new Mock<IEntityStore<Product>>();
        productService = new ProductService(productStoreMock.Object);
    }

    [Fact]
    public async Task TestGetById()
    {
        var p = TestProduct();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.GetById(p.Id);
        Assert.True(res.Succeeded);
        Assert.Equal(p, res.Content);
    }

    [Fact]
    public async Task TestGetById_NotFound()
    {
        var p = TestProduct();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);

        var res = await productService.GetById(p.Id);
        Assert.False(res.Succeeded);
    }

    [Fact]
    public async Task TestCreate()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(p);

        var res = await productService.Create(u, p);
        Assert.True(res.Succeeded);
        Assert.Equal(p, res.Content);
        Assert.Equal(u.Id, p.SellerId);
    }

    [Fact]
    public async Task TestCreate_Exists()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(() => p);

        var res = await productService.Create(u, p);
        Assert.False(res.Succeeded);
        Assert.Equal("Exists", res.FailureMessage);
        productStoreMock.Verify(x => x.Save(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(69)]
    [InlineData(null)]
    [InlineData(int.MaxValue)]
    public async Task TestCreate_InvalidCost(int? cost)
    {
        var p = TestProduct();
        p.Cost = cost;
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(() => p);

        var res = await productService.Create(u, p);
        Assert.False(res.Succeeded);
        Assert.Equal("Invalid cost", res.FailureMessage);
        productStoreMock.Verify(x => x.Save(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(-8)]
    [InlineData(null)]
    public async Task TestCreate_InvalidAmount(int? amount)
    {
        var p = TestProduct();
        p.AmountAvailable = amount;
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(() => p);

        var res = await productService.Create(u, p);
        Assert.False(res.Succeeded);
        Assert.Equal("Invalid amount", res.FailureMessage);
        productStoreMock.Verify(x => x.Save(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task TestDelete()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Delete(p)).ReturnsAsync(() => true);

        var res = await productService.Delete(u, p.Id);
        Assert.True(res.Succeeded);
        Assert.Equal(p, res.Content);
    }

    [Fact]
    public async Task TestDelete_NotExist()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);

        var res = await productService.Delete(u, p.Id);
        Assert.False(res.Succeeded);
        Assert.Equal("Not found", res.FailureMessage);
        productStoreMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task TestDelete_UserNotSeller()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.Delete(u, p.Id);
        Assert.False(res.Succeeded);
        Assert.Equal("Not authorized", res.FailureMessage, true);
        productStoreMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task TestDelete_DeleteFails()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Delete(p)).ReturnsAsync(() => false);

        var res = await productService.Delete(u, p.Id);
        Assert.False(res.Succeeded);
        Assert.Equal("Failed to delete", res.FailureMessage, true);
    }

    [Fact]
    public async Task TestUpdate()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Update(It.IsAny<Product>())).ReturnsAsync(() => true);

        var newProduct = new Product
        {
            Cost = 150,
            AmountAvailable = 19,
            ProductName = "New Name",
            Id = "should-not-change"
        };

        var res = await productService.Update(u, p.Id, newProduct);

        Assert.True(res.Succeeded);
        Assert.Equal(150, res.Content!.Cost);
        Assert.Equal(19, res.Content!.AmountAvailable);
        Assert.Equal("New Name", res.Content.Name);
    }

    [Fact]
    public async Task TestUpdate_NotExist()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);

        var newProduct = new Product
        {
            Cost = 150,
            AmountAvailable = 19,
            ProductName = "New Name",
            Id = "should-not-change"
        };

        var res = await productService.Update(u, p.Id, newProduct);

        Assert.False(res.Succeeded);
        Assert.Equal("Not found", res.FailureMessage);
    }

    [Fact]
    public async Task TestUpdate_UserNotSeller()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Update(It.IsAny<Product>())).ReturnsAsync(() => true);

        var newProduct = new Product
        {
            Cost = 150,
            AmountAvailable = 19,
            ProductName = "New Name",
            Id = "should-not-change"
        };

        var res = await productService.Update(u, p.Id, newProduct);

        Assert.False(res.Succeeded);
        Assert.Equal("Not authorized", res.FailureMessage, true);
    }

    [Theory]
    [InlineData(69)]
    [InlineData(null)]
    [InlineData(int.MaxValue)]
    public async Task TestUpdate_InvalidCost(int? cost)
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Update(It.IsAny<Product>())).ReturnsAsync(() => true);

        var newProduct = new Product
        {
            Cost = cost, //invalid
            AmountAvailable = 19,
            ProductName = "New Name",
            Id = "should-not-change"
        };

        var res = await productService.Update(u, p.Id, newProduct);

        Assert.False(res.Succeeded);
        Assert.Equal("Invalid cost", res.FailureMessage, true);
    }

    [Theory]
    [InlineData(-8)]
    [InlineData(null)]
    public async Task TestUpdate_InvalidAmount(int? amount)
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Update(It.IsAny<Product>())).ReturnsAsync(() => true);

        var newProduct = new Product
        {
            Cost = 50,
            AmountAvailable = amount, //invalid
            ProductName = "New Name",
            Id = "should-not-change"
        };

        var res = await productService.Update(u, p.Id, newProduct);

        Assert.False(res.Succeeded);
        Assert.Equal("Invalid amount", res.FailureMessage, true);
    }

    [Fact]
    public async Task TestSellAmount()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.SellAmount(p.Id, 12);

        Assert.True(res.Succeeded);
        Assert.Equal(8, res.Content);
        productStoreMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
        productStoreMock.Verify(x => x.Save(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task TestSellAmount_NotEnoughStock()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.SellAmount(p.Id, 22);

        Assert.False(res.Succeeded);
        Assert.Equal("Not enough stock", res.FailureMessage);
        productStoreMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        productStoreMock.Verify(x => x.Save(It.IsAny<Product>()), Times.Never);

    }

    private Product TestProduct()
    {
        return new Product
        {
            Cost = 50,
            Id = "Test-Id",
            AmountAvailable = 20,
            ProductName = "Test Product",
            SellerId = "12345"
        };
    }

    private User TestUser()
    {
        return new User
        {
            Deposit = 10,
            Id = "TestId",
            Password = "randomPwd",
            Roles = new[] { "Admin" },
            Name = "TestUser"
        };
    }
}