using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{

    //Can use the generic below to automatically route to Controller prefix:
    //not ideal if you need to change controller name
    //[Route("api/[controller]")] is the same as [Route("api/VillaNumberAPI")] as It's in the VillaNumberAPIController
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        //53. standard api response
        protected APIResponse _response;

        //31. Logger dependency injection- DEFAULT logger 
        private readonly ILogger<VillaNumberAPIController> _logger;

        ////41.dependency injection to use ef data. removed 50. repository
        //private readonly ApplicationDbContext _db;

        //50 repository
        private readonly IVillaNumberRepository _dbVillaNumber;

        //47 automapper
        private readonly IMapper _mapper;

        //57. villa number custom error
        private readonly IVillaRepository _dbVilla;

        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, ILogger<VillaNumberAPIController> logger, IMapper mapper, IVillaRepository dbVilla) //(ApplicationDbContext
        {
            //_db = db;
            _dbVillaNumber = dbVillaNumber;
            _logger = logger;
            _mapper = mapper;
            this._response = new();
            _dbVilla = dbVilla; 
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                _logger.LogInformation("Getting all villas");

                //get villa list. removed in 73.
                //IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync();
                //73 include villa in villanumbers
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties: "Villa");

                //convert to villaNumberDTO
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList); //Map<destination type>(object to convert) //<output>(input)
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

        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        //these are to document/remove undocumented
        //[ProducesResponseType(200, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                //19. add validation for bad request 400
                if (id == 0)
                {
                    ////33. custom logger, changes reverted
                    //_logger.Log("Get Villa Error with Id: " + id, "error");
                    _logger.LogError("Get VillaNumber Error with Id: " + id);
                    return BadRequest(); //400
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);

                //19. add validation for notfound 404
                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound; 
                    return NotFound(_response); 
                }
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber); //Map<destination type>(object to convert) //<output>(input)
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]//want to give 201 created instead
        //[ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                ////This is needed if we didn't use [ApiController] annotation in controller
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                //24. Custom ModelState Validation, unique/distinct villa name
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa Number already Exists!");//key:value. Key must be unique
                    return BadRequest(ModelState);
                }

                //57. villa number custom error/validation
                if (await _dbVilla.GetAsync(u => u.Id == createDTO.VillaID) == null) //null means VillaId is invalid
                {
                    ModelState.AddModelError("CustomError", "Villa ID is Invalid!");
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
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO); //<output>(input)

                await _dbVillaNumber.CreateAsync(villaNumber); //lesson 50 repo
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);//returns location: https://localhost:7245/api/VillaNumberAPI/3 in response headers
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        //use IActionResult instead of ActionResult becuase you do not need to define the return type
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                //get villa to delete
                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);

                if (villaNumber == null)
                {
                    return NotFound();
                }
                await _dbVillaNumber.RemoveAsync(villaNumber);
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

        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    return BadRequest();
                }

                //57. villa number custom error/validation
                if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaID) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa ID is Invalid!");
                    return BadRequest(ModelState);
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
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);

                await _dbVillaNumber.UpdateAsync(model);
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

        [HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO)
        {
            //validation
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            //get vill a from list of villas
            //var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id); //removed in 50
            var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id, tracked: false);

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
            VillaNumberUpdateDTO villaNumberDTO = _mapper.Map<VillaNumberUpdateDTO>(villaNumber);

            //validation
            if (villaNumber == null)
            {
                return BadRequest();
            }

            //if found, apply the patch to the object
            patchDTO.ApplyTo(villaNumberDTO, ModelState);

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
            VillaNumber model = _mapper.Map<VillaNumber>(villaNumberDTO);

            //_db.Villas.Update(model); //removed in 50
            //await _db.SaveChangesAsync();

            await _dbVillaNumber.UpdateAsync(model);

            //validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}
