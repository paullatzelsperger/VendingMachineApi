using Microsoft.AspNetCore.Mvc;
using VendingMachine.Core.Services;
using VendingMachine.Model;
using VendingMachineApi.Authorization;
using VendingMachineApi.Core;
using VendingMachineApi.Models;

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
        if (authUser.Id == id || authUser.Roles.Contains(Role.Admin.ToString(), StringComparer.InvariantCultureIgnoreCase))
        {    var user = await userService.GetById(id);
            return MapResult(user);
            
        }
        return StatusCode(StatusCodes.Status403Forbidden);
    }

    [Authorize(Role.Admin)] //todo: discussion: should this be available to all users?
    [HttpGet(Name = "Get all users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var res = await userService.GetAll();
        return MapResult(res);
    }

    [Authorize]
    [HttpPut("{id}", Name = "Update User")]
    public async Task<IActionResult> UpdateUser(string id, UserDto newUserDetails)
    {
        var user = this.GetAuthenticatedUser();

        if (user.Id == id || user.Roles.Contains(Role.Admin.ToString(), StringComparer.InvariantCultureIgnoreCase))
        {
            var result = await userService.Update(id, newUserDetails);
            return MapResult(result);
        }

        return Forbid();
    }

    [Authorize]
    [HttpDelete("{id}", Name = "Delete user")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = this.GetAuthenticatedUser();

        if (user.Id == id || user.Roles.Contains(Role.Admin.ToString(), StringComparer.InvariantCultureIgnoreCase))
        {
            var result = await userService.Delete(id);
            return result.Succeeded ? Ok() : BadRequest(result.FailureMessage);
        }

        return Forbid();
    }
    
    private IActionResult MapResult(ServiceResult<ICollection<User>> result)
    {
        return result.Succeeded ? Ok(result.Content!.Select(u => u.AsDto()).ToList()) : BadRequest(result.FailureMessage);
    }
    
    private IActionResult MapResult(ServiceResult<User> result)
    {
        
        return result.Succeeded ? Ok(result.Content!.AsDto()) : BadRequest(result.FailureMessage);
    }
}