using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinarySettings;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepository, IMapper mapper,
            IOptions<CloudinarySettings> cloudinarySettings)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
            _cloudinarySettings = cloudinarySettings;

            Account account = new Account(
                _cloudinarySettings.Value.CloudName,
                _cloudinarySettings.Value.ApiKey,
                _cloudinarySettings.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _datingRepository.GetPhoto(id);
            if (photo == null) return BadRequest();
            var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
            return Ok(photoToReturn);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _datingRepository.GetUser(userId);
            if (user == null) return BadRequest();
            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();
            if (file == null) return BadRequest("You don't add photo!");
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                };

                photoForCreationDto.Url = uploadResult.Url.ToString();
                photoForCreationDto.PublicId = uploadResult.PublicId;

                var photo = _mapper.Map<Photo>(photoForCreationDto);
                if (!user.Photos.Any(x => x.IsMain))
                {
                    photo.IsMain = true;
                }
                user.Photos.Add(photo);

                if (await _datingRepository.SaveAll())
                {
                    var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                    return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
                }
            }
            return BadRequest("Could not add a photo!");
        }

        [HttpPut("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _datingRepository.GetUser(userId);
            if (user == null) return BadRequest();
            if (!user.Photos.Any(p => p.Id == id)) return Unauthorized();

            var photo = await _datingRepository.GetPhoto(id);
            if (photo.IsMain)
                return BadRequest("This is already the main photo!");
            var currentMainPhoto = await _datingRepository.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;
            photo.IsMain = true;
            if (await _datingRepository.SaveAll())
                return NoContent();
            return BadRequest("Could not send photo main!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _datingRepository.GetUser(userId);
            if (user == null) return BadRequest();
            if (!user.Photos.Any(p => p.Id == id)) return Unauthorized();

            var photo = await _datingRepository.GetPhoto(id);
            if (photo.IsMain)
                return BadRequest("You cannot delete your main photo!");

            if (photo.PublicId != null)
            {
                var deletePrams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deletePrams);
                if (result.Result == "ok")
                {
                    _datingRepository.Delete(photo);
                }
            }
            else
            {
                _datingRepository.Delete(photo);
            }

            if (await _datingRepository.SaveAll())
                return Ok();
            return BadRequest("Failed to delete the photo");
        }

    }
}