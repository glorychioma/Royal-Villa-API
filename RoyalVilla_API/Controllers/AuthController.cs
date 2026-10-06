using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authservice;

        public AuthController(IAuthService authService)
        {
            _authservice = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseDTO<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status409Conflict)]

        public async Task<ActionResult<ApiResponseDTO<UserDTO>>> Register([FromBody]RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                //authService
                if (registrationRequestDTO == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Registration data is required."));
                }

                if (await _authservice.IsEmailExists(registrationRequestDTO.Email))
                {
                    return Conflict(ApiResponseDTO<object>.Conflict($"The user with {registrationRequestDTO.Email} already exists."));
                }
                var user = await _authservice.RegisterAsync(registrationRequestDTO);

                if (user == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Registration failed"));
                }

                var response = ApiResponseDTO<UserDTO>.CreatedAt("Successfully registered a new user", user);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponseDTO<object>.Create(false, StatusCodes.Status500InternalServerError, "An internal server error occurred while processing the request.", ex.Message));

            }
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDTO<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponseDTO<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                //authService
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Login data is required."));
                }

                var user = await _authservice.LoginAsync(loginRequestDTO);

                if (user == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Login failed"));
                }

                var response = ApiResponseDTO<LoginResponseDTO>.Okay("Successfully logged in a new user", user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponseDTO<object>.Create(false, StatusCodes.Status500InternalServerError, "An internal server error occurred while trying to login.", ex.Message));

            }
        }

    }
}
