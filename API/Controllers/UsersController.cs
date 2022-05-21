using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]

    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();

            return Ok(users);
        }
        //Parameters in route
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        //No objects back because client has all data related to updated entity
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //Gives us username from token that API uses to authenticate user which is user being updated
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByUsernameAsync(username);

            //Saves us manually mapping between Dto and user object without it would look like
            //user.City = memberUpdateDto.City
            //user.Country = memberUpdateDto.City etc...
            //map(from, to)
            _mapper.Map(memberUpdateDto, user);

            //Flags as being updated by entity framework, guarantees no error
            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");

        }
    }
}