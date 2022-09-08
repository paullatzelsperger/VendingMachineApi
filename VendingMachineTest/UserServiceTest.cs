using Moq;
using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;
using VendingMachineApi.Services;

namespace VendingMachineTest;

public class UserServiceTest
{
    private readonly IUserService userService;
    private readonly Mock<IUserStore> userStoreMock;

    public UserServiceTest()
    {
        userStoreMock = new Mock<IUserStore>();
        userService = new UserService(userStoreMock.Object);
    }

    [Fact]
    public void Test1()
    {
        Assert.NotNull(userService);
    }

    [Fact]
    public async Task TestCreate()
    {
        var res = await userService.Create(TestUser());
        Assert.True(res.Succeeded);
    }

    [Fact]
    public async Task TestCreate_WhenExists()
    {
        userStoreMock.Setup(store => store.FindById(It.IsAny<string>())).ReturnsAsync(TestUser);

        var res = await userService.Create(TestUser());
        Assert.False(res.Succeeded);
        Assert.Equal("Exists", res.FailureMessage);
    }

    [Fact]
    public async Task Update()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(x => x.Update(It.IsAny<User>())).ReturnsAsync(true);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u);
        Assert.True(res.Succeeded);
    }

    [Fact]
    public async Task Update_WhenNotExists()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => null);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u);
        Assert.True(res.Failed);
        Assert.Equal("Not Found", res.FailureMessage, ignoreCase: true);
    }

    [Fact]
    public async Task Update_WhenFails()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Update(It.IsAny<User>())).ReturnsAsync(false);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u);
        Assert.True(res.Failed);
        Assert.Equal("Failed to update", res.FailureMessage, ignoreCase: true);
    }

    [Fact]
    public async Task Delete()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(true);

        Assert.True((await userService.Delete(u.Id)).Succeeded);
    }

    [Fact]
    public async Task Delete_WhenNotExists()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => null);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(true);

        var res = await userService.Delete(u.Id);
        Assert.False(res.Succeeded);
        Assert.Equal("Not Found", res.FailureMessage);
    }

    [Fact]
    public async Task Delete_WhenDeleteFails()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(false);

        var res = await userService.Delete(u.Id);
        Assert.False(res.Succeeded);
        Assert.Equal("Failed to remove", res.FailureMessage);
    }

    [Fact]
    public async Task TestGetByName()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName("testname")).ReturnsAsync(() => u);
        var res = await userService.GetByName("testname");
        Assert.True(res.Succeeded);
        Assert.Equal(u, res.Content);

        var res2 = await userService.GetByName("not-exist");
        Assert.False(res2.Succeeded);
    }

    [Fact]
    public async Task TestGetById()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Id)).ReturnsAsync(() => u);
        var res = await userService.GetByName(u.Id);
        Assert.True(res.Succeeded);
        Assert.Equal(u, res.Content);

        var res2 = await userService.GetByName("not-exist");
        Assert.False(res2.Succeeded);
    }

    [Fact]
    public async Task TestAuthenticate()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Username)).ReturnsAsync(() => u);

        var res = await userService.Authenticate(u.Username, u.Password!);
        Assert.True(res.Succeeded);
    }

    [Fact]
    public async Task TestAuthenticate_WrongUsername()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Username)).ReturnsAsync(() => u);

        var res = await userService.Authenticate("not-exist", "some-pwd");
        Assert.True(res.Failed);
        Assert.Equal("Not Found", res.FailureMessage);
    }

    [Fact]
    public async Task TestAuthenticate_WrongPassword()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Username)).ReturnsAsync(() => u);

        var res = await userService.Authenticate(u.Username, "wrong-pwd");
        Assert.True(res.Failed);
        Assert.Equal("Authentication failed", res.FailureMessage);
    }

    private User TestUser()
    {
        return new User
        {
            Deposit = 10,
            Id = "TestId",
            Password = "randomPwd",
            Roles = new[] { "Admin" },
            Username = "TestUser"
        };
    }
}