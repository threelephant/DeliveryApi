using System;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers
{
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly deliveryContext db;

        public AdminController(deliveryContext db)
        {
            this.db = db;
        }
        
        [HttpGet("user")]
        public async Task<IActionResult> GetUsers([FromRoute] string city, string order)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("courier/candidate")]
        public async Task<IActionResult> GetCouriersCandidate([FromRoute] string city, string order)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("store/candidate")]
        public async Task<IActionResult> GetStoresCandidate([FromRoute] string city, string order)
        {
            throw new NotImplementedException();
        }
        
        [HttpPut("courier/{login}/status")]
        public async Task<IActionResult> ChangeCourierStatus([FromRoute] string login, [FromBody] string status)
        {
            throw new NotImplementedException();
        }
        
        [HttpPut("store/{login}/status")]
        public async Task<IActionResult> ChangeStoreStatus([FromRoute] string login, [FromBody] string status)
        {
            throw new NotImplementedException();
        }
    }
}