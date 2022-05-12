using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Identity.Business.Interfaces;
using Rookie.AMO.Identity.ViewModel;
using Rookie.AMO.Identity.ViewModel.UserModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.Net.Http.Headers;
using Rookie.AMO.Identity.Contract;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rookie.AMO.Identity.Quickstart.Api
{
    [EnableCors("AllowOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebAPIProvider _webAPIProvider;

        public UserController(IUserService userService, IWebAPIProvider webAPIProvider)
        {
            _userService = userService;
            _webAPIProvider = webAPIProvider;
        }
        
        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _userService.GetAllAsync();
        }

        [HttpGet("GetUserbyListId")]
        public async Task<IEnumerable<UserListDto>> GetUserbyListId([FromBody] List<string> ListUserId)
        {
            return await _userService.GetUserNameByListId(ListUserId);
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("{userId}")]
        public async Task<UserDto> GetByIdAsync(Guid userId)
        {
            return await _userService.GetByIdAsync(userId);
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPost("find")]
        public async Task<PagedResponseModel<UserDto>> PagedQueryAsync
        (FilterUserModel filterUserModel)
        {
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            return await _userService.PagedQueryAsync(
                filterUserModel.name,
                filterUserModel.type,
                filterUserModel.page,
                filterUserModel.limit,
                filterUserModel.propertyName,
                filterUserModel.desc,
                filterUserModel.search, adminLocation);
            //return await _userService.PagedQueryAsync(name, type, page, limit, propertyName, desc, search, adminLocation);
        }


        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUserAsync(UserRequest user)
        {

            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            var userDto = await _userService.CreateUserAsync(user, adminLocation);
            return Ok(userDto);
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DisableUserAsync(Guid id)
        {
            try
            {
                await _userService.DisableUserAsync(id);
                
            }
            catch {
                return NotFound();
            }
            
            return Ok();
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPut("{id}")]
        public async Task Update(Guid id, [FromBody] UserUpdateRequest request)
        {
            await _userService.UpdateUserAsync(id, request);
        }


    }
}
