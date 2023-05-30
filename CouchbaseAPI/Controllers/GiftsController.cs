using Cache.Services;
using Document;
using Microsoft.AspNetCore.Mvc;

namespace CouchbaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class GiftsController : ControllerBase
    {
        private readonly IGiftsService _giftsService;

        public GiftsController(IGiftsService giftsService)
        {
            _giftsService = giftsService;
        }

        [HttpGet]
        [Route("/list")]
        public async Task<JsonResult> LoadList(CancellationToken token = default)
        {
            try
            {
                var wishlistItemList = await _giftsService.LoadWishlistItemAsync(token).ConfigureAwait(false);
                return new JsonResult(wishlistItemList);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("/list/get/{id}")]
        public async Task<JsonResult> GetWishListById(Guid id)
        {
            try
            {
                var wishlist = await _giftsService.GetWishlistByIdAsync(id).ConfigureAwait(false);
                return new JsonResult(wishlist with { Id = id });
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("/list/create-or-edit")]
        public async Task<IActionResult> CreateOrEdit(WishlistItemCreateOrUpdate documentToCreateOrUpdate)
        {
            try
            {
                await _giftsService.CreateOrEditAsync(documentToCreateOrUpdate).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete]
        [Route("/list/delete/{id}")]
        public async Task<IActionResult> Delete(Guid id) 
        {
            try
            {
                await _giftsService.DeleteAsync(id).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete]
        [Route("/list/soft-delete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                await _giftsService.DeleteAsync(id, true).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
