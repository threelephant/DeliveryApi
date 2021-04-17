using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts.Order;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderStatus = Delivery.Contracts.Order.OrderStatus;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы, связанные с заказами
    /// </summary>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly deliveryContext db;

        public OrderController(deliveryContext db)
        {
            this.db = db;
        }
        
        /// <summary>
        /// Информация о заказе
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Информация о заказе</response>
        /// <response code="403">Пользователь не является клиентом, владельцем предприятия или курьером,
        /// относящимся к данному заказу</response>
        /// <response code="404">Данного заказа не существует</response>
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

            if (!(order.UserLogin == User.Identity?.Name
                || order.CourierLogin == User.Identity?.Name
                || order.Store.OwnerLogin == User.Identity?.Name))
            {
                return Forbid();
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

        /// <summary>
        /// Информация о заказах, сделанных пользователем
        /// </summary>
        /// <response code="200">Информация о заказах, сделанных пользователем</response>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var order = await db.Orders
                .Where(o => o.UserLogin == User.Identity.Name)
                .Select(o => new
                {
                    id = o.Id,
                    store_title = o.Store.Title,
                    order_status = o.Status.Name
                })
                .ToListAsync();

            return Ok(order);
        }
        
        /// <summary>
        /// Информация о статусе заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Информация о статусе заказа</response>
        /// <response code="403">Пользователь не является клиентом, владельцем предприятия или курьером,
        /// относящимся к данному заказу</response>
        /// <response code="404">Данного заказа не существует</response>
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
            
            if (order.UserLogin != User.Identity?.Name
                || order.CourierLogin != User.Identity?.Name
                || order.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            var response = new
            {
                store_title = order.Store.Title,
                status = order.Status.Name
            };

            return Ok(response);
        }
        
        /// <summary>
        /// Оценка заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="rating">Оценка заказа</param>
        /// <response code="200">Заказ оценён</response>
        /// <response code="400">Заказ уже оценён</response>
        /// <response code="403">Оценивающий не является заказчиком</response>
        /// <response code="404">Данного заказа нет</response>
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateOrder([FromRoute] long id, [FromBody] RateOrder rating)
        {
            var order = await db.Orders
                .Include(o => o.Status)
                .Where(o => o.Status.Name == "Заказ доставлен")
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            if (order.UserLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            if (order.Rating != null)
            {
                return BadRequest(new { response = "Уже оценено" });
            }

            order.Rating = rating.Rating;
            await db.SaveChangesAsync();
            
            return Ok();
        }

        /// <summary>
        /// Подтвердить заказ
        /// </summary>
        /// <param name="addressId">ID адреса доставки</param>
        /// <response code="200">Заказ подтверждён</response>
        [HttpPost("new/{addressId}")]
        public async Task<IActionResult> ConfirmOrder(long addressId)
        {
            var userCart = db.Carts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Store)
                .Where(c => c.UserLogin == User.Identity.Name)
                .ToList();

            var stores = userCart
                .GroupBy(c => c.Product.Store)
                .Select(s => s.Key);

            foreach (var store in stores)
            {
                var newOrder = new Order
                {
                    UserLogin = User.Identity.Name,
                    StoreId = store.Id,
                    StatusId = db.OrderStatuses.FirstOrDefault(s => s.Name == "Пользователь подал заказ").Id,
                    OrderDate = DateTime.Now,
                    AddressId = addressId
                };

                await db.Orders.AddAsync(newOrder);

                var storeUserCart = userCart.Where(c => c.Product.Store.Id == store.Id);
                foreach (var cart in storeUserCart)
                {
                    var newOrderProduct = new OrderProduct
                    {
                        Order = newOrder,
                        Product = cart.Product,
                        Count = cart.Count
                    };
                    await db.OrderProducts.AddAsync(newOrderProduct);
                }
            }

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Отказаться от заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Заказ отменён</response>
        /// <response code="403">Оценивающий не является заказчиком</response>
        /// <response code="404">Данного заказа нет</response>
        [HttpPost("{id}/denied")]
        public async Task<IActionResult> DeniedOrder([FromRoute] long id)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.UserLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            order.Status = await db.OrderStatuses
                .FirstOrDefaultAsync(s => s.Name == "Пользователь отказался от заказа");

            await db.SaveChangesAsync();
            return Ok();
        }
    }
}