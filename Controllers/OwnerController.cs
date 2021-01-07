using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts.Account;
using Delivery.Contracts.Owner;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы для владельца предприятия
    /// </summary>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Пользователь не является владельцем каких-либо предприятий</response>
    [Authorize]
    [AuthorizeOwner]
    [ApiController]
    [Route("api/owner")]
    public class OwnerController : ControllerBase
    {
        private readonly deliveryContext db;

        public OwnerController(deliveryContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Список заведений пользователя
        /// </summary>
        /// <response code="200">Список заведений пользователя</response>
        [HttpGet("store")]
        public async Task<IActionResult> GetOwnerStores()
        {
            var stores = db.Stores
                .Where(s => s.OwnerLogin == User.Identity.Name)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    rating = db.Ratings.Any(r => r.StoreId == s.Id) ? 
                        Math.Round(db.Ratings.Where(r => r.StoreId == s.Id)
                                .Average(r => r.Rating1), 2)
                            .ToString(CultureInfo.CurrentCulture) : null
                });

            return Ok(stores);
        }
        
        //TODO: Think about statistics
        /// <summary>
        /// Возвращает информацию о предприятии
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Возвращает информацию о предприятии</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного предприятия не существует</response>
        [HttpGet("store/{id}")]
        public async Task<IActionResult> GetOwnerStore([FromRoute] long id)
        {
            var store = await db.Stores
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (store == null)
            {
                return NotFound();
            }
            
            if (store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }
            
            var address = await db.Addresses
                .Select(a => new { a, Locality = a.Locality.Name })
                .FirstOrDefaultAsync(a => a.a.Id == id);
            var categories = db.StoreCategories
                .Where(c => c.StoreId == id)
                .Select(c => c.Category.Title);
            
            var response = new
            {
                id = store.Id,
                title = store.Title,
                rating = db.Ratings.Any(r => r.StoreId == id) ? 
                    Math.Round(db.Ratings.Where(r => r.StoreId == id)
                            .Average(r => r.Rating1), 2)
                        .ToString(CultureInfo.CurrentCulture) : null,
                completed_orders = db.Orders.Count(o => o.StoreId == id 
                                                        && o.Status.Name == "Заказ доставлен"),
                address = new
                {
                    locality = address.Locality,
                    street = address.a.Street,
                    building_number = address.a.Building,
                    apartment_number = address.a.Apartment,
                    entrance = address.a.Entrance,
                    level = address.a.Level
                },
                categories = categories.AsEnumerable()
            };

            return Ok(response);
        }
        
        /// <summary>
        /// Выводит заказы предприятия
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <param name="order">Сортировка</param>
        /// <response code="200">Возвращает информацию о предприятии</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного предприятия не существует</response>
        [HttpGet("store/{id}/orders")]
        public async Task<IActionResult> GetOwnerOrders([FromRoute] long id, [FromRoute] string order)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }
            
            var orders = db.Orders
                .Where(o => o.StoreId == id)
                .Select(o => new
                {
                    id = o.Id,
                    first_name = o.UserLoginNavigation.FirstName,
                    sum = db.OrderProducts
                        .Include(p => p.Product)
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => new
                        {
                            cost = p.Count * (double)Convert.ToDecimal(p.Product.Price)
                        })
                        .Sum(p => p.cost),
                    date_order = o.OrderDate,
                    date_taking = o.TakingDate,
                    date_delivery = o.DeliveryDate,
                    products = db.OrderProducts
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => new
                        {
                            id = p.ProductId,
                            title = p.Product.Title,
                            price = p.Product.Price,
                            weight = p.Product.Weight,
                            count = p.Count
                        }).AsEnumerable()
                });

            if ((await orders.ToListAsync()).Count == 0)
            {
                return NotFound();
            }
            
            Enum.TryParse(order, out OwnerOrdersOrder ownerOrder);
            switch (ownerOrder)
            {
                case OwnerOrdersOrder.NameAsc:
                    orders.OrderBy(o => o.first_name);
                    break;
                case OwnerOrdersOrder.NameDesc:
                    orders.OrderByDescending(o => o.first_name);
                    break;
                case OwnerOrdersOrder.DateOrderAsc:
                    orders.OrderBy(o => o.date_order);
                    break;
                case OwnerOrdersOrder.DateOrderDesc:
                    orders.OrderByDescending(o => o.date_order);
                    break;
                case OwnerOrdersOrder.DateTakingAsc:
                    orders.OrderBy(o => o.date_taking);
                    break;
                case OwnerOrdersOrder.DateTakingDesc:
                    orders.OrderByDescending(o => o.date_taking);
                    break;
                case OwnerOrdersOrder.DateDeliveryAsc:
                    orders.OrderBy(o => o.date_delivery);
                    break;
                case OwnerOrdersOrder.DateDeliveryDesc:
                    orders.OrderByDescending(o => o.date_delivery);
                    break;
            }

            return Ok(orders);
        }
        
        /// <summary>
        /// Добавляет предприятие
        /// </summary>
        /// <param name="request">Параметры добавляемого предприятия</param>
        /// <response code="200">Добавляет предприятие</response>
        [HttpPost("store")]
        public async Task<IActionResult> AddStore([FromBody] StoreRequest request)
        {
            var address = await db.Addresses
                .Include(a => a.Locality)
                .FirstOrDefaultAsync(a => a.Locality.Name == request.address.locality
                                          && a.Street == request.address.street
                                          && a.Building == request.address.building
                                          && a.Apartment == request.address.apartment
                                          && a.Entrance == request.address.entrance
                                          && a.Level == request.address.level);

            if (address == null)
            {
                address = new Address
                {
                    Locality = db.Localities.FirstOrDefault(l => l.Name == request.address.locality),
                    Street = request.address.street,
                    Building = request.address.building,
                    Apartment = request.address.apartment,
                    Entrance = request.address.entrance,
                    Level = request.address.level
                };
                await db.AddAsync(address);
                await db.SaveChangesAsync();
            }

            var categories = db.CategoryStores
                .Where(cs => request.categories.Contains(cs.Title))
                .Select(c => c.Id);
            
            var store = new Store
            {
                Title = request.title,
                BeginWorking = request.working_hours.begin,
                EndWorking = request.working_hours.end,
                Address = address,
                StoreStatus = db.StoreStatuses.FirstOrDefault(s => s.Name == "Рассматривается"),
                OwnerLogin = User.Identity?.Name
            };

            await db.Stores.AddAsync(store);
            await db.SaveChangesAsync();

            foreach (var categoryId in categories)
            {
                await db.StoreCategories.AddAsync(new StoreCategory
                {
                    StoreId = store.Id,
                    CategoryId = categoryId
                });
            }

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Изменяет параметры предприятия
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <param name="request">Параметры изменяемого предприятия</param>
        /// <response code="200">Успешное изменение</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного предприятия не существует</response>
        [HttpPut("store/{id}")]
        public async Task<IActionResult> ChangeStore([FromRoute] long id, [FromBody] StoreRequest request)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null)
            {
                return NotFound();
            }

            if (store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }
            
            var address = await db.Addresses
                .Include(a => a.Locality)
                .FirstOrDefaultAsync(a => a.Locality.Name == request.address.locality
                                          && a.Street == request.address.street
                                          && a.Building == request.address.building
                                          && a.Apartment == request.address.apartment
                                          && a.Entrance == request.address.entrance
                                          && a.Level == request.address.level);

            if (address == null)
            {
                address = new Address
                {
                    Locality = db.Localities.FirstOrDefault(l => l.Name == request.address.locality),
                    Street = request.address.street,
                    Building = request.address.building,
                    Apartment = request.address.apartment,
                    Entrance = request.address.entrance,
                    Level = request.address.level
                };
                await db.AddAsync(address);
                await db.SaveChangesAsync();
            }

            var categories = db.CategoryStores
                .Where(cs => request.categories.Contains(cs.Title))
                .Select(c => c.Id);

            var currentCategories = db.StoreCategories
                .Where(cs => cs.StoreId == id)
                .Select(cs => cs.CategoryId);
            
            var newCategories = categories.Where(c => 
                !currentCategories.Contains(c));
            var oldCategories = currentCategories.Where(c =>
                !categories.Contains(c));

            store.Title = request.title;
            store.BeginWorking = request.working_hours.begin;
            store.EndWorking = request.working_hours.end;
            store.Address = address;

            db.StoreCategories.RemoveRange(oldCategories.Select(c => new StoreCategory
            {
                StoreId = id,
                CategoryId = c
            }));
            
            await db.StoreCategories.AddRangeAsync(
                newCategories.Select(c => new StoreCategory
            {
                StoreId = id,
                CategoryId = c
            }));

            await db.SaveChangesAsync();

            return Ok();
        }
        
        /// <summary>
        /// Удаляет предприятие
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="204">Успешное удаление</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного предприятия не существует</response>
        [HttpDelete("store/{id}")]
        public async Task<IActionResult> DeleteStore([FromRoute] long id)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
            {
                return NotFound();
            }

            if (store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            db.Stores.Remove(store);
            await db.SaveChangesAsync();

            return NoContent();
        }
        
        /// <summary>
        /// Добавляет продукт предприятия
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Успешное добавление</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного предприятия не существует</response>
        [HttpPost("store/{id}/menu")]
        public async Task<IActionResult> AddStoreItem([FromRoute] long id, [FromBody] ProductRequest request)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
            {
                return NotFound();
            }

            if (store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }
            
            var product = new Product
            {
                Title = request.title,
                Price = Convert.ToDecimal(request.price),
                Weight = request.weight,
                StoreId = id
            };

            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();

            return Ok();
        }
        
        /// <summary>
        /// Изменяет продукт предприятия
        /// </summary>
        /// <param name="id">ID продукта</param>
        /// <response code="200">Успешное изменение</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного продукта не существует</response>
        [HttpPut("store/menu/{id}")]
        public async Task<IActionResult> ChangeStoreItem([FromRoute] long id, [FromBody] ProductRequest request)
        {
            var product = await db.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            if (product.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            product.Title = request.title;
            product.Price = Convert.ToDecimal(request.price);
            product.Weight = request.weight;

            await db.SaveChangesAsync();

            return Ok();
        }
        
        /// <summary>
        /// Удаляет продукт предприятия
        /// </summary>
        /// <param name="id">ID продукта</param>
        /// <response code="204">Успешное удаление</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного продукта не существует</response>
        [HttpDelete("store/menu/{id}")]
        public async Task<IActionResult> DeleteStoreItem([FromRoute] long id)
        {
            var product = await db.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            
            return NoContent();
        }
        
        /// <summary>
        /// Принимает заказ клиента
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Принимает заказ клиента</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного заказа не существует</response>
        [HttpPost("order/{id}/accept")]
        public async Task<IActionResult> AcceptOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.Status.Name == "Пользователь подал заказ")
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            if (order.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Предприятие приняло заказ");

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Уведомляет о готовности заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Уведомляет о готовности заказа</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного заказа не существует</response>
        [HttpPost("order/{id}/ready")]
        public async Task<IActionResult> ReadyOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.Status.Name == "Курьер принял заказ")
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            if (order.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Заказ готов");

            await db.SaveChangesAsync();
            return Ok();
        }
        
        /// <summary>
        /// Отказаться от заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <response code="200">Отказаться от заказа</response>
        /// <response code="403">Пользователь не является владельцем данного предприятия</response>
        /// <response code="404">Данного заказа не существует</response>
        [HttpPost("order/{id}/deny")]
        public async Task<IActionResult> DenyOrder([FromRoute] long id)
        {
            var order = await db.Orders
                .Where(o => o.CourierLogin == User.Identity.Name
                            && o.Status.Name == "Курьер принял заказ"
                            || o.Status.Name == "Заказ готов"
                            || o.Status.Name == "Предприятие приняло заказ")
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            if (order.Store.OwnerLogin != User.Identity?.Name)
            {
                return Forbid();
            }

            order.Status = await db.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Предприятие отказалось от заказа");

            await db.SaveChangesAsync();
            return Ok();
        }
    }
}