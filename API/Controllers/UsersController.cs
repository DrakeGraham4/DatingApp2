using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
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

            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

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

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            //Getting user, eagerly loading photos
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            //getting result back from photoservice
            var result = await _photoService.AddPhotoAsync(file);

            //Check to see result, if there is an error we return a bad request
            if (result.Error != null) return BadRequest(result.Error.Message);

            //if past error, create new photo
            var photo = new Photo
            {
                url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            //Check to see if user already has photos, if not set main photo
            if (user.Photos.Count == 0)
            {
                photo.isMain = true;
            }

            //Add photo
            user.Photos.Add(photo);

            //return photo
            if (await _userRepository.SaveAllAsync())
                return _mapper.Map<PhotoDto>(photo);

            //if all goes wrong another bad request
            return BadRequest("Problem adding photo");

        }
    }
}