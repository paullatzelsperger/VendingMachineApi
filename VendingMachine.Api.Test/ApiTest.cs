using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using VendingMachine.Core.DataAccess;
using VendingMachine.Data.DataAccess;
using VendingMachine.Model;

namespace VendingMachineApi.IntegrationTest;

//test should run one after the other, to avoid race conditions
[Collection("api test collection")]
public class ApiTest : IDisposable
{
    protected readonly HttpClient HttpClient;
    protected User TestUser => ReloadTestUser();
    private readonly WebApplicationFactory<Program> webAppFactory;

    protected readonly JsonSerializerOptions? JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IEntityStore<User> userStore;
    private const string TestUserId = "TestUser";

    protected ApiTest()
    {
        webAppFactory = new WebApplicationFactory<Program>();
        HttpClient = webAppFactory.CreateDefaultClient();


        var user = new User
        {
            Username = "TestUser",
            Deposit = 100,
            Id = TestUserId,
            Password = "p4ssw0rd",
            Roles = new[] { "buyer", "seller", "admin" }
        };
        var type = typeof(IEntityStore<User>);

        userStore = GetService(type) as IEntityStore<User> ?? throw new InvalidOperationException();
        userStore.Save(user);
        HttpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(user.Username, user.Password);
    }

    protected object? GetService(Type type)
    {
        return webAppFactory.Services.GetService(type);
    }

    protected AuthenticationHeaderValue? CreateBasicAuthHeader(string user, string pwd)
    {
        var credential = $"{user}:{pwd}";
        return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
    }
    
    private User ReloadTestUser()
    {
        var t = userStore?.FindById(TestUserId);
        t?.Wait();
        return t?.Result!;
    }

    public void Dispose()
    {
        new UserContext().Database.EnsureDeleted();
        new UserContext().Database.EnsureCreated();
        new ProductContext().Database.EnsureDeleted();
        new ProductContext().Database.EnsureCreated();
        
    }
}