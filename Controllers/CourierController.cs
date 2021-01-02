using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Dto.Courier;
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
            await db.SaveChangesAsync();

            return Created(new Uri($"/courier/{login}"), new {});
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> ChangeCourier([FromRoute] string login, [FromBody] CourierRequest courierRequest)
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteCourier([FromRoute] string login, [FromBody] CourierRequest courierRequest)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{login}/work")]
        public async Task<IActionResult> GetWorkStatus([FromRoute] string login)
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("locality/{title}")]
        public async Task<IActionResult> GetOrdersInLocality([FromRoute] string title, [FromRoute] string order)
        {
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