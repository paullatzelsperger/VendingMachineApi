using Moq;
using VendingMachine.Core.Services;
using VendingMachine.Model;
using VendingMachine.Model.Models;

namespace VendingMachineTest;

public class VendingServiceTest
{
    private readonly IVendingService vendingService;
    private readonly Mock<IProductService> productServiceMock;
    private readonly Mock<IUserService> userServiceMock;

    public VendingServiceTest()
    {
        productServiceMock = new Mock<IProductService>();
        userServiceMock = new Mock<IUserService>();

        vendingService = new VendingService(userServiceMock.Object, productServiceMock.Object);
    }

    [Fact]
    public async Task TestBuy()
    {
        var u = TestUser();
        u.Deposit = 5000;
        var p = TestProduct();
        const int amount = 17;
        var productId = p.Id;

        userServiceMock.Setup(x => x.GetById(u.Id)).ReturnsAsync(() => ServiceResult<User>.Success(u));
        productServiceMock.Setup(x => x.GetById(productId)).ReturnsAsync(() => ServiceResult<Product>.Success(p));
        productServiceMock.Setup(x => x.SellAmount(productId, amount)).ReturnsAsync(() => ServiceResult<int>.Success(p.AmountAvailable.GetValueOrDefault(0) - amount));

        var res = await vendingService.Buy(u, productId, amount);
        Assert.True(res.Succeeded);

        var resp = res.Content!;
        Assert.Equal(850, resp.TotalAmountSpent);
        Assert.Equal(2, resp.Change.Length);
        Assert.Equal(41, resp.Change[0].Amount);
        Assert.Equal(1, resp.Change[1].Amount);
    }

    [Fact]
    public async Task TestBuy_NotEnoughFunds()
    {
        var u = TestUser();
        u.Deposit = 50; //not enough
        var p = TestProduct();

        userServiceMock.Setup(x => x.GetById(u.Id)).ReturnsAsync(() => ServiceResult<User>.Success(u));
        productServiceMock.Setup(x => x.GetById("some-product")).ReturnsAsync(() => ServiceResult<Product>.Success(p));

        var res = await vendingService.Buy(u, "some-product", 17);
        Assert.False(res.Succeeded);
        Assert.Equal("Insufficient funds", res.FailureMessage);
    }

    [Fact]
    public async Task TestBuy_NotEnoughStock()
    {
        var u = TestUser();
        u.Deposit = 5000;
        var p = TestProduct();
        const int amount = 17;
        var productId = p.Id;

        userServiceMock.Setup(x => x.GetById(u.Id)).ReturnsAsync(() => ServiceResult<User>.Success(u));
        productServiceMock.Setup(x => x.GetById(productId)).ReturnsAsync(() => ServiceResult<Product>.Success(p));
        productServiceMock.Setup(x => x.SellAmount(productId, amount)).ReturnsAsync(() => ServiceResult<int>.Failure("Not enough stock"));


        var res = await vendingService.Buy(u, productId, amount);
        Assert.False(res.Succeeded);
        Assert.Equal("Not enough stock", res.FailureMessage);
    }

    [Fact]
    public async Task TestBuy_ProductNotFound()
    {
        var u = TestUser();
        const int amount = 17;

        userServiceMock.Setup(x => x.GetById(u.Id)).ReturnsAsync(() => ServiceResult<User>.Success(u));
        productServiceMock.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(() => ServiceResult<Product>.Failure("Not found"));


        var res = await vendingService.Buy(u, "test-prod-id", amount);
        Assert.False(res.Succeeded);
        Assert.Equal("Not found", res.FailureMessage);
        productServiceMock.Verify(x => x.SellAmount(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task TestResetBalance()
    {
        var u = TestUser();

        userServiceMock.Setup(x => x.Update(u.Id, u.AsDto())).ReturnsAsync(() => ServiceResult<User>.Success(u));

        var res = await vendingService.ResetBalance(u);
        Assert.True(res.Succeeded);
        Assert.Null(res.Content);
    }

    [Fact]
    public async Task TestResetBalance_Failed()
    {
        var u = TestUser();
        userServiceMock.Setup(x => x.Update(u.Id, u.AsDto())).ReturnsAsync(() => ServiceResult<User>.Failure("Failed to update"));
        var res = await vendingService.ResetBalance(u);
        Assert.False(res.Succeeded);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task TestDeposit(int amount)
    {
        var u = TestUser();

        userServiceMock.Setup(x => x.Update(u.Id, u.AsDto())).ReturnsAsync(ServiceResult<User>.Success);

        var res = await vendingService.Deposit(u, amount);
        Assert.True(res.Succeeded);
        Assert.Null(res.Content);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(-69)]
    [InlineData(69)]
    [InlineData(30)]
    public async Task TestDeposit_InvalidAmount(int amount)
    {
        var u = TestUser();
        var res = await vendingService.Deposit(u, amount);
        Assert.False(res.Succeeded);
        Assert.Equal("Invalid deposit", res.FailureMessage);
        userServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TestDeposit_FailedToUpdate()
    {
        var u = TestUser();
        userServiceMock.Setup(x => x.Update(u.Id, u.AsDto())).ReturnsAsync(() => ServiceResult<User>.Failure("Failed to update"));

        var res = await vendingService.Deposit(u, 50);
        Assert.False(res.Succeeded);
        Assert.Equal("Failed to update", res.FailureMessage);
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