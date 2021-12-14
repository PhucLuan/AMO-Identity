using Rookie.AMO.Identity.ViewModel.UserModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.Business.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    }
}
