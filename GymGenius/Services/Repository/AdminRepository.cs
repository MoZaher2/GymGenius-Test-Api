using GymGenius.Models;
using GymGenius.Models.Identity;
using GymGenius.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;

namespace GymGenius.Services.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ResponseGeneral> AddUserToRoleAsync(string UserNameOrID, string RoleName)
        {
            var RG = new ResponseGeneral();

            var user = await _userManager.FindByNameAsync(UserNameOrID);
            if (user == null)
            {
                user = await _userManager.FindByIdAsync(UserNameOrID);

                if (user == null)
                {
                    RG.Message = $"User With UserNameOrID : '{UserNameOrID}' not found.";
                    RG.Done = false;
                }
            }

            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                RG.Message = $"Role {RoleName} not found.";
                RG.Done = false;
            }

            await _userManager.AddToRoleAsync(user, RoleName);

            RG.Message = $"User {UserNameOrID} add to Role {RoleName} successfully.";
            RG.Done = true; 

            return RG;
        }

        public async Task<IEnumerable<object>> GetAllRolesAsync()
        {
            var data = await _roleManager.Roles.Select(role => new
            {
                id = role.Id,
                name = role.Name
            }).ToListAsync();

            return data; 
        }

        public async Task<IEnumerable<object>> GetAllUserByRoleNameAsync(string RoleName)
        {
            var userRole = await _userManager.GetUsersInRoleAsync(RoleName);
            if (userRole == null)
            {
                throw new NotFoundException($"No users found in role {RoleName}.");
            }

            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                throw new BadRequestException($"Role {RoleName} not found.");
            }

            var users = userRole.Select(user => new
            {
                id = user.Id,
                username = user.UserName
            }).ToList();

            return users;
        }

        public async Task<ResponseGeneral> RemoveUserFromRoleAsync(string UserNameOrID, string RoleName)
        {
            var RG = new ResponseGeneral();

            var user = await _userManager.FindByNameAsync(UserNameOrID);
            if (user == null)
            {
                user = await _userManager.FindByIdAsync(UserNameOrID);

                if (user == null)
                {
                    RG.Message = $"User With UserNameOrID : '{UserNameOrID}' not found.";
                    RG.Done = false ;
                }
            }

            if (!await _userManager.IsInRoleAsync(user, RoleName))
            {
                RG.Message = $"User {UserNameOrID} is not in role {RoleName}.";
                RG.Done = false ;
            }

            await _userManager.RemoveFromRoleAsync(user, RoleName);

            RG.Message = $"User {UserNameOrID} Remove to Role {RoleName} successfully.";
            RG.Done = true;

            return RG;
        }

        public async Task<ResponseGeneral> RemoveUserAsync(string UserNameO)
        {
            var response = new ResponseGeneral();

            // Check if the user exists
            var user = await _userManager.FindByNameAsync(UserNameO);
            if (user == null)
            {
                response.Message = $"User with username '{UserNameO}' not found.";
                response.Done = false;
                return response;
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                response.Message = $"User {UserNameO} deleted successfully.";
                response.Done = true;
            }
            else
            {
                // Handle errors if deletion fails
                response.Message = $"Failed to delete user {UserNameO}.";
                response.Done = false;
                foreach (var error in result.Errors)
                {
                    response.Message += $" {error.Description}";
                }
            }

            return response;
        }
    }
}
