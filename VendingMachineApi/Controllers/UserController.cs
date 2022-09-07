using Microsoft.AspNetCore.Mvc;
using VendingMachineApi.Authorization;
using VendingMachineApi.Core;
using VendingMachineApi.Models;
using VendingMachineApi.Services;

namespace VendingMachineApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPost(Name = "Create User")]
    public async Task<IActionResult> CreateUser(User user)
    {
        var result = await userService.Create(user);
        return MapResult(result);
    }

    [Authorize] //todo: discussion: should this only be available to the authenticated user?
    [HttpGet("{id}", Name = "Get user by id")]
    public async Task<IActionResult> GetUser(string id)
    {
        var authUser = this.GetAuthenticatedUser();
        if (authUser.Id == id || authUser.Roles.Contains(Role.Admin.ToString()))
        {    var user = await userService.GetById(id);
            return MapResult(user);
            
        }
        return StatusCode(StatusCodes.Status403Forbidden);
    }

    [Authorize(Role.Admin)] //todo: discussion: should this be available to all users?
    public async Task<IActionResult> GetAllUsers()
    {
        var res = await userService.GetAll();
        return MapResult(res);
    }

    [Authorize(Role.Admin)]
    [HttpPut("{id}", Name = "Update User")]
    public async Task<IActionResult> UpdateUser(string id, User user)
    {
        var result = await userService.Update(id, user);
        return MapResult(result);
    }

    [Authorize(Role.Admin)]
    [HttpDelete("{id}", Name = "Delete user")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await userService.Delete(id);
        return result.Succeeded ? Ok() : BadRequest(result.FailureMessage);
    }
    
    private IActionResult MapResult(ServiceResult<ICollection<User>> result)
    {
        return result.Succeeded ? Ok(result.Content) : BadRequest(result.FailureMessage);
    }
    
    private IActionResult MapResult(ServiceResult<User> result)
    {
        return result.Succeeded ? Ok(result.Content) : BadRequest(result.FailureMessage);
    }
}