using FluentAssertions;
using Moq;
using VendingMachine.Core.DataAccess;
using VendingMachine.Core.Services;
using VendingMachine.Model;

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
        res.Succeeded.Should().BeTrue();
        res.Content.Should().BeEquivalentTo(p);
    }

    [Fact]
    public async Task TestGetById_NotFound()
    {
        var p = TestProduct();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);

        var res = await productService.GetById(p.Id);
        res.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task TestCreate()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(p);

        var res = await productService.Create(u, p);
        res.Succeeded.Should().BeTrue();
        res.Content.Should().BeEquivalentTo(p);
        p.SellerId.Should().Be(u.Id);
    }

    [Fact]
    public async Task TestCreate_Exists()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);
        productStoreMock.Setup(x => x.Save(p)).ReturnsAsync(() => p);

        var res = await productService.Create(u, p);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Exists");
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
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Invalid cost");
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
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Invalid amount");
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
        res.Succeeded.Should().BeTrue();
        res.Content.Should().BeEquivalentTo(p);
    }

    [Fact]
    public async Task TestDelete_NotExist()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => null);

        var res = await productService.Delete(u, p.Id);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not found");
        productStoreMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task TestDelete_UserNotSeller()
    {
        var p = TestProduct();
        var u = TestUser();
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.Delete(u, p.Id);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not Authorized");
        
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
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Failed to delete");
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

        res.Succeeded.Should().BeTrue();
        res.Content.Should().NotBeNull();
        res.Content!.Cost.Should().Be(150);
        res.Content.AmountAvailable.Should().Be(19);
        res.Content.Name.Should().Be("New Name");
        
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

        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not found");
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

        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not Authorized");
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

        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Invalid cost");
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

        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Invalid amount");
    }

    [Fact]
    public async Task TestSellAmount()
    {
        var p = TestProduct();
        var u = TestUser();
        p.SellerId = u.Id;
        productStoreMock.Setup(x => x.FindById(p.Id)).ReturnsAsync(() => p);

        var res = await productService.SellAmount(p.Id, 12);

        res.Succeeded.Should().BeTrue();
        res.Content.Should().Be(8);
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

        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not enough stock");
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