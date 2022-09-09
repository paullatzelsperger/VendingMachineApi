using VendingMachine.Model;

namespace VendingMachine.Core.Services;

public interface IVendingService
{
    /// <summary>
    /// Buys a product for a user. The total cost of amount * product.Cost is deducted from the user's
    /// deposit.
    /// </summary>
    /// <param name="user">The user who wants to buy the product</param>
    /// <param name="productId">The product ID</param>
    /// <param name="amount">Positive integer specifying the amount.</param>
    /// <returns>A failed response if the user does not have sufficient deposit, or the product does not have
    /// enough AmountAvailable (=stock). The total cost, the product and the change are returned in case of success.</returns>
    Task<ServiceResult<PurchaseResponse>> Buy(User user, string productId, int amount);

    /// <summary>
    /// Resets a user's deposit to 0.
    /// </summary>
    /// <param name="user">The user whos deposit is to be reset.</param>
    /// <returns>Failed response if user not found, success with no content otherwise</returns>
    Task<ServiceResult<object>> ResetBalance(User user);

    /// <summary>
    /// Adds a certain amount to a user's deposit. 
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="amount">Only 5, 10, 20, 50, 100 are acceptable values</param>
    /// <returns>A failed result if the amount is invalid or the user does not exist.</returns>
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
        await userService.Update(user.Id, user.AsDto());

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
        var res = await userService.Update(user.Id, user.AsDto());
        return res.Succeeded ? ServiceResult<object>.Success() : ServiceResult<object>.Failure(res.FailureMessage!);
    }

    public async Task<ServiceResult<object>> Deposit(User user, int amount)
    {
        if (!ValidateAmount(amount))
        {
            return ServiceResult<object>.Failure("Invalid deposit");
        }

        user.Deposit += amount;
        var updateRes = await userService.Update(user.Id, user.AsDto());

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