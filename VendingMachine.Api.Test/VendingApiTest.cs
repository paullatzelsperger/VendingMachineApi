using System.Net;
using System.Text.Json;
using VendingMachine.Core.DataAccess;
using VendingMachine.Model;

namespace VendingMachineApi.IntegrationTest;

public class VendingApiTest : ApiTest
{
    private readonly IEntityStore<Product> productStore;


    public VendingApiTest()
    {
        productStore = (GetService(typeof(IEntityStore<Product>)) as IEntityStore<Product>)!;
    }

    [Fact]
    public async Task TestBuy()
    {
        const string productId = "test-product-id";
        var product = await productStore.Save(new Product
        {
            AmountAvailable = 50,
            Cost = 25,
            Name = "Test Product",
            Id = productId,
            SellerId = TestUser.Id
        });
        const int amount = 3;
        var response = await HttpClient.PostAsync($"api/vending/buy?productId={productId}&amount={amount}", null);

        Assert.True(response.IsSuccessStatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var purchaseResponse = JsonSerializer.Deserialize<PurchaseResponse>(json, JsonOptions)!;
        Assert.Equal(product, purchaseResponse.Product);
        Assert.Equal(75, purchaseResponse.TotalAmountSpent);
        Assert.Equal(2, purchaseResponse.Change.Length);
        Assert.Contains(purchaseResponse.Change, coin => coin.Amount == 1 && coin.Value == "20 Cent");
        Assert.Contains(purchaseResponse.Change, coin => coin.Amount == 1 && coin.Value == "5 Cent");
    }

    [Fact]
    public async Task TestBuy_InsufficientFunds()
    {
        const string productId = "test-product-id";
        await productStore.Save(new Product
        {
            AmountAvailable = 50,
            Cost = TestUser.Deposit * 2, //make sure TestUser can't afford it 
            Name = "Test Product",
            Id = productId,
            SellerId = TestUser.Id
        });

        const int amount = 1;
        var response = await HttpClient.PostAsync($"api/vending/buy?productId={productId}&amount={amount}", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Insufficient funds", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestBuy_InsufficientStock()
    {
        const string productId = "test-product-id";
        await productStore.Save(new Product
        {
            AmountAvailable = 2,
            Cost = TestUser.Deposit / 4, //make sure TestUser can afford it 
            Name = "Test Product",
            Id = productId,
            SellerId = TestUser.Id
        });

        const int amount = 3;
        var response = await HttpClient.PostAsync($"api/vending/buy?productId={productId}&amount={amount}", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Not enough stock", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestBuy_ProductNotFound()
    {
        const string productId = "notexist-product-id";
        const int amount = 3;
        var response = await HttpClient.PostAsync($"api/vending/buy?productId={productId}&amount={amount}", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Not Found", await response.Content.ReadAsStringAsync());
    }


    [Theory]
    [InlineData(75)]
    [InlineData(0)]
    [InlineData(-8)]
    public async Task TestDeposit_InvalidAmount(int amount)
    {
        var response = await HttpClient.PostAsync($"api/vending/deposit?amount={amount}", null);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal("Invalid deposit", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task TestDeposit_ValidAmount(int amount)
    {
        var response = await HttpClient.PostAsync($"api/vending/deposit?amount={amount}", null);
        Assert.True(response.IsSuccessStatusCode);
    }


    [Fact]
    public async Task TestDeposit_UserNotFound()
    {
        var credentials = CreateBasicAuthHeader("some-other", "some-pwd");
        HttpClient.DefaultRequestHeaders.Authorization = credentials;
        const int deposit = 50;
        var response = await HttpClient.PostAsync($"api/vending/deposit?amount={deposit}", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TestResetBalance_UserNotFound()
    {
        var credentials = CreateBasicAuthHeader("some-other", "some-pwd");
        HttpClient.DefaultRequestHeaders.Authorization = credentials;
        var response = await HttpClient.PostAsync($"api/vending/reset", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task TestResetBalance()
    {
        var response = await HttpClient.PostAsync($"api/vending/reset", null);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(0, TestUser.Deposit);
    }
}