using Microsoft.AspNetCore.Mvc;
using VendingMachine.Core;
using VendingMachine.Core.Services;
using VendingMachine.Model;
using VendingMachineApi.Authorization;

namespace VendingMachineApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;

    public ProductController(IProductService productService)
    {
        this.productService = productService;
    }

    [Authorize]
    [HttpGet(Name = "Get all products")]
    public async Task<IActionResult> GetProducts()
    {
        var result = await productService.GetAll();
        return new OkObjectResult(result.Content);
    }

    [Authorize]
    [HttpGet("{id}", Name = "Get product by id")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var result = await productService.GetById(id);

        return result.Succeeded
            ? Ok(result.Content)
            : BadRequest(result.FailureMessage);
    }

    [Authorize(Role.Seller)]
    [HttpPost(Name = "Create Product")]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var res = await productService.Create(this.GetAuthenticatedUser(), product);
        return res.Succeeded ? Ok(res.Content) : BadRequest(res.FailureMessage);
    }


    [Authorize(Role.Seller)]
    [HttpPut("{id}", Name = "Update product")]
    public async Task<IActionResult> UpdateProduct(string id, Product newProduct)
    {
        var authenticatedUser = this.GetAuthenticatedUser();

        var updateResult = await productService.Update(authenticatedUser,id, newProduct);
        return updateResult.Succeeded ? Ok(updateResult.Content) : MapResult(updateResult);
    }

    [Authorize(Role.Seller)]
    [HttpDelete("{id}", Name = "Delete product")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var au = this.GetAuthenticatedUser();
        var result = await productService.Delete(au, id);

        return result.Succeeded ? Ok() : MapResult(result);
    }

    private IActionResult MapResult(ServiceResult<Product> result)
    {
        // todo: could be improved to avoid string comparison
        return result.FailureMessage == "Not Authorized" ? StatusCode(StatusCodes.Status403Forbidden) : BadRequest(result.FailureMessage);
    }

}