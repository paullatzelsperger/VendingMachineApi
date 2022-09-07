using VendingMachineApi.Models;

namespace VendingMachineApi.Services;

public interface IProductService
{
    Task<ServiceResult<ICollection<Product>>> GetAll();
    Task<ServiceResult<Product>> GetById(string productName);
    Task<ServiceResult<Product>> Create(User user, Product product);
    Task<ServiceResult<Product>> Delete(User user, string productId);
    Task<ServiceResult<Product>> Update(User user, string productId, Product product);
    Task<ServiceResult<int>> SellAmount(string product, int amount);
}

internal class ProductService : IProductService
{
    private readonly ICollection<Product> productStore;

    public ProductService()
    {
        productStore = new List<Product>();
    }

    public Task<ServiceResult<ICollection<Product>>> GetAll()
    {
        return Task.FromResult(ServiceResult<ICollection<Product>>.Success(productStore));
    }

    public Task<ServiceResult<Product>> GetById(string productId)
    {
        var p = productStore.FirstOrDefault(prod => prod.Id == productId);
        return Task.FromResult(p != null ? ServiceResult<Product>.Success(p) : ServiceResult<Product>.Failure("Not Found"));
    }

    public async Task<ServiceResult<Product>> Create(User user, Product product)
    {
        var res = await GetById(product.Id);
        if (res.Succeeded)
        {
            return ServiceResult<Product>.Failure("Conflict");
        }

        if (product.Cost % 5 != 0) return ServiceResult<Product>.Failure("Invalid cost!");
        if (product.AmountAvailable < 0) return ServiceResult<Product>.Failure("Invalid amount!");

        product.SellerId = user.Id; // make sure the correct seller ID is used

        productStore.Add(product);
        return ServiceResult<Product>.Success(product);
    }

    public async Task<ServiceResult<Product>> Delete(User user, string productId)
    {
        var res = await GetById(productId);
        if (res.Failed) return res;

        var existingProduct = res.Content!;

        if (existingProduct.SellerId != user.Id)
        {
            return ServiceResult<Product>.Failure("Not Authorized");
        }

        productStore.Remove(existingProduct);
        return ServiceResult<Product>.Success(existingProduct);
    }

    public async Task<ServiceResult<Product>> Update(User user, string productId, Product product)
    {
        var res = await GetById(productId);
        if (res.Failed) return res;

        if (product.SellerId != user.Id) // only update if updator == creator
        {
            return ServiceResult<Product>.Failure("Not Authorized");
        }

        var existingProduct = res.Content!;

        if (product.Cost % 5 != 0) return ServiceResult<Product>.Failure("Invalid amount!");
        if (product.AmountAvailable < 0) return ServiceResult<Product>.Failure("Invalid amount!");

        existingProduct.Cost = product.Cost;
        existingProduct.AmountAvailable = product.AmountAvailable;
        existingProduct.ProductName = product.ProductName;
        // do not update Id or SellerId, could potentially break the domain logic

        return ServiceResult<Product>.Success(existingProduct);
    }

    public async Task<ServiceResult<int>> SellAmount(string productId, int amount)
    {
        var result = await GetById(productId);
        if (result.Succeeded)
        {
            var product = result.Content!;
            if (product.AmountAvailable >= amount)
            {
                product.AmountAvailable -= amount;
                // no need for further action because we're dealing with an in-mem collection here    
                return ServiceResult<int>.Success(product.AmountAvailable.Value);
            }
            return ServiceResult<int>.Failure("Not enough stock");
        }

        return ServiceResult<int>.Failure(result.FailureMessage!);


    }
}