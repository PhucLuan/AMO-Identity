using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Rookie.AMO.Identity.ViewModel.UserModels;
using Rookie.AMO.Identity.DataAccessor.Entities;

namespace Rookie.AMO.Identity.Business
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            FromDataAccessorLayer();
            FromPresentationLayer();
        }

        private void FromPresentationLayer()
        {
            CreateMap<UserRequest, User>(MemberList.Destination)
                .ForMember(u => u.FullName, x => x.MapFrom(u => $"{u.FirstName} {u.LastName}"))
                .ForMember(u => u.LastModified, ig => ig.Ignore());
            CreateMap<UserUpdateRequest, User>(MemberList.Destination);
        }

        private void FromDataAccessorLayer()
        {
            CreateMap<User, UserDto>(MemberList.Destination);
            CreateMap<IdentityRole, RoleDto>(MemberList.Destination);
        }
    }
}
