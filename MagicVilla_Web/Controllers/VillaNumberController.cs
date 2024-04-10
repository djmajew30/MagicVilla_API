using AutoMapper;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Models.VM;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace MagicVilla_Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IVillaNumberService _villaNumberService;
        private readonly IMapper _mapper;
        private readonly IVillaService _villaService;

        public VillaNumberController(IVillaNumberService villaNumberService, IMapper mapper, IVillaService villaService)
        {
            _villaNumberService = villaNumberService;
            _mapper = mapper;
            _villaService = villaService;
        }

        public async Task<IActionResult> IndexVillaNumber()
        {
            List<VillaNumberDTO> list = new();

            var response = await _villaNumberService.GetAllAsync<APIResponse>();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<VillaNumberDTO>>(Convert.ToString(response.Result));
            }
            return View(list);
        }

        //Create Villa HTTPGET
        //get 
        public async Task<IActionResult> CreateVillaNumber()
        {
            //for dropdown
            VillaNumberCreateVM villaNumberVM = new();
            var response = await _villaService.GetAllAsync<APIResponse>();
            if (response != null && response.IsSuccess)
            {
                villaNumberVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>
                    (Convert.ToString(response.Result)).Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }); ;
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVillaNumber(VillaNumberCreateVM model)
        {
            if (ModelState.IsValid)
            {

                var response = await _villaNumberService.CreateAsync<APIResponse>(model.VillaNumber);
                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(IndexVillaNumber));
                }
            }
            return View(model);
        }

        ////GetAsync for updating a record
        //public async Task<IActionResult> UpdateVilla(int villaId)
        //{
        //    var response = await _villaService.GetAsync<APIResponse>(villaId);
        //    if (response != null && response.IsSuccess)
        //    {
        //        VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
        //        //convert to VillaUpdateDTO
        //        return View(_mapper.Map<VillaUpdateDTO>(model));

        //    }
        //    return NotFound();
        //}

        ////UpdateAsync for updating a record
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateVilla(VillaUpdateDTO model)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var response = await _villaService.UpdateAsync<APIResponse>(model);
        //        if (response != null && response.IsSuccess)
        //        {
        //            return RedirectToAction(nameof(IndexVilla));
        //        }
        //    }
        //    return View(model);
        //}

        ////GetAsync for delete
        //public async Task<IActionResult> DeleteVilla(int villaId)
        //{
        //    var response = await _villaService.GetAsync<APIResponse>(villaId);

        //    //dont need to check if (ModelState.IsValid)
        //    if (response != null && response.IsSuccess)
        //    {
        //        VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
        //        return View(model);
        //    }
        //    return NotFound();
        //}

        ////DeleteAsync for delete
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteVilla(VillaDTO model)
        //{

        //    var response = await _villaService.DeleteAsync<APIResponse>(model.Id);
        //    if (response != null && response.IsSuccess)
        //    {
        //        return RedirectToAction(nameof(IndexVilla));
        //    }

        //    return View(model);
        //}



    //}
}
}
