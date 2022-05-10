using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountsController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServices _tokenServices;
        public AccountsController(DataContext context,ITokenServices tokenService)
        {
            _context = context;
            _tokenServices = tokenService;
        }

        [HttpPost("register")]
        public  async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if( await UserExists(registerDto.UserName))
            return BadRequest("User already exists");
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return new UserDto{
                UserName = user.UserName, 
                Token = _tokenServices.CreateToken(user)
            };
        } 

        private async Task<bool> UserExists(string UserName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == UserName.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await 
            _context.Users
            .SingleOrDefaultAsync
            (x=>x.UserName == loginDto.UserName);   
            if(user == null) return Unauthorized("Invalid User"); 

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for(int i=0; i<computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }
            return new UserDto{
                UserName = user.UserName,
                Token = _tokenServices.CreateToken(user)
            };
        }
    }
}