using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Identity.ViewModel;
using Rookie.AMO.Identity.ViewModel.UserModels;
using Rookie.AMO.Identity.Business.Interfaces;
using Rookie.AMO.Identity.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServerHost.Quickstart.UI;
using EnsureThat;
using PasswordGenerator;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Rookie.AMO.Identity.Business.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<User> userManager, IMapper mapper, IEmailSender emailSender)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailSender = emailSender;

        }
        public async Task<UserDto> CreateUserAsync(UserRequest userRequest, string adminLocation)
        {
            userRequest.UserName = Regex.Replace(userRequest.UserName, @"\s", "");
            var user = _mapper.Map<User>(userRequest);
            user.LastModified = DateTime.UtcNow;
            user.Location = adminLocation;
            if (user.Type != "Admin") { 
                user.CodeStaff = AutoGenerateStaffCode();           
            }

            var passwordGenerator = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: true, includeSpecial: true, passwordLength: 8);
            var randomPassword = "12345678"; //passwordGenerator.Next();

            var createUserResult = await _userManager.CreateAsync(user, randomPassword);

            if (!createUserResult.Succeeded)
            {
                throw new Exception(createUserResult.Errors.First().Description);
            }

            var claims = new List<Claim>()
            {
                new Claim(IdentityModel.JwtClaimTypes.GivenName, user.FirstName),
                new Claim(IdentityModel.JwtClaimTypes.FamilyName, user.LastName),
                new Claim(IdentityModel.JwtClaimTypes.Role, user.Type),
                new Claim("location", adminLocation)
            };

            var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);

            if (!addClaimsResult.Succeeded)
            {
                throw new Exception("Unexpected errors! Add claims operation is not success.");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, userRequest.Type);

            if (!addRoleResult.Succeeded)
            {
                throw new Exception("Unexpected errors! Add role operation is not success.");
            }

            //----------Send password-------------------------
            await _emailSender.SendEmailAsync(
                userRequest.Email, 
                "Rookies - AMO",
                $"<h1>Rookie - AMO</h1><p>Use this password for your first login: {randomPassword}</p>"
            );
            //------------------------------------------------

            return _mapper.Map<UserDto>(user);
        }

        public async Task DisableUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception("User not found!");
            }
            user.Disable = true;
            await _userManager.UpdateAsync(user);
        }

        public async Task EnableUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception("User not found!");
            }
            user.Disable = false;
            await _userManager.UpdateAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(Expression<Func<User, bool>> filter = null)
        {
            var user = _userManager.Users;
            if (filter != null) {
                user = user.Where(filter);
            }
            return _mapper.Map<IEnumerable<UserDto>>(await user.ToListAsync());
        }

        public async Task<UserDto> GetByIdAsync(Guid userId)
        {
            return _mapper.Map<UserDto>(await _userManager.FindByIdAsync(userId.ToString()));
        }

        public async Task<PagedResponseModel<UserDto>> PagedQueryAsync(
            string name, string type, int page, int limit, 
            string propertyName, bool desc, string search,
            string location)
        {
            var query = _userManager.Users.Where(x => x.Location == location);

            query = query.Where(x => String.IsNullOrEmpty(type)
                         || type.ToLower().Contains(x.Type.ToLower()))
                         .Where(x => String.IsNullOrEmpty(name)
                         || x.FullName.ToLower().Contains(name.ToLower())
                         || x.CodeStaff.Contains(name.ToLower()))
                                .Where(x => x.Disable == false);

            if (!String.IsNullOrEmpty(search)) {
                query = query.Where(x =>
                    x.CodeStaff.Contains(search) ||
                    x.FullName.Contains(search) ||
                    x.UserName.Contains(search)
                );
            }

            switch (propertyName)
            {
                case "StaffCode":
                    if (desc)
                        query = query.OrderByDescending(a => a.CodeStaff);
                    else
                        query = query.OrderBy(a => a.CodeStaff);
                    break;
                case "LastModified":
                    query = query.OrderByDescending(a => a.LastModified);
                    break;
                case "FullName":
                    if (desc)
                        query = query.OrderByDescending(a => a.FullName);
                    else
                        query = query.OrderBy(a => a.FullName);
                    break;
                case "JoinedDate":
                    if (desc)
                        query = query.OrderByDescending(a => a.JoinedDate);
                    else
                        query = query.OrderBy(a => a.JoinedDate);
                    break;
                case "Type":
                    if (desc)
                        query = query.OrderByDescending(a => a.Type);
                    else
                        query = query.OrderBy(a => a.Type);
                    break;
                default:
                    query = (IOrderedQueryable<User>)query.OrderByPropertyName(propertyName, desc);
                    break;
            }
            var assets = await query
                .AsNoTracking()
                .PaginateAsync(page, limit);

            return new PagedResponseModel<UserDto>
            {
                CurrentPage = assets.CurrentPage,
                TotalPages = assets.TotalPages,
                TotalItems = assets.TotalItems,
                Items = _mapper.Map<IEnumerable<UserDto>>(assets.Items)
            };
        }

        public async Task UpdateUserAsync(Guid id, UserUpdateRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            Ensure.Any.IsNotNull(user, nameof(user));

            var claims = await _userManager.GetClaimsAsync(user);

            if (user.Type != request.Type)
            {
                await _userManager.RemoveFromRoleAsync(user, user.Type);
                await _userManager.AddToRoleAsync(user, request.Type);
                var newClaim = new Claim(IdentityModel.JwtClaimTypes.Role, request.Type);
                await _userManager.ReplaceClaimAsync(user, claims.First(x => x.Type == IdentityModel.JwtClaimTypes.Role), newClaim);
                user.Type = request.Type;
            }

            bool fullNameChange = false;

            if (user.FirstName != request.FirstName)
            {
                var newClaim = new Claim(IdentityModel.JwtClaimTypes.GivenName, request.FirstName);
                await _userManager.ReplaceClaimAsync(user, claims.First(x => x.Type == IdentityModel.JwtClaimTypes.GivenName), newClaim);
                user.FirstName = request.FirstName;
                fullNameChange = true;
            }

            if (user.LastName != request.LastName)
            {
                var newClaim = new Claim(IdentityModel.JwtClaimTypes.FamilyName, request.LastName);
                await _userManager.ReplaceClaimAsync(user, claims.First(x => x.Type == IdentityModel.JwtClaimTypes.FamilyName), newClaim);
                user.LastName = request.LastName;
                fullNameChange = true;
            }

            if (fullNameChange)
            {
                var requestFullName = $"{request.FirstName} {request.LastName}";
                user.FullName = requestFullName;
            }
            user.LastModified = DateTime.UtcNow;
            user.Gender = request.Gender;
            user.JoinedDate = request.JoinedDate;
            user.DateOfBirth = request.DateOfBirth;

            await _userManager.UpdateAsync(user);
        }

        private string AutoGenerateStaffCode()
        {
            var staffCode = new StringBuilder();
            var userList = _userManager.Users
                                    .OrderByDescending(x => Convert.ToInt32(
                                        x.CodeStaff.Replace(UserContants.PrefixStaffCode, "")
                                     ));
            if (!userList.Any())
            {
                return UserContants.PrefixStaffCode + "0001";
            }

            var latestCode = userList.First().CodeStaff;
            var nextNumber = Convert.ToInt32(latestCode.Replace(UserContants.PrefixStaffCode, "")) + 1;
            staffCode.Append(UserContants.PrefixStaffCode);
            staffCode.Append(nextNumber.ToString("0000"));
            return staffCode.ToString();
        }

        public async Task<IEnumerable<UserListDto>> GetUserNameByListId(List<string> ListUserId)
        {
            IEnumerable<UserListDto> model = new List<UserListDto>();
            model = await _userManager.Users.Where(x => ListUserId.Contains(x.Id)).Select(u =>
               new UserListDto
               {
                   UserID = u.Id,
                   UserName = u.UserName
               }).ToListAsync();

            return model;
        }
    }
}
