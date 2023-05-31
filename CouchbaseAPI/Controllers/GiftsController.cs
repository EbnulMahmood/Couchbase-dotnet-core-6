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
        [Route("/list/with-customer")]
        public async Task<JsonResult> LoadListWithCustome(CancellationToken token = default)
        {
            try
            {
                var wishlistItemList = await _giftsService.LoadWishlistItemWithCustomerAsync(token).ConfigureAwait(false);
                return new JsonResult(wishlistItemList);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("/list/get/{id}")]
        public async Task<JsonResult> GetWishListById(Guid id, CancellationToken token = default)
        {
            try
            {
                var wishlist = await _giftsService.GetWishlistByIdAsync(id, token).ConfigureAwait(false);
                return new JsonResult(wishlist);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("/list/create-or-edit")]
        public async Task<IActionResult> CreateOrEdit(WishlistItemCreateOrUpdate documentToCreateOrUpdate, CancellationToken token = default)
        {
            try
            {
                await _giftsService.CreateOrEditAsync(documentToCreateOrUpdate, token).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete]
        [Route("/list/delete/{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken token = default)
        {
            try
            {
                await _giftsService.DeleteAsync(id, token: token).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete]
        [Route("/list/soft-delete/{id}")]
        public async Task<IActionResult> SoftDelete(Guid id, CancellationToken token = default)
        {
            try
            {
                await _giftsService.DeleteAsync(id, isSoftDelete: true, token).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
