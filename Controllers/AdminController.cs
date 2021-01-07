using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts.User;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы администратора
    /// </summary>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Пользователь не является администратором</response>
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly deliveryContext db;

        public AdminController(deliveryContext db)
        {
            this.db = db;
        }
        
        /// <summary>
        /// Список пользователей по параметрам
        /// </summary>
        /// <param name="city">Город</param>
        /// <param name="order">Сортировка</param>
        /// <param name="limit">Количество пользоателей</param>
        /// <param name="offset">Смещение</param>
        /// <response code="200">Список пользователей по параметрам</response>
        [HttpGet("user")]
        public async Task<IActionResult> GetUsers(string city, string order = "loginAsc", 
            int limit = 10, int offset = 0)
        {
            var users = await db.Users
                .Skip(offset)
                .Take(limit)
                .Select(u => new
                {
                    login = u.Login,
                    first_name = u.FirstName,
                    last_name = u.LastName,
                    middle_name = u.MiddleName,
                    phone = u.Phone
                })
                .ToListAsync();

            if (!String.IsNullOrWhiteSpace(city))
            {
                users = (from user in users
                    join userAddress in db.UserAddresses on user.login equals userAddress.UserLogin
                    join address in db.Addresses on userAddress.AddressId equals address.Id
                    join locality in db.Localities on address.LocalityId equals locality.Id
                    where locality.Name == city
                    select user).ToList();
            }

            Enum.TryParse(order, out UserOrder userOrder);
            var response = userOrder switch
            {
                UserOrder.loginAsc => users.OrderBy(u => u.login),
                UserOrder.loginDesc => users.OrderByDescending(u => u.login),
                UserOrder.nameAsc => users.OrderBy(u => u.last_name)
                    .ThenBy(u => u.first_name)
                    .ThenBy(u => u.middle_name),
                UserOrder.nameDesc => users.OrderByDescending(u => u.last_name)
                    .ThenByDescending(u => u.first_name)
                    .ThenByDescending(u => u.middle_name)
            };
            
            return Ok(response);
        }
        
        /// <summary>
        /// Список курьеров-кандидатов
        /// </summary>
        /// <response code="200">Список курьеров-кандидатов</response>
        [HttpGet("courier/candidate")]
        public async Task<IActionResult> GetCouriersCandidate()
        {
            var couriers = db.Couriers
                .Include(c => c.UserLoginNavigation)
                .Include(c => c.WorkStatus)
                .Where(c => c.WorkStatus.Name == "Рассматривается")
                .Select(c => new
                {
                    login = c.UserLogin,
                    first_name = c.UserLoginNavigation.FirstName,
                    last_name = c.UserLoginNavigation.LastName,
                    middle_name = c.UserLoginNavigation.MiddleName,
                    date_begin = c.Birth,
                    passport = new
                    {
                        citizenship = c.Citizenship,
                        number = c.PassportNumber,
                        birth = c.Birth
                    }
                });

            return Ok(couriers);
        }
        
        /// <summary>
        /// Список предприятий-кандидатов
        /// </summary>
        /// <param name="city">Город</param>
        /// <param name="order">Сортировка</param>
        /// <response code="200">Список предприятий-кандидатов</response>
        [HttpGet("store/candidate")]
        public async Task<IActionResult> GetStoresCandidate([FromRoute] string city, string order)
        {
            var stores = db.Stores
                .Include(s => s.Address)
                .ThenInclude(a => a.Locality)
                .Include(s => s.StoreStatus)
                .Where(s => s.StoreStatus.Name == "Рассматривается")
                .Select(s => new
                {
                    id = s.Id,
                    login = s.OwnerLogin,
                    title = s.Title,
                    address = new
                    {
                        locality = s.Address.Locality.Name,
                        street = s.Address.Street,
                        building = s.Address.Building,
                        apartment = s.Address.Apartment,
                        entrance = s.Address.Entrance,
                        level = s.Address.Level
                    },
                    categories = db.StoreCategories
                        .Include(sc => sc.Category)
                        .Where(sc => sc.StoreId == s.Id)
                        .Select(sc => sc.Category.Title).AsEnumerable(),
                    working_hours = new
                    {
                        begin = s.BeginWorking.ToString(),
                        end = s.EndWorking.ToString()
                    }
                });

            return Ok(stores);
        }
        
        /// <summary>
        /// Принять курьера на работу
        /// </summary>
        /// <param name="login">Логин курьера</param>
        /// <response code="200">Курьер принят на работу</response>
        /// <response code="404">Кандидат не найден</response>
        [HttpPost("courier/{login}/accept")]
        public async Task<IActionResult> AcceptCourier([FromRoute] string login)
        {
            var courier = await db.Couriers
                .Where(s => s.WorkStatus.Name == "Рассматривается")
                .FirstOrDefaultAsync(c => c.UserLogin == login);
            
            if (courier == null)
            {
                return NotFound();
            }

            courier.WorkStatus = await db.WorkCourierStatuses
                .FirstOrDefaultAsync(s => s.Name == "Одобрено");

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Отклонить заявку курьера на работу
        /// </summary>
        /// <param name="login">Логин курьера</param>
        /// <response code="200">Курьер не принят на работу</response>
        /// <response code="404">Кандидат не найден</response>
        [HttpPost("courier/{login}/deny")]
        public async Task<IActionResult> DenyCourier([FromRoute] string login)
        {
            var courier = await db.Couriers
                .Where(s => s.WorkStatus.Name == "Рассматривается")
                .FirstOrDefaultAsync(c => c.UserLogin == login);
            
            if (courier == null)
            {
                return NotFound();
            }

            courier.WorkStatus = await db.WorkCourierStatuses
                .FirstOrDefaultAsync(s => s.Name == "Отклонено");
            
            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Принять предприятие
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Предприятие принято</response>
        /// <response code="404">Предприятие не найдено</response>
        [HttpPost("store/{id}/accept")]
        public async Task<IActionResult> AcceptStore([FromRoute] long id)
        {
            var store = await db.Stores
                .Where(s => s.StoreStatus.Name == "Рассматривается")
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (store == null)
            {
                return NotFound();
            }

            store.StoreStatus = await db.StoreStatuses
                .FirstOrDefaultAsync(s => s.Name == "Подтверждено");
            
            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Не принять предприятие
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Предприятие не принято</response>
        /// <response code="404">Предприятие не найдено</response>
        [HttpPost("store/{id}/deny")]
        public async Task<IActionResult> DenyStore([FromRoute] long id)
        {
            var store = await db.Stores
                .Where(s => s.StoreStatus.Name == "Рассматривается")
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (store == null)
            {
                return NotFound();
            }

            store.StoreStatus = await db.StoreStatuses
                .FirstOrDefaultAsync(s => s.Name == "Отклонено");
            
            await db.SaveChangesAsync();
            return Ok();
        }
    }
}