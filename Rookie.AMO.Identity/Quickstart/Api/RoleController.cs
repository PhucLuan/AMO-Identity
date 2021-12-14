using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Identity.Business.Interfaces;
using Rookie.AMO.Identity.ViewModel.UserModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.Quickstart.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            return await _roleService.GetAllRolesAsync();
        }
    }
}
