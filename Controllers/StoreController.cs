using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Domain.Store;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    [Route("store")]
    public class StoreController : ControllerBase
    {
        private readonly deliveryContext db;
        public StoreController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetStores(string city, 
            StoresOrder order = StoresOrder.IdAsc, int limit = 10, int offset = 0)
        {
            var stores = db.Stores
                .Where(s => s.Address.Locality.Name == city)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    rating = db.Ratings.Any(r => r.StoreId == s.Id) ? 
                        Math.Round(db.Ratings.Where(r => r.StoreId == s.Id)
                                .Average(r => r.Rating1), 2)
                            .ToString(CultureInfo.CurrentCulture) : null
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStore([FromRoute] long id)
        {
            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id);
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
        
        [HttpGet("{id}/menu")]
        public async Task<IActionResult> GetMenu([FromRoute] long id)
        {
            var response = await db.Products
                .Where(p => p.StoreId == id)
                .Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    price = p.Price,
                    weight = p.Weight
                }).ToListAsync();

            return Ok(response);
        }
        
        [HttpGet("menu/{idMenu}")]
        public async Task<IActionResult> GetMenuItem([FromRoute] long idMenu)
        {
            var storeItem = await db.Products
                .Select(p => new { p, StoreTitle = p.Store.Title })
                .FirstOrDefaultAsync(p => p.p.Id == idMenu);
            var response = new
            {
                id = storeItem.p.Id,
                title = storeItem.p.Title,
                price = storeItem.p.Price,
                weight = storeItem.p.Weight,
                store_title = storeItem.StoreTitle
            };
            
            return Ok(response);
        }

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
    }
}