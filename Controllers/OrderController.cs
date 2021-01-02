using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var order = await db.Orders
                .Include(o => o.Store)
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }
            
            var products = db.OrderProducts
                .Where(p => p.OrderId == id)
                .Select(p => new
                {
                    id = p.ProductId,
                    title = p.Product.Title,
                    price = p.Product.Price,
                    weight = p.Product.Weight,
                    count = p.Count
                }).AsEnumerable();
            
            var response = new
            {
                store_title = order.Store.Title,
                order_status = order.Status.Name,
                date_order = order.OrderDate,
                date_taking = order.TakingDate,
                date_delivery = order.DeliveryDate,
                products
            };
            
            return Ok(response);
        }
        
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetOrderStatus([FromRoute] long id)
        {
            var order = await db.Orders
                .Include(o => o.Store)
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            var response = new
            {
                store_title = order.Store.Title,
                status = order.Status.Name
            };

            return Ok(response);
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