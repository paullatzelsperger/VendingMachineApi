using FluentAssertions;
using Moq;
using VendingMachine.Core.DataAccess;
using VendingMachine.Core.Services;
using VendingMachine.Model;

namespace VendingMachineTest;

public class UserServiceTest
{
    private readonly IUserService userService;
    private readonly Mock<IEntityStore<User>> userStoreMock;

    public UserServiceTest()
    {
        userStoreMock = new Mock<IEntityStore<User>>();
        userService = new UserService(userStoreMock.Object);
    }

    [Fact]
    public async Task TestCreate()
    {
        var res = await userService.Create(TestUser());
        res.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task TestCreate_WhenExists()
    {
        userStoreMock.Setup(store => store.FindById(It.IsAny<string>())).ReturnsAsync(TestUser);

        var res = await userService.Create(TestUser());
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Exists");
    }

    [Fact]
    public async Task Update()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(x => x.Update(It.IsAny<User>())).ReturnsAsync(true);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u.AsDto());
        res.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WhenNotExists()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => null);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u.AsDto());
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not Found");
    }

    [Fact]
    public async Task Update_WhenFails()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Update(It.IsAny<User>())).ReturnsAsync(false);

        u.Roles = new[] { "SomeNewRole" };

        var res = await userService.Update(u.Id, u.AsDto());
        
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Failed to update");
    }

    [Fact]
    public async Task Delete()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(true);
        (await userService.Delete(u.Id)).Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_WhenNotExists()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => null);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(true);

        var res = await userService.Delete(u.Id);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not Found");
    }

    [Fact]
    public async Task Delete_WhenDeleteFails()
    {
        var u = TestUser();
        userStoreMock.Setup(s => s.FindById(It.IsAny<string>())).ReturnsAsync(() => u);
        userStoreMock.Setup(s => s.Delete(It.IsAny<User>())).ReturnsAsync(false);

        var res = await userService.Delete(u.Id);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Failed to remove");
    }

    [Fact]
    public async Task TestGetById()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindById(u.Id)).ReturnsAsync(() => u);
        var res = await userService.GetById(u.Id);
        res.Succeeded.Should().BeTrue();
        res.Content.Should().BeEquivalentTo(u);

        var res2 = await userService.GetById("not-exist");
        res2.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task TestGetById_Notfound()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindById(u.Id)).ReturnsAsync(() => null);
        var res = await userService.GetById(u.Id);
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not found");

        var res2 = await userService.GetById("not-exist");
        res2.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuthenticate()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Name)).ReturnsAsync(() => u);

        var res = await userService.Authenticate(u.Name, u.Password!);
        res.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task TestAuthenticate_WrongUsername()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Name)).ReturnsAsync(() => u);

        var res = await userService.Authenticate("not-exist", "some-pwd");
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Not Found");
    }

    [Fact]
    public async Task TestAuthenticate_WrongPassword()
    {
        var u = TestUser();
        userStoreMock.Setup(x => x.FindByName(u.Name)).ReturnsAsync(() => u);

        var res = await userService.Authenticate(u.Name, "wrong-pwd");
        res.Succeeded.Should().BeFalse();
        res.FailureMessage.Should().Be("Authentication failed");
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