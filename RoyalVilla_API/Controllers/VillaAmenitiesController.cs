using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa-amenities")]
    [ApiController]
    public class VillaAmenitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public VillaAmenitiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDTO<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponseDTO<IEnumerable<VillaAmenitiesDTO>>>> GetVillaAmenities()
        {
            
                var villaAmenities = await _db.VillaAmenities.ToListAsync();
                var response = ApiResponseDTO<IEnumerable<VillaAmenitiesDTO>>.Okay("Successfully retrieved all Villa Amenities", _mapper.Map<IEnumerable<VillaAmenitiesDTO>>(villaAmenities));
                return response;
           
        }


        [HttpGet("{Id:int}")]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponseDTO<VillaAmenitiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id < 0)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Villa Id must be greater than 0"));
                }
                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(user => user.Id == id);
                if (villaAmenities == null)
                {
                    return NotFound(ApiResponseDTO<object>.NotFound($"Villa Amenities with Id of {id} was not found"));
                }
                var response = ApiResponseDTO<VillaAmenitiesDTO>.Okay($"Successfully retrieved Villa Amenities with Id of {id}", _mapper.Map<VillaAmenitiesDTO>(villaAmenities));
                return response;
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occured while trying to retrieve villa amenities with Id {id} : {ex.Message}" );
            }
            
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaAmenitiesCreateDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponseDTO<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Villa amenities data is required"));
                }
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(user => user.Id == villaAmenitiesDTO.VillaId);
                if (existingVilla == null)
                {
                    return Conflict(ApiResponseDTO<object>.Conflict($"This villa with Id {villaAmenitiesDTO.VillaId} does not exist."));
                }
                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesDTO);
                villaAmenities.CreatedDate = DateTime.UtcNow;

                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();

                var response = CreatedAtAction(nameof(CreateVillaAmenities), new { Id = villaAmenitiesDTO.Id }, ApiResponseDTO<VillaAmenitiesDTO>.CreatedAt("Successfully created a new Villa Amenities", _mapper.Map<VillaAmenitiesDTO>(villaAmenities)));
                return response;
            }
            //catch (Exception ex)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, $"An internal server error occured while trying to create a new Villa amenities of ID {villaAmenitiesDTO.Id} : {ex.Message}");
            //}
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    InnerInnerException = ex.InnerException?.InnerException?.Message
                });
            }

        }


        [HttpPut("{Id:int}")]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaAmenitiesUpdateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaAmenitiesUpdateDTO>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponseDTO<VillaAmenitiesUpdateDTO>>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesUpdateDTO)
        {
            try
            {
                if (villaAmenitiesUpdateDTO == null)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Villa Amenities data is required."));
                }
                if (id < 0)
                {
                    return BadRequest(ApiResponseDTO<object>.BadRequst("Enter a valid villa amenities Id"));
                }
                if (id != villaAmenitiesUpdateDTO.Id)
                {
                    return ApiResponseDTO<VillaAmenitiesUpdateDTO>.BadRequst("Id in the url must match Id in the body");
                }
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(user => user.Id == villaAmenitiesUpdateDTO.VillaId);
                if (existingVilla == null)
                {
                    return Conflict(ApiResponseDTO<object>.Conflict($"This villa with Id {villaAmenitiesUpdateDTO.VillaId} does not exist."));
                }

                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(user => user.Id == id);
                if (villaAmenities == null)
                {
                    return ApiResponseDTO<VillaAmenitiesUpdateDTO>.NotFound($"Villa amenities data of Id {id} not found");
                }

                _mapper.Map(villaAmenitiesUpdateDTO, villaAmenities);
                villaAmenities.UpdatedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return ApiResponseDTO<VillaAmenitiesUpdateDTO>.Okay($"Successfully updated Villa with ID {villaAmenitiesUpdateDTO.Id}", _mapper.Map<VillaAmenitiesUpdateDTO>(villaAmenities));
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An internal error occured while trying to update this Villa amenities of Id {id} : {ex.Message} ");
            }           


        }

        [HttpDelete("{Id:int}")]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponseDTO<object>>> DeleteVillaAmenities(int id)
        {
            try
            {
                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVillaAmenities == null)
                {
                    return ApiResponseDTO<object>.NotFound($"Villa amenities with ID {id} was not found");
                }
                _db.VillaAmenities.Remove(existingVillaAmenities);
                await _db.SaveChangesAsync();

                return ApiResponseDTO<object>.NoContent($"Successfully deleted Villa with ID {id}");


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the villa amenities: {ex.Message}");
            }
        }


    }
}
