using System;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers
{
    [Route("store")]
    public class StoreController : ControllerBase
    {
        private readonly deliveryContext db;
        public StoreController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStores(string city, int limit, int offset, string order)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStore([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("{id}/menu")]
        public async Task<IActionResult> GetMenu([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("{idStore}/menu/{idMenu}")]
        public async Task<IActionResult> GetMenuItem([FromRoute] long idStore, [FromRoute] long idMenu)
        {
            throw new NotImplementedException();
        }

        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateStore([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
    }
}