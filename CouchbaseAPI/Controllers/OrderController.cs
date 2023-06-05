using Cache.Services;
using Microsoft.AspNetCore.Mvc;

namespace CouchbaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Route("/load/customer")]
        public async Task<IActionResult> LoadCustomer(CancellationToken token = default)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var customerList = await _orderService.LoadCustomerAsync(token);
                watch.Stop();
                return Ok($"{customerList.Count()} Records Load Time: {watch.ElapsedMilliseconds} milliseconds, {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds} seconds and {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes} minutes");
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("/load/order")]
        public async Task<IActionResult> LoadOrder(CancellationToken token = default)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var customerList = await _orderService.LoadOrderAsync(token);
                watch.Stop();
                return Ok($"{customerList.Count()} Records Load Time: {watch.ElapsedMilliseconds} milliseconds, {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds} seconds and {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes} minutes");
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("/load/order-with-customer")]
        public async Task<IActionResult> LoadOrderWithCustomer(CancellationToken token = default)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var customerList = await _orderService.LoadOrderWithCustomerAsync(token);
                watch.Stop();
                return Ok($"{customerList.Count()} Records Load Time: {watch.ElapsedMilliseconds} milliseconds, {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds} seconds and {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes} minutes");
            }
            catch (Exception)
            {

                throw;
            }
        }

        //[HttpGet]
        //[Route("/seed-data")]
        //public async Task<IActionResult> LoadList(CancellationToken token = default)
        //{
        //    try
        //    {
        //        await _orderService.SeedDataAsync(0, 20000, token);
        //        //5 million
        //       await _orderService.SeedDataAsync(20000, 5000000, token);
        //        return Ok();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}
