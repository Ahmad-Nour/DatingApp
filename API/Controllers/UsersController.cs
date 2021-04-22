using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _datingRepository.GetUser(userId);

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "female" ? "male" : "female";
            userParams.UserId = userId;

            var users = await _datingRepository.GetUsers(userParams);

            if (users == null) return NotFound();
            var usersTOReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(usersTOReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepository.GetUser(id);
            if (user == null) return NotFound();

            var userTOReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userTOReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _datingRepository.GetUser(id);
            if (user == null) return BadRequest();

            _mapper.Map<UserForUpdateDto>(user);
            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }
            return BadRequest("Something wrong when add user with id: {id}");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            if (id == recipientId) return BadRequest("You cannont like your self!");
            var liked = await _datingRepository.GetLike(id, recipientId);
            if (liked != null) return BadRequest("You are already like this user!");

            if (await _datingRepository.GetUser(recipientId) == null)
                return NotFound();

            var like = new Like
            {
                LikeeId = recipientId,
                LikerId = id
            };
            await _datingRepository.Add<Like>(like);

            if (await _datingRepository.SaveAll())
                return Ok();

            return BadRequest();
        }
    }
}