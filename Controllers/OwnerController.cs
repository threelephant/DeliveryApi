using System;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers
{
    [Route("owner")]
    public class OwnerController : ControllerBase
    {
        private readonly deliveryContext db;

        public OwnerController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet("{login}/stores")]
        public async Task<IActionResult> GetOwnerStores([FromRoute] string login)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("{login}/orders")]
        public async Task<IActionResult> GetOwnerOrders([FromRoute] string login, [FromRoute] string order)
        {
            throw new NotImplementedException();
        }
        
        //TODO: Add body request
        [HttpPost("store")]
        public async Task<IActionResult> AddStore()
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("store/{id}")]
        public async Task<IActionResult> DeleteStore([FromRoute] string id)
        {
            throw new NotImplementedException();
        }
        
        //TODO: Add body request
        [HttpPost("store/{id}/menu")]
        public async Task<IActionResult> AddStoreItem([FromRoute] string id)
        {
            throw new NotImplementedException();
        }
        
        //TODO: Add body request
        [HttpPut("store/{idStore}/menu/{idMenu}")]
        public async Task<IActionResult> ChangeStoreItem([FromRoute] string idStore, [FromRoute] string idMenu)
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("store/{idStore}/menu/{idMenu}")]
        public async Task<IActionResult> DeleteStoreItem([FromRoute] string idStore, [FromRoute] string idMenu)
        {
            throw new NotImplementedException();
        }
        
        [HttpPut("order/{id}")]
        public async Task<IActionResult> ChangeOrderStatus([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
    }
}