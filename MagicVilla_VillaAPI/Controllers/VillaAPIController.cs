using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
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
        //41.dependency injection to use ef data
        private readonly ApplicationDbContext _db;

        public VillaAPIController(ApplicationDbContext db, ILogger<VillaAPIController> logger)
        {
            _db = db;
            _logger = logger;
        }

        ////33. Custom logger instead of default logger (changes reverted in same lesson)
        //private readonly ILogging _logger;
        //public VillaAPIController(ILogging logger)
        //{
        //    _logger = logger;
        //}


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            ////33. custom logger, changes reverted
            //_logger.Log("Getting all villas", "");
            //_logger.Log("Getting all villas", "warning"); //test warning loggingv2
            _logger.LogInformation("Getting all villas");
            return Ok( await _db.Villas.ToListAsync()); //200 success
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

            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);

            //19. add validation for notfound 404
            if (villa == null)
            {
                return NotFound(); //404
            }

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]//want to give 201 created instead
        //[ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO villaDTO)
        {
            ////This is needed if we didn't use [ApiController] annotation in controller
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //24. Custom ModelState Validation, unique/distinct villa name
            if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already Exists!"); //key:value. Key must be unique
                return BadRequest(ModelState);
            }

            //add validations
            //not the right information
            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            ////id should be 0
            ////removed 44. VillaCreateDTO no longer uses Id
            //if (villaDTO.Id > 0)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            ////Get/assign new id of the villa object, max id + 1
            ////removed lesson 41. no longer needed with EF core
            //villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            //VillaStore.villaList.Add(villaDTO);

            //add lesson 41
            //manually map/assign properties. conversion needed becuase type villadto not type villa
            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            await _db.Villas.AddAsync(model); //this step populates id field
            await _db.SaveChangesAsync();

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
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();
            return NoContent(); //204
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
        {
            if (villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }

            //get villa from list. removed lesson 41.
            //var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            ////Update/Edit. removed lesson 41.
            //villa.Name = villaDTO.Name;
            //villa.Sqft = villaDTO.Sqft;
            //villa.Occupancy = villaDTO.Occupancy;

            //41. convert villadto to object villa
            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
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
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            //convert villa to villadto
            VillaUpdateDTO villaDTO = new()
            {
                Amenity = villa.Amenity,
                Details = villa.Details,
                Id = villa.Id,
                ImageUrl = villa.ImageUrl,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Rate = villa.Rate,
                Sqft = villa.Sqft
            };

            //validation
            if (villa == null)
            {
                return BadRequest();
            }

            //if found, apply the patch to the object
            patchDTO.ApplyTo(villaDTO, ModelState);

            //convert villadto back to villa to update record
            Villa model = new Villa()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            _db.Villas.Update(model);
            await _db.SaveChangesAsync();

            //validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}
