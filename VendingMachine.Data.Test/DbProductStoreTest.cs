using FluentAssertions;
using VendingMachine.Data.DataAccess;
using VendingMachine.Model;

namespace VendingMachine.Data.Test;

public class DbProductStoreTest : IDisposable
{
  private readonly DbProductStore store;

    public DbProductStoreTest()
    {
        store = new DbProductStore(); //uses in mem database
    }

    [Fact]
    public async void TestSave()
    {
        var product = CreateProduct();
        await store.Save(product);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(product);
    }

    private static Product CreateProduct()
    {
        var product = new Product
        {
            Cost = 50, AmountAvailable = 10, Id = "test-product", ProductName = "test product", SellerId = "user123"
        };
        return product;
    }

    [Fact]
    public async void TestSave_WhenSameExists()
    {
        var product = CreateProduct();
        await store.Save(product);

        await store.Save(product);
        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(product);
    }

    [Fact]
    public async void TestSave_WhenExistsDifferentProperties()
    {
        var product = CreateProduct();
        await store.Save(product);

        product.Name = "Modified";
        await store.Save(product);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(product);
    }

    [Fact]
    public async void TestUpdate_NotExists()
    {
        var product= CreateProduct();

        await store.Update(product);

        (await store.FindAll()).Should().BeEmpty();
    }

    [Fact]
    public async void TestUpdate_WhenExists()
    {
        var product= CreateProduct();

        await store.Save(product);

        product.Name = "modified";

        await store.Update(product);

        (await store.FindAll()).Should().HaveCount(1).And.Contain(u => u.Name == "modified");
    }

    [Fact]
    public async void TestUpdate_MultipleCalls()
    {
        var product= CreateProduct();

        await store.Save(product);

        product.Name = "modified";
        await store.Update(product);

        product.AmountAvailable = 99;
        await store.Update(product);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(product);
    }

    public void Dispose()
    {
        using var ctx = new ProductContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();
    }
}