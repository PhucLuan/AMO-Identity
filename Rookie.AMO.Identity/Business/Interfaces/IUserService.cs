using Rookie.AMO.Identity.DataAccessor.Entities;
using Rookie.AMO.Identity.ViewModel;
using Rookie.AMO.Identity.ViewModel.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllAsync(Expression<Func<User, bool>> filter = null);
        Task<UserDto> CreateUserAsync(UserRequest userRequest, string adminLocation);
        Task UpdateUserAsync(Guid id, UserUpdateRequest request);
        Task DisableUserAsync(Guid userId);
        Task EnableUserAsync(Guid userId);
        Task<PagedResponseModel<UserDto>> PagedQueryAsync(string name, string type, int page, 
            int limit, string propertyName, bool desc, string search, string location);
        Task<IEnumerable<UserListDto>> GetUserNameByListId(List<string> ListUserId);
    }
}
