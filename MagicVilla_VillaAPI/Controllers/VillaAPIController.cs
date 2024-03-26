using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
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
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            return Ok(VillaStore.villaList); //200 success
        }

        [HttpGet("{id:int}")]
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
    }
}
