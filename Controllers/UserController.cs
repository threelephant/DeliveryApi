using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly deliveryContext db;
        public UserController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet("{login}")]
        public async Task<IActionResult> GetUser(string login)
        {
            if (login != User.Identity?.Name)
            {
                return Unauthorized();
            }

            var userAddresses = from user in db.Users
                join userAddress in db.UserAddresses on user.Login equals userAddress.UserLogin
                join address in db.Addresses on userAddress.AddressId equals address.Id
                join building in db.Buildings on address.BuildingId equals building.Id
                join street in db.Streets on building.StreetId equals street.Id
                join streetType in db.StreetTypes on street.StreetTypeId equals streetType.Id
                join locality in db.Localities on street.LocalityId equals locality.Id
                where user.Login == login
                select new
                {
                    locality = locality.Name,
                    street = new
                    {
                        type = streetType.Type,
                        name = street.Name
                    },
                    building_number = building.BuildingNumber,
                    apartment_number = address.ApartmentNumber
                };

            var currentUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);

            var response = new
            {
                name = new
                {
                    first_name = currentUser.FirstName,
                    last_name = currentUser.LastName,
                    middle_name = currentUser.MiddleName
                },
                phone = currentUser.Phone,
                addresses = await userAddresses.ToListAsync()
            };

            return Ok(response);
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> ChangeUser(string login)
        {
            throw new NotImplementedException();
        }
    }
}