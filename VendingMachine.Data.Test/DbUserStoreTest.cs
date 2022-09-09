using FluentAssertions;
using VendingMachine.Data.DataAccess;
using VendingMachine.Model;

namespace VendingMachine.Data.Test;

public class DbUserStoreTest : IDisposable
{
    private readonly DbUserStore store;

    public DbUserStoreTest()
    {
        store = new DbUserStore(); //uses in mem database
    }

    [Fact]
    public async void TestSave()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };
        await store.Save(user);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(user);
    }

    [Fact]
    public async void TestSave_WhenSameExists()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };
        await store.Save(user);

        await store.Save(user);
        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(user);
    }

    [Fact]
    public async void TestSave_WhenExistsDifferentProperties()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };
        await store.Save(user);

        user.Name = "Modified";
        await store.Save(user);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(user);
    }

    [Fact]
    public async void TestUpdate_NotExists()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };

        await store.Update(user);

        (await store.FindAll()).Should().BeEmpty();
    }

    [Fact]
    public async void TestUpdate_WhenExists()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };
        await store.Save(user);

        user.Name = "modified";

        await store.Update(user);

        (await store.FindAll()).Should().HaveCount(1).And.Contain(u => u.Name == "modified");
    }

    [Fact]
    public async void TestUpdate_MultipleCalls()
    {
        var user = new User
        {
            Deposit = 15, Id = "test-id", Name = "test user", Roles = Array.Empty<string>()
        };
        await store.Save(user);

        user.Name = "modified";
        await store.Update(user);

        user.Deposit = 99;
        await store.Update(user);

        user.Roles = new[] { "new role" };
        await store.Update(user);

        (await store.FindAll()).Should().HaveCount(1).And.AllBeEquivalentTo(user);
    }

    public void Dispose()
    {
        using var ctx = new UserContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();
    }
}