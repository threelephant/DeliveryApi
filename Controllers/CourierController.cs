using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Domain.Courier;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    [Route("courier")]
    public class CourierController : ControllerBase
    {
        private readonly deliveryContext db;
        public CourierController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet("{login}")]
        public async Task<IActionResult> GetCourier([FromRoute] string login)
        {
            var userInfo = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            var courierInfo = await db.Couriers.FirstOrDefaultAsync(c => c.UserLogin == login);
            var successOrderCount = await db.Orders.CountAsync(o => o.Status.Name == "Заказ доставлен");
            
            var result = new CourierGet
            {
                first_name = userInfo.FirstName,
                middle_name = userInfo.MiddleName,
                last_name = userInfo.LastName,
                phone = userInfo.Phone,
                passport = new Passport
                {
                    citizenship = courierInfo.Citizenship,
                    number = courierInfo.PassportNumber,
                    birth = courierInfo.Birth.Date,
                },
                date_begin = courierInfo.DateWorkBegin.Date,
                payroll = courierInfo.Payroll.ToString(CultureInfo.CurrentCulture),
                success_order_count = successOrderCount
            };

            return Ok(result);
        }

        [HttpPost("{login}")]
        public async Task<IActionResult> AddCourier([FromRoute] string login, [FromBody] CourierRequest courierRequest)
        {
            var oldCourier = await db.Couriers.FirstOrDefaultAsync(c => c.UserLogin == login);

            if (oldCourier != null)
            {
                if (oldCourier.WorkStatusId == db.WorkCourierStatuses.FirstOrDefault(s => s.Name == "Одобрено").Id)
                {
                    return BadRequest();
                }
                
                oldCourier.WorkStatus = db.WorkCourierStatuses
                    .FirstOrDefault(s => s.Name == "Рассматривается");
            }
            else
            {
                var courier = new Courier
                {
                    UserLogin = login,
                    DateWorkBegin = courierRequest.date_begin,
                    Citizenship = courierRequest.citizenship,
                    PassportNumber = courierRequest.number,
                    Birth = courierRequest.birth,
                    WorkStatus = db.WorkCourierStatuses.FirstOrDefault(s => s.Name == "Рассматривается")
                };

                await db.Couriers.AddAsync(courier);
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> ChangeCourier([FromRoute] string login, [FromBody] CourierChangeRequest request)
        {
            var courier = await db.Couriers.FirstOrDefaultAsync(c => c.UserLogin == login);

            if (courier == null)
            {
                return NotFound();
            }
            
            courier.Citizenship = request.citizenship;
            courier.Birth = request.birth;
            courier.PassportNumber = request.number;
            
            await db.SaveChangesAsync();
            return Ok();
        }
        
        [HttpDelete("{login}")]
        public async Task<IActionResult> QuitCourier([FromRoute] string login)
        {
            var courier = await db.Couriers.FirstOrDefaultAsync(c => c.UserLogin == login);

            if (courier == null)
            {
                return NotFound();
            }

            courier.WorkStatus = await db.WorkCourierStatuses
                .FirstOrDefaultAsync(s => s.Name == "Увольнение");
            
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{login}/work")]
        public async Task<IActionResult> GetWorkStatus([FromRoute] string login)
        {
            var courier = await db.Couriers
                .Include(c => c.WorkStatus)
                .FirstOrDefaultAsync(c => c.UserLogin == login);

            if (courier == null)
            {
                return NotFound();
            }

            var response = new
            {
                status = courier.WorkStatus.Name
            };
            
            return Ok(response);
        }
        
        [HttpGet("locality/{title}")]
        public async Task<IActionResult> GetOrdersInLocality([FromRoute] string title)
        {
            var orders = await db.Orders
                .Where(o => o.Address.Locality.Name == title
                            && o.CourierLogin == null
                && o.Status == db.OrderStatuses
                    .FirstOrDefault(s => s.Name == "Предприятие приняло заказ"))
                .Select(o => new
                {
                    id = o.Id,
                    sum = db.OrderProducts
                        .Include(p => p.Product)
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => new
                        {
                            cost = p.Count * (double)Convert.ToDecimal(p.Product.Price)
                        })
                        .Sum(p => p.cost),
                    customer = new
                    {
                        first_name = o.UserLoginNavigation.FirstName,
                        address = new
                        {
                            street = o.Address.Street,
                            building_number = o.Address.Building,
                            apartment_number = o.Address.Apartment,
                            entrance = o.Address.Entrance,
                            level = o.Address.Level
                        }
                    },
                    store = new
                    {
                        title = o.Store.Title,
                        address = new
                        {
                            street = o.Store.Address.Street,
                            building_number = o.Store.Address.Building,
                            apartment_number = o.Store.Address.Apartment,
                            entrance = o.Store.Address.Entrance,
                            level = o.Store.Address.Level
                        }
                    }
                })
                .ToListAsync();

            return Ok(orders);
        }
        
        [HttpPost("order/{id}/accept")]
        public async Task<IActionResult> AcceptOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.Status.Name == "Предприятие приняло заказ"
                    && o.CourierLogin == null)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            order.CourierLogin = User.Identity.Name;
            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Курьер принял заказ");

            await db.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPost("order/{id}/take")]
        public async Task<IActionResult> TakeOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.Status.Name == "Заказ готов"
                    && o.CourierLogin == User.Identity.Name)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Курьер взял заказ");
            order.TakingDate = DateTime.Now;

            await db.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPost("order/{id}/deliver")]
        public async Task<IActionResult> DeliverOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.Status.Name == "Курьер взял заказ"
                    && o.CourierLogin == User.Identity.Name)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Заказ доставлен");
            order.DeliveryDate = DateTime.Now;
            
            await db.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPost("order/{id}/deny")]
        public async Task<IActionResult> DenyOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.CourierLogin == User.Identity.Name
                && o.Status.Name == "Курьер принял заказ"
                || o.Status.Name == "Заказ готов"
                || o.Status.Name == "Курьер взял заказ")
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Курьер отказался от заказа");

            await db.SaveChangesAsync();
            return Ok();
        }
        
    }
}