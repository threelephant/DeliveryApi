using System;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers
{
    [Route("order")]
    public class OrderController : ControllerBase
    {
        private readonly deliveryContext db;

        public OrderController(deliveryContext db)
        {
            this.db = db;
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetOrderStatus([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateOrder([FromRoute] long id)
        {
            throw new NotImplementedException();
        }

        //TODO: Add body request
        [HttpPost("{id}/cart")]
        public async Task<IActionResult> ActionOrder([FromRoute] long id)
        {
            throw new NotImplementedException();
        }
    }
}