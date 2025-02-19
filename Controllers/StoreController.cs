using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Cache;
using Delivery.Contracts.Store;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы для информации о предприятиях
    /// </summary>
    [ApiController]
    [Route("api/store")]
    public class StoreController : ControllerBase
    {
        private readonly deliveryContext db;
        public StoreController(deliveryContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Возвращает список предприятий
        /// </summary>
        /// <param name="city">Город</param>
        /// <param name="order">Сортировка</param>
        /// <param name="limit">Количество магазинов</param>
        /// <param name="offset">Смещение</param>
        /// <response code="200">Возвращает список предприятий</response>
        [HttpGet]
        public async Task<IActionResult> GetStores(string city, 
            StoresOrder order = StoresOrder.IdAsc, int limit = 10, int offset = 0)
        {
            var stores = db.Stores
                .Where(s => s.Address.Locality.Name == city
                            && s.StoreStatus.Name == "Подтверждено")
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    rating = db.Ratings.Any(r => r.StoreId == s.Id)
                        ? Math.Round(db.Ratings.Where(r => r.StoreId == s.Id)
                                .Average(r => r.Rating1), 2)
                            .ToString(CultureInfo.CurrentCulture)
                        : null,
                    description = s.Description,
                    status = db.StoreStatuses.FirstOrDefault(ss => ss.Id == s.StoreStatusId).Name
                });

            var response = order switch
            {
                StoresOrder.IdAsc => stores.OrderBy(s => s.id),
                StoresOrder.IdDesc => stores.OrderByDescending(s => s.id),
                StoresOrder.NameAsc => stores.OrderBy(s => s.title),
                StoresOrder.NameDesc => stores.OrderByDescending(s => s.title),
                StoresOrder.RatingAsc => stores.OrderBy(s => s.rating),
                StoresOrder.RatingDesc => stores.OrderByDescending(s => s.rating),
                _ => stores
            };

            await response.Skip(offset).Take(limit).ToListAsync();
            return Ok(response);
        }
        
        /// <summary>
        /// Возвращает информацию о предприятии
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Возвращает список предприятий</response>
        /// <response code="404">Предприятие не найдено</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStore([FromRoute] long id)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id
                                                                 && s.StoreStatus.Name == "Подтверждено");

            if (store == null)
            {
                return NotFound();
            }
            
            var address = await db.Addresses
                .Include(a => a.Locality)
                .FirstOrDefaultAsync(a => a.Id == store.AddressId);
            
            var categories = db.StoreCategories
                .Where(c => c.StoreId == id)
                .Select(c => c.Category.Title);

            var response = new
            {
                id = store.Id,
                title = store.Title,
                rating = db.Ratings.Any(r => r.StoreId == id)
                    ? Math.Round(db.Ratings.Where(r => r.StoreId == id)
                            .Average(r => r.Rating1), 2)
                        .ToString(CultureInfo.CurrentCulture)
                    : null,
                address = new
                {
                    locality = address.Locality.Name,
                    street = address.Street,
                    building_number = address.Building,
                    apartment_number = address.Apartment,
                    entrance = address.Entrance,
                    level = address.Level
                },
                categories = categories.AsEnumerable()
            };

            return Ok(response);
        }
        
        /// <summary>
        /// Возвращает список продукций предприятия
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Возвращает список продукций предприятия</response>
        /// <response code="404">Предприятие не найдено</response>
        [HttpGet("{id}/menu")]
        public async Task<IActionResult> GetMenu([FromRoute] long id)
        {
            var isStoreExist = db.Stores.Any(s => s.Id == id);
            if (!isStoreExist)
            {
                return NotFound();
            }
            
            var response = await db.Products
                .Where(p => p.StoreId == id)
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    price = p.Price,
                    weight = p.Weight,
                    store_id = p.StoreId,
                }).ToListAsync();

            return Ok(response);
        }
        
        /// <summary>
        /// Возвращает информацию о продукции предприятия
        /// </summary>
        /// <param name="id">ID продукции</param>
        /// <response code="200">Возвращает список продукции предприятия</response>
        /// <response code="404">Продукция не найдена</response>
        [HttpGet("menu/{idMenu}")]
        public async Task<IActionResult> GetMenuItem([FromRoute] long idMenu)
        {
            var storeItem = await db.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == idMenu);

            if (storeItem == null)
            {
                return NotFound();
            }
            
            var response = new
            {
                id = storeItem.Id,
                title = storeItem.Title,
                price = storeItem.Price,
                weight = storeItem.Weight,
                store_id = storeItem.StoreId,
                store_title = storeItem.Store.Title
            };
            
            return Ok(response);
        }

        /// <summary>
        /// Оценка предприятия
        /// </summary>
        /// <param name="id">ID предприятия</param>
        /// <response code="200">Заведение успешно оценено</response>
        /// <response code="400">Заведение уже оценено</response>
        /// <response code="401">Пользователь не авторизован</response>
        [Authorize]
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateStore([FromRoute] long id, [FromBody] RateStore rateStore)
        {
            var storeRating = await db.Ratings
                .FirstOrDefaultAsync(r => r.StoreId == id && r.UserLogin == User.Identity.Name);

            if (storeRating != null)
            {
                return BadRequest(new { response = "Заведение уже оценено" });
            }

            var rating = new Rating
            {
                StoreId = id,
                UserLogin = User.Identity?.Name,
                Rating1 = rateStore.Rating
            };
            
            await db.Ratings.AddAsync(rating);
            await db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Поиск продуктов
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <response code="200">Результат поиска</response>
        [HttpGet]
        [Route("search/products")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            var products = db.Products
                .FromSqlRaw(
                    "select * from \"Product\"\n" +
                    $"where \"Product\".\"DocumentWithWeights\" @@ plainto_tsquery('{query}')\n" +
                    $"order by ts_rank(\"Product\".\"DocumentWithWeights\", plainto_tsquery('{query}')) desc"
                )
                .ToList()
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    description = p.Description,
                    price = p.Price,
                    store_title = db.Stores.FirstOrDefault(s => s.Id == p.StoreId)?.Title
                });

            return Ok(products);
        }
        
        /// <summary>
        /// Поиск магазинов
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <response code="200">Результат поиска</response>
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchStores([FromQuery] string query)
        {
            var stores = db.Stores
                .FromSqlRaw(
                    "select * from \"Stores\"\n" +
                    $"where \"Stores\".\"DocumentWithWeights\" @@ plainto_tsquery('{query}')\n" +
                    $"order by ts_rank(\"Stores\".\"DocumentWithWeights\", plainto_tsquery('{query}')) desc"
                )
                .ToList()
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    description = s.Description,
                    categories = db.StoreCategories
                        .Where(sc => sc.StoreId == s.Id)
                        .Select(sc => sc.Category.Title)
                        .ToList()
                });
        
            return Ok(stores);
        }

        /// <summary>
        /// Список категорий
        /// </summary>
        /// <response code="200">Список категорий</response>
        [HttpGet]
        [Route("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = db.CategoryStores
                .Select(c => c.Title);

            return Ok(categories);
        }
        
        /// <summary>
        /// Список кмагазинов по категории
        /// </summary>
        /// <response code="200">Список магазинов по категории</response>
        [HttpGet]
        [Route("category/{category}")]
        public async Task<IActionResult> GetStoresByCategories([FromRoute] string category)
        {
            var stores = db.StoreCategories
                .Include(sc => sc.Store)
                .Include(sc => sc.Category)
                .Where(sc => sc.Category.Title == category)
                .Select(sc => new
                {
                    id = sc.StoreId,
                    title = sc.Store.Title,
                });

            return Ok(stores);
        }

        [HttpPost]
        [Route("search/stores")]
        public async Task<IActionResult> GetStoresByProducts([FromBody] List<string> products)
        {
            var productsEntities = new List<Product>();
            foreach (var product in products)
            {
                var productEntity = db.Products
                    .Where(p => p.Title.ToLower().Contains(product.ToLower()));
                productsEntities.AddRange(productEntity);
            }

            var stores = productsEntities.GroupBy(p => p.StoreId).Select(g => new
            {
                store_id = g.Key,
                store_title = db.Stores.FirstOrDefault(s => s.Id == g.Key)?.Title,
                count = g.Count(),
            })
                .OrderBy(s => s.count);


            return Ok(stores);
        }
    }
}