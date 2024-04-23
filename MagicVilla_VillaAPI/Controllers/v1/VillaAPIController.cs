using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers.v1
{

    //Can use the generic below to automatically route to Controller prefix:
    //not ideal if you need to change controller name
    //[Route("api/[controller]")] is the same as [Route("api/VillaAPI")] as It's in the VillaAPIController
    [Route("api/v{version:apiVersion}/VillaAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaAPIController : ControllerBase
    {
        //53. standard api response
        protected APIResponse _response;

        //31. Logger dependency injection- DEFAULT logger 
        private readonly ILogger<VillaAPIController> _logger;

        ////41.dependency injection to use ef data. removed 50. repository
        //private readonly ApplicationDbContext _db;

        //50 repository
        private readonly IVillaRepository _dbVilla;

        //47 automapper
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, ILogger<VillaAPIController> logger, IMapper mapper) //(ApplicationDbContext
        {
            //_db = db;
            _dbVilla = dbVilla;
            _logger = logger;
            _mapper = mapper;
            _response = new();
        }


        [HttpGet]
        //[ResponseCache(Duration = 30)] //seconds
        [ResponseCache(CacheProfileName = "Default30")] //114 profile caching
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _logger.LogInformation("Getting all villas");

                //get villa list
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();

                //convert to villaDTO
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList); //Map<destination type>(object to convert) //<output>(input)
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        //these are to document/remove undocumented
        //[ProducesResponseType(200, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [ResponseCache(Location =ResponseCacheLocation.None,NoStore =true)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                //19. add validation for bad request 400
                if (id == 0)
                {
                    ////33. custom logger, changes reverted
                    //_logger.Log("Get Villa Error with Id: " + id, "error");
                    _logger.LogError("Get Villa Error with Id: " + id);
                    return BadRequest(); //400
                }

                var villa = await _dbVilla.GetAsync(u => u.Id == id);

                //19. add validation for notfound 404
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa); //Map<destination type>(object to convert) //<output>(input)
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
            //return Ok(_mapper.Map<VillaDTO>(villa));
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]//want to give 201 created instead
        //[ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                ////This is needed if we didn't use [ApiController] annotation in controller
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                //24. Custom ModelState Validation, unique/distinct villa name
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already Exists!");//key:value. Key must be unique
                    return BadRequest(ModelState);
                }

                //add validations
                //not the right information
                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }

                ////id should be 0
                ////removed 44. VillaCreateDTO no longer uses Id
                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}

                ////add lesson 41. commented out lesson 47.
                ////manually map/assign properties. conversion needed becuase type villadto not type villa
                //Villa model = new()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //    ImageUrl = createDTO.ImageUrl,
                //    Name = createDTO.Name,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft
                //};

                //47. automapper. convert villaDTO to villa object
                Villa villa = _mapper.Map<Villa>(createDTO); //<output>(input)

                await _dbVilla.CreateAsync(villa); //lesson 50 repo
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);//returns location: https://localhost:7245/api/VillaAPI/3 in response headers
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [Authorize(Roles = "admin")] //CUSTOM does not exist, will not work. Done on purpose to show
        //use IActionResult instead of ActionResult becuase you do not need to define the return type
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                //get villa to delete
                var villa = await _dbVilla.GetAsync(u => u.Id == id);

                if (villa == null)
                {
                    return NotFound();
                }
                await _dbVilla.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent; //204
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    return BadRequest();
                }

                ////41. convert villadto to object villa. commented out in 47. automapper
                //Villa model = new()
                //{
                //    Amenity = updateDTO.Amenity,
                //    Details = updateDTO.Details,
                //    Id = updateDTO.Id,
                //    ImageUrl = updateDTO.ImageUrl,
                //    Name = updateDTO.Name,
                //    Occupancy = updateDTO.Occupancy,
                //    Rate = updateDTO.Rate,
                //    Sqft = updateDTO.Sqft
                //};

                //47. automapper to convert villaupdateDTO to villa
                Villa model = _mapper.Map<Villa>(updateDTO);

                await _dbVilla.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            //validation
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            //get vill a from list of villas
            //var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id); //removed in 50
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

            ////convert villa to villadto. removed in 47. automapper
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    Id = villa.Id,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft
            //};

            //47 automapper. convert villa to villaupdatedto
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            //validation
            if (villa == null)
            {
                return BadRequest();
            }

            //if found, apply the patch to the object
            patchDTO.ApplyTo(villaDTO, ModelState);

            ////convert villadto back to villa to update record. removed 47. automapper
            //Villa model = new Villa()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    Id = villaDTO.Id,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft
            //};

            //47. automapper. convert villaupdateDTO to villa model
            Villa model = _mapper.Map<Villa>(villaDTO);

            //_db.Villas.Update(model); //removed in 50
            //await _db.SaveChangesAsync();

            await _dbVilla.UpdateAsync(model);

            //validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}
