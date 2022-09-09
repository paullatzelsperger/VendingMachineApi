using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VendingMachine.Core.DataAccess;
using VendingMachine.Model;
using VendingMachine.Model.Models;
using VendingMachineApi.Controllers;

namespace VendingMachineApi.IntegrationTest;

public class UserApiTest : ApiTest
{
    private readonly IEntityStore<User> userStore;

    public UserApiTest()
    {
        userStore = (GetService(typeof(IEntityStore<User>)) as IEntityStore<User>)!;
    }

    [Fact]
    public async Task TestCreateUser()
    {
        var user = CreateUser();
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var content = JsonContent.Create(user);
        var response = await HttpClient.PostAsync("/api/user", content);
        response.IsSuccessStatusCode.Should().BeTrue();
        (await response.Content.ReadAsStringAsync()).Should().NotBeNull();
    }

    [Fact]
    public async Task TestCreateUser_AlreadyExists()
    {
        var user = CreateUser();
        await userStore.Save(user);
        HttpClient.DefaultRequestHeaders.Authorization = null;
        var content = JsonContent.Create(user);
        var response = await HttpClient.PostAsync("/api/user", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("Exists");
    }

    [Fact]
    public async Task TestGetUserById()
    {
        var id = TestUser.Id;
        var response = await HttpClient.GetAsync($"api/user/{id}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var receivedUser = JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), JsonOptions);

        TestUser.AsDto().Should().BeEquivalentTo(receivedUser);
    }

    [Fact]
    public async Task TestGetUserById_NotAuthenticated()
    {
        var creds = CreateBasicAuthHeader("some-user", "some-pwd");
        HttpClient.DefaultRequestHeaders.Authorization = creds;

        var id = TestUser.Id;
        var response = await HttpClient.GetAsync($"api/user/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TestGetUserById_WhenOtherAndAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer", "admin" };
        await userStore.Save(user);

        var id = TestUser.Id;
        var creds = CreateBasicAuthHeader(user.Username!, user.Password!);
        HttpClient.DefaultRequestHeaders.Authorization = creds;

        var response = await HttpClient.GetAsync($"api/user/{id}");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TestGetUserById_WhenOtherAndNotAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer" };
        await userStore.Save(user);

        var id = TestUser.Id;
        var creds = CreateBasicAuthHeader(user.Username!, user.Password!);
        HttpClient.DefaultRequestHeaders.Authorization = creds;

        var response = await HttpClient.GetAsync($"api/user/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TestGetAllUsers()
    {
        var user = CreateUser();
        await userStore.Save(user);

        var response = await HttpClient.GetAsync("api/User");
        response.IsSuccessStatusCode.Should().BeTrue();
        var users = JsonSerializer.Deserialize<ICollection<User>>(await response.Content.ReadAsStringAsync(), JsonOptions);
        users.Should().HaveCount(2);
        users.Should().ContainEquivalentOf(TestUser.AsDto()).And.ContainEquivalentOf(user.AsDto());
    }

    [Fact]
    public async Task TestGetAllUsers_Unauthorized()
    {
        var user = CreateUser(); // no admin
        await userStore.Save(user);

        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        var response = await HttpClient.GetAsync("api/User");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TestUpdateUser_Self()
    {
        var user = CreateUser(); // no admin
        await userStore.Save(user);
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        var modifiedUser = CreateUser(); //identical
        modifiedUser.Username = "modified username";

        var response = await HttpClient.PutAsync($"api/user/{user.Id}", JsonContent.Create(modifiedUser));
        response.IsSuccessStatusCode.Should().BeTrue();
        JsonSerializer.Deserialize<UserDto>(await response.Content.ReadAsStringAsync(), JsonOptions)
                      .Should().BeEquivalentTo(modifiedUser.AsDto());
        (await userStore.FindAll()).Should().HaveCount(2);

    }

    [Fact]
    public async Task TestUpdateUser_OtherButAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer", "admin" }; // is allowed to modify
        await userStore.Save(user);

        var user2 = new User
        {
            Username = "Test User 2",
            Deposit = 50,
            Id = "testuser2",
            Password = "some-owd",
            Roles = Array.Empty<string>()
        };
        await userStore.Save(user2);
        
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        user2.Username = "modified username"; // modify username

        var response = await HttpClient.PutAsync($"api/user/{user2.Id}", JsonContent.Create(user2));
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TestUpdateUser_OtherNoAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer" }; // is allowed to modify
        await userStore.Save(user);

        var user2 = new User
        {
            Username = "Test User 2",
            Deposit = 50,
            Id = "testuser2",
            Password = "some-owd",
            Roles = Array.Empty<string>()
        };
        await userStore.Save(user2);
        
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        user2.Username = "modified username"; // modify username

        var response = await HttpClient.PutAsync($"api/user/{user2.Id}", JsonContent.Create(user2));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TestDeleteUser_Self()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer" }; // self is allowed to modify
        await userStore.Save(user);
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        var response = await HttpClient.DeleteAsync($"api/User/{user.Id}");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TestDeleteUser_OtherButAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer", "admin" }; // is allowed to modify
        await userStore.Save(user);

        var userToDelete = new User
        {
            Username = "delete-user", Deposit = 15, Id = "deluser", Password = "pwd", Roles = Array.Empty<string>()
        };
        await userStore.Save(userToDelete);
        
        
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        var response = await HttpClient.DeleteAsync($"api/User/{userToDelete.Id}");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TestDeleteUser_OtherNoAdmin()
    {
        var user = CreateUser();
        user.Roles = new[] { "buyer" }; // is not allowed to modify
        await userStore.Save(user);

        var userToDelete = new User
        {
            Username = "delete-user", Deposit = 15, Id = "deluser", Password = "pwd", Roles = Array.Empty<string>()
        };
        await userStore.Save(userToDelete);
        
        
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username!, user.Password!);

        var response = await HttpClient.DeleteAsync($"api/User/{userToDelete.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static User CreateUser()
    {
        var user = new User
        {
            Deposit = 10,
            Id = "some-id",
            Username = "some-user",
            Password = "test-pwd",
            Roles = new[] { "buyer" }
        };
        return user;
    }
}