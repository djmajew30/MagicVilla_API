using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    //Can use the generic below to automatically route to Controller prefix:
    //not ideal if you need to change controller name
    //[Route("api/[controller]")] is the same as [Route("api/VillaAPI")] as It's in the VillaAPIController
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            return Ok(VillaStore.villaList); //200 success
        }

        [HttpGet("{id:int}", Name ="GetVilla")]
        //these are to document/remove undocumented
        //[ProducesResponseType(200, Type =typeof(VillaDTO))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO>  GetVilla(int id)
        {
            //19. add validation for bad request 400
            if (id == 0)
            {
                return BadRequest(); //400
            }

            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);

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
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            ////This is needed if we didn't use [ApiController] annotation in controller
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //add validations
            //not the right information
            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            //id should be 0
            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError); 
            }

            //Get/assign new id of the villa object, max id + 1 
            villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDTO);

            return CreatedAtRoute("GetVilla",new { id = villaDTO.Id }, villaDTO); //returns location: https://localhost:7245/api/VillaAPI/3 in response headers
        }

    }
}
