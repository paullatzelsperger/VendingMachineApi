using VendingMachineApi.Models;

namespace VendingMachineApi.Services;

public interface IVendingService
{
    Task<ServiceResult<PurchaseResponse>> Buy(User user, string productId, int amount);
    Task<ServiceResult<object>> ResetBalance(User user);
    Task<ServiceResult<object>> Deposit(User user, int amount);
}

public class VendingService : IVendingService
{
    private static readonly int[] ValidAmounts = { 5, 10, 20, 50, 100 };
    private readonly IUserService userService;
    private readonly IProductService productService;

    public VendingService(IUserService userService, IProductService productService)
    {
        this.userService = userService;
        this.productService = productService;
    }

    public async Task<ServiceResult<PurchaseResponse>> Buy(User user, string productId, int amount)
    {
        // get product
        var productResult = await productService.GetById(productId);
        if (productResult.Failed) return ServiceResult<PurchaseResponse>.Failure(productResult.FailureMessage!);

        // check balance -- todo: move to user service?
        var product = productResult.Content!;
        var totalCost = product.Cost! * amount;
        if (user.Deposit < totalCost)
        {
            return ServiceResult<PurchaseResponse>.Failure("Insufficient funds");
        }

        // update data model
        user.Deposit -= totalCost;
        await userService.Update(user.Id, user);

        // also checks stock
        var serviceResult = await productService.SellAmount(product.Id, amount);
        if (serviceResult.Failed)
        {
            return ServiceResult<PurchaseResponse>.Failure(serviceResult.FailureMessage!);
        }

        // prepare response
        var change = CalculateChange(user.Deposit.GetValueOrDefault(0));

        var purchaseResponse = new PurchaseResponse
        {
            Product = product,
            TotalAmountSpent = totalCost,
            Change = change
        };

        return ServiceResult<PurchaseResponse>.Success(purchaseResponse);
    }

    public async Task<ServiceResult<object>> ResetBalance(User user)
    {
        user.Deposit = 0;
        var res = await userService.Update(user.Id, user);
        return res.Succeeded ? ServiceResult<object>.Success() : ServiceResult<object>.Failure(res.FailureMessage!);
    }

    public async Task<ServiceResult<object>> Deposit(User user, int amount)
    {
        if (!ValidateAmount(amount))
        {
            return ServiceResult<object>.Failure("Invalid deposit");
        }

        user.Deposit += amount;
        var updateRes = await userService.Update(user.Id, user);

        return updateRes.Succeeded ? ServiceResult<object>.Success() : ServiceResult<object>.Failure(updateRes.FailureMessage!);
    }

    private Coin[] CalculateChange(int userDeposit)
    {
        var remainder = userDeposit;
        var change = new List<Coin>(); //5, 10, 20, 50, 100 cents
        foreach (var value in ValidAmounts.Reverse())
        {
            var num = remainder / value;
            if (num > 0)
            {
                change.Add(new Coin { Value = $"{value} Cent", Amount = num });
            }
            remainder %= value;

        }

        return change.ToArray();
    }

    private bool ValidateAmount(int amount)
    {
        return ValidAmounts.Contains(amount);
    }
}