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

namespace MagicVilla_VillaAPI.Controllers
{

    //Can use the generic below to automatically route to Controller prefix:
    //not ideal if you need to change controller name
    //[Route("api/[controller]")] is the same as [Route("api/VillaAPI")] as It's in the VillaAPIController
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {

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
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            ////33. custom logger, changes reverted
            //_logger.Log("Getting all villas", "");
            //_logger.Log("Getting all villas", "warning"); //test warning loggingv2
            _logger.LogInformation("Getting all villas");

            //return Ok( await _db.Villas.ToListAsync()); //200 success
            //47 automapper
            //get villa list
            IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
            //convert to villaDTO
            return Ok(_mapper.Map<List<VillaDTO>>(villaList)); //Map<destination type>(object to convert) //<output>(input)
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        //these are to document/remove undocumented
        //[ProducesResponseType(200, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
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
                return NotFound(); //404
            }

            //return Ok(villa);
            return Ok(_mapper.Map<VillaDTO>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]//want to give 201 created instead
        //[ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            ////This is needed if we didn't use [ApiController] annotation in controller
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //24. Custom ModelState Validation, unique/distinct villa name
            if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already Exists!"); //key:value. Key must be unique
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
            //if (createDTO.Id > 0)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            ////Get/assign new id of the villa object, max id + 1
            ////removed lesson 41. no longer needed with EF core
            //createDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            //VillaStore.villaList.Add(createDTO);

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
            Villa model = _mapper.Map<Villa>(createDTO); //<output>(input)

            //await _db.Villas.AddAsync(model); //this step populates id field
            //await _db.SaveChangesAsync();
            await _dbVilla.CreateAsync(model); //lesson 50 repo

            //return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO); //returns location: https://localhost:7245/api/VillaAPI/3 in response headers
            return CreatedAtRoute("GetVilla", new { id = model.Id }, model); //returns location: https://localhost:7245/api/VillaAPI/3 in response headers
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        //use IActionResult instead of ActionResult becuase you do not need to define the return type
        public async Task<IActionResult> DeleteVilla(int id)
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

            //_db.Villas.Remove(villa);
            //await _db.SaveChangesAsync();
            await _dbVilla.RemoveAsync(villa);

            return NoContent(); //204
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            if (updateDTO == null || id != updateDTO.Id)
            {
                return BadRequest();
            }

            //get villa from list. removed lesson 41.
            //var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            ////Update/Edit. removed lesson 41.
            //villa.Name = updateDTO.Name;
            //villa.Sqft = updateDTO.Sqft;
            //villa.Occupancy = updateDTO.Occupancy;

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

            //_db.Villas.Update(model);
            //await _db.SaveChangesAsync();
            await _dbVilla.UpdateAsync(model);

            return NoContent();
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
