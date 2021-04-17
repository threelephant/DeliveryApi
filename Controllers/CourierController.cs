using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts.Account;
using Delivery.Contracts.Courier;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы курьера
    /// </summary>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Пользователь не является курьером</response>
    [Authorize]
    [ApiController]
    [Route("api/courier")]
    public class CourierController : ControllerBase
    {
        private readonly deliveryContext db;
        public CourierController(deliveryContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Информация о курьере
        /// </summary>
        /// <param name="login">Логин курьера</param>
        /// <response code="200">Информация о курьере</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
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

        /// <summary>
        /// Заявка на работу курьером
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="courierRequest">Параметры заявки</param>
        /// <response code="200">Заявка успешно подана</response>
        /// <response code="400">Заявка уже подана</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpPost("{login}")]
        public async Task<IActionResult> AddCourier([FromRoute] string login, [FromBody] CourierRequest courierRequest)
        {
            var oldCourier = await db.Couriers.FirstOrDefaultAsync(c => c.UserLogin == login);

            if (oldCourier != null)
            {
                if (oldCourier.WorkStatusId == db.WorkCourierStatuses
                    .FirstOrDefault(s => s.Name == "Одобрено")?.Id)
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

        /// <summary>
        /// Меняет параметры курера
        /// </summary>
        /// <param name="login">Логин курьера</param>
        /// <param name="request">Параметры запроса</param>
        /// <response code="200">Параметры успешно изменены</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        /// <response code="404">Курьера с таким логином не существует</response>
        [AuthorizeByLogin]
        [AuthorizeCourier]
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
        
        /// <summary>
        /// Увольнение курьера
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="204">Увольнение прошло успешно</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        /// <response code="404">Курьера с таким логином не существует</response>
        [AuthorizeByLogin]
        [AuthorizeCourier]
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

        /// <summary>
        /// Статус курьера
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Статус курьера</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        /// <response code="404">Курьера с таким логином не существует</response>
        [AuthorizeByLogin]
        [AuthorizeCourier]
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
        
        /// <summary>
        /// Список заказов-кандидатов для курьера
        /// </summary>
        /// <param name="title">Название города</param>
        /// <response code="200">Список заказов-кандидатов для курьера</response>
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
        
        /// <summary>
        /// Список текущих заказов для курьера
        /// </summary>
        /// <response code="200">Список текущих заказов для курьера</response>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentOrders()
        {
            var orders = await db.Orders
                .Where(o => o.CourierLogin == User.Identity.Name
                            && (o.Status.Name == "Заказ готов"
                            || o.Status.Name == "Курьер принял заказ"
                            || o.Status.Name == "Курьер взял заказ"))
                .Select(o => new
                {
                    id = o.Id,
                    is_after = o.Status.Name == "Курьер взял заказ",
                    address = $"{o.Store.Address.Street}, {o.Store.Address.Building}",
                    info = $"Отправитель: {o.Store.Title}, Заказчик: {o.UserLoginNavigation.FirstName}",
                    status = o.Status.Name,
                })
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Список старых заказов для курьера
        /// </summary>
        /// <response code="200">Список текущих заказов для курьера</response>
        [HttpGet("old")]
        public async Task<IActionResult> GetOldOrders()
        {
            var orders = await db.Orders
                .Where(o => o.CourierLogin == User.Identity.Name
                            && (o.Status.Name == "Пользователь отказался от заказа"
                            || o.Status.Name == "Заказ доставлен"
                            || o.Status.Name == "Курьер отказался от заказа"
                            || o.Status.Name == "Предприятие отказалось от заказа"))
                .Select(o => new
                {
                    id = o.Id,
                    info = $"Заказ от {o.OrderDate}",
                    status = o.Status.Name
                })
                .ToListAsync();

            return Ok(orders);
        }
        
        //TODO: обработать исключения
        /// <summary>
        /// Информация о новом заказе
        /// </summary>
        /// <response code="200">Информация о новом заказе</response>
        [HttpGet("new/{id}")]
        public async Task<IActionResult> GetNewOrder(long id)
        {
            var order = await db.Orders
                .Include(o => o.Store)
                    .ThenInclude(s => s.Address)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            return Ok(new
            {
                id = order.Id,
                address = $"{order.Store.Address.Street}, {order.Store.Address.Building}",
                title = order.Store.Title,
                weight = order.OrderProducts.Sum(op => op.Product.Weight * op.Count),
            });
        }
        
        //TODO: обработать исключения
        /// <summary>
        /// Информация о текущем заказе
        /// </summary>
        /// <response code="200">Информация о текущем заказе</response>
        [HttpGet("current/{id}")]
        public async Task<IActionResult> GetCurrentOrder(long id)
        {
            var order = await db.Orders
                .Include(o => o.Status)
                .Include(o => o.UserLoginNavigation)
                .Include(o => o.Address)
                .Include(o => o.Store)
                .ThenInclude(s => s.Address)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return Ok(new
            {
                id = order.Id,
                time = order.TakingDate,
                status = order.Status.Name,
                price = order.OrderProducts.Sum(op => op.Count * op.Product.Price),
                weight = order.OrderProducts.Sum(op => op.Product.Weight * op.Count),
                customer = order.UserLoginNavigation.FirstName,
                customer_address = $"{order.Address.Street}, {order.Address.Building}, кв. {order.Address.Apartment}",
                store = order.Store.Title,
                store_address = $"{order.Store.Address.Street}, {order.Store.Address.Building}",
                products = order.OrderProducts.Select(op => new
                {
                    title = op.Product.Title,
                    count = op.Count,
                    weight = op.Product.Weight * op.Count,
                }),
            });
        }
        
        //TODO: обработать исключения
        /// <summary>
        /// Информация о текущем заказе
        /// </summary>
        /// <response code="200">Информация о текущем заказе</response>
        [HttpGet("old/{id}")]
        public async Task<IActionResult> GetOldOrder(long id)
        {
            var order = await db.Orders
                .Include(o => o.Status)
                .Include(o => o.UserLoginNavigation)
                .Include(o => o.Address)
                .Include(o => o.Store)
                    .ThenInclude(s => s.Address)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            return Ok(new
            {
                id = order.Id,
                time = order.DeliveryDate,
                status = order.Status.Name,
                price = order.OrderProducts.Sum(op => op.Count * op.Product.Price),
                weight = order.OrderProducts.Sum(op => op.Product.Weight * op.Count),
                customer = order.UserLoginNavigation.FirstName,
                customer_address = $"{order.Address.Street}, {order.Address.Building}",
                store = order.Store.Title,
                store_address = $"{order.Store.Address.Street}, {order.Store.Address.Building}",
                products = order.OrderProducts.Select(op => new
                {
                    title = op.Product.Title,
                    count = op.Count,
                    weight = op.Product.Weight * op.Count,
                }),
            });
        }

        /// <summary>
        /// Принять заказ
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Заказ принят</response>
        /// <response code="404">Такого заказа не существует</response>
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

            order.CourierLogin = User.Identity?.Name;
            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Курьер принял заказ");

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Уведомить о взятие заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Заказ взят</response>
        /// <response code="404">Такого заказа не существует</response>
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
        
        /// <summary>
        /// Уведомить о доставке клиенту заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Заказ доставлен</response>
        /// <response code="404">Такого заказа не существует</response>
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
        
        /// <summary>
        /// Отказаться от заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Отказ успешно оформлен</response>
        /// <response code="404">Такого заказа не существует</response>
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