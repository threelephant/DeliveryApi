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
            // var orders = db.Orders
            //     .Include(o => o.Store)
            //     .ThenInclude(s => s.Address)
            //     .ThenInclude(a => a.Locality)
            //     .Include(o => o.UserLoginNavigation)
            //     .ThenInclude(u => u.UserAddresses)
            //     .ThenInclude(a => a.Address)
            //     .Where(o => o.Store.Address.Locality.Name == title)
            //     .Select(o => new
            //     {
            //         customer = new
            //         {
            //             first_name = o.UserLoginNavigation.FirstName,
            //             address = new
            //             {
            //                 o.UserLoginNavigation.UserAddresses
            //             }
            //         }
            //     });
            
            throw new NotImplementedException();
        }
        
        [HttpGet("order/{id}")]
        public async Task<IActionResult> GetOrderInfo([FromRoute] long id)
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