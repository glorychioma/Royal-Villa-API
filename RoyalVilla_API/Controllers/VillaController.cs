using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]    
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        //[Authorize("Admin")]
        [ProducesResponseType(typeof(ApiResponseDTO<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDTO<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _db.Villa.ToListAsync();

            return ApiResponseDTO<IEnumerable<VillaDTO>>.Okay("Successfully retrieved all villas", _mapper.Map<IEnumerable<VillaDTO>>(villas));
           
        }

        [HttpGet("{Id:int}")]
        //[Authorize("Customer")]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDTO<VillaDTO>>> GetVillasById(int id)
        {
            try {
                if (id < 0)
                {
                    return ApiResponseDTO<VillaDTO>.BadRequst("Villa ID must be greater than 0");
                }
                var villa = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if (villa == null)
                {
                    return ApiResponseDTO<VillaDTO>.NotFound($"Villa with ID {id} was not found.");
                }
                return ApiResponseDTO<VillaDTO>.Okay($"Successfully retrieved villa with ID: {id}", _mapper.Map<VillaDTO>(villa));
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving villa with ID {id}: {ex.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDTO<VillaDTO>>> CreateVilla(VillaCreateDTO villaDto)
        {
            try
            {
                if (villaDto == null )
                {
                    return ApiResponseDTO<VillaDTO>.BadRequst("Villa data is required");
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name);
                if (duplicateVilla != null)
                {
                    return ApiResponseDTO<VillaDTO>.Conflict($"This {villaDto.Name} already exists.");                
                }

                Villa villa = _mapper.Map<Villa>(villaDto);

                await _db.Villa.AddAsync(villa);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateVilla), new {id = villa.Id }, ApiResponseDTO<VillaDTO>.CreatedAt("Successfully created a new villa", _mapper.Map<VillaDTO>(villa)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating the villa: {ex.Message}");
            }
        }

        [HttpPut("{Id:int}")]
        [ProducesResponseType(typeof(ApiResponseDTO<VillaUpdateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDTO<VillaUpdateDTO>>> CreateVilla(int id, VillaUpdateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                {
                    return ApiResponseDTO<VillaUpdateDTO>.BadRequst($"Villa data is required");
                }
                if (id != villaDto.Id)
                {
                    return ApiResponseDTO<VillaUpdateDTO>.BadRequst("Villa ID in url does not match villa ID in request body");
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return ApiResponseDTO<VillaUpdateDTO>.NotFound($"Villa with ID {id} was not found");                
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower() && u.Id != villaDto.Id);
                if (duplicateVilla != null)
                {
                    return ApiResponseDTO<VillaUpdateDTO>.Conflict($"This {villaDto.Name} name already exists.");                    
                }

                _mapper.Map(villaDto, existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;
                await _db.SaveChangesAsync();

                return ApiResponseDTO<VillaUpdateDTO>.Okay($"Successfully updated Villa with ID {villaDto.Id}", villaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the villa: {ex.Message}");
            }
        }

        [HttpDelete("{Id:int}")]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status404NotFound)]        
        [ProducesResponseType(typeof(ApiResponseDTO<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDTO<object>>> DeleteVilla(int id)
        {
            try
            { 
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return ApiResponseDTO<object>.NotFound($"Villa with ID {id} was not found");
                }
                _db.Remove(existingVilla);
                await _db.SaveChangesAsync();

                return ApiResponseDTO<object>.NoContent($"Successfully deleted Villa with ID {id}");
                
                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the villa: {ex.Message}");
            }
        }
    }
}
