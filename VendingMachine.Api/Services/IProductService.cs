using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;

namespace VendingMachineApi.Services;

public interface IProductService
{
    Task<ServiceResult<ICollection<Product>>> GetAll();
    Task<ServiceResult<Product>> GetById(string productName);
    Task<ServiceResult<Product>> Create(User user, Product product);
    Task<ServiceResult<Product>> Delete(User user, string productId);
    Task<ServiceResult<Product>> Update(User user, string productId, Product newProduct);
    Task<ServiceResult<int>> SellAmount(string productId, int amount);
}

internal class ProductService : IProductService
{
    private readonly IEntityStore<Product> productStore;

    public ProductService(IEntityStore<Product> productStore)
    {
        this.productStore = productStore;
    }

    public async Task<ServiceResult<ICollection<Product>>> GetAll()
    {
        return ServiceResult<ICollection<Product>>.Success(await productStore.FindAll());
    }

    public async Task<ServiceResult<Product>> GetById(string productId)
    {
        var p = await productStore.FindById(productId);
        return p != null ? ServiceResult<Product>.Success(p) : ServiceResult<Product>.Failure("Not Found");
    }

    public async Task<ServiceResult<Product>> Create(User user, Product product)
    {
        var res = await productStore.FindById(product.Id);
        if (res != null)
        {
            return ServiceResult<Product>.Failure("Exists");
        }

        if (product.Cost == null || product.Cost % 5 != 0) return ServiceResult<Product>.Failure("Invalid cost");
        if (product.AmountAvailable is null or < 0) return ServiceResult<Product>.Failure("Invalid amount");

        product.SellerId = user.Id; // make sure the correct seller ID is used

        await productStore.Save(product);
        return ServiceResult<Product>.Success(product);
    }

    public async Task<ServiceResult<Product>> Delete(User user, string productId)
    {
        var existingProduct = await productStore.FindById(productId);
        if (existingProduct == null) return ServiceResult<Product>.Failure("Not found");


        if (existingProduct.SellerId != user.Id)
        {
            return ServiceResult<Product>.Failure("Not Authorized");
        }

        return await productStore.Delete(existingProduct) 
            ? ServiceResult<Product>.Success(existingProduct) 
            : ServiceResult<Product>.Failure("Failed to delete");
    }

    public async Task<ServiceResult<Product>> Update(User user, string productId, Product newProduct)
    {
        var existingProduct = await productStore.FindById(productId);
        if (existingProduct == null) return ServiceResult<Product>.Failure("Not found");

        if (existingProduct.SellerId != user.Id) // only update if updator == creator
        {
            return ServiceResult<Product>.Failure("Not Authorized");
        }


        if (newProduct.Cost is null || newProduct.Cost % 5 != 0) return ServiceResult<Product>.Failure("Invalid cost");
        if (newProduct.AmountAvailable is null or < 0) return ServiceResult<Product>.Failure("Invalid amount");

        existingProduct.Cost = newProduct.Cost;
        existingProduct.AmountAvailable = newProduct.AmountAvailable;
        existingProduct.ProductName = newProduct.ProductName;
        // do not update Id or SellerId, could potentially break the domain logic

        await productStore.Update(existingProduct);

        return ServiceResult<Product>.Success(existingProduct);
    }

    public async Task<ServiceResult<int>> SellAmount(string productId, int amount)
    {
        var product = await productStore.FindById(productId);
        if (product == null) return ServiceResult<int>.Failure("Not found");
        if (!(product.AmountAvailable >= amount)) return ServiceResult<int>.Failure("Not enough stock");

        product.AmountAvailable -= amount;

        await productStore.Update(product);

        return ServiceResult<int>.Success(product.AmountAvailable.Value);
    }
}