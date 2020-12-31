using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Dto;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers
{
    // [Authorize]
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
            // if (login != User.Identity?.Name)
            // {
            //     return Unauthorized();
            // }

            var userAddresses = from user in db.Users
                join userAddress in db.UserAddresses on user.Login equals userAddress.UserLogin
                join address in db.Addresses on userAddress.AddressId equals address.Id
                join locality in db.Localities on address.LocalityId equals locality.Id
                where user.Login == login
                select new UserAddresses
                {
                    locality = locality.Name,
                    street = address.Street,
                    building =  address.Building,
                    apartment = address.Apartment,
                    entrance = address.Entrance,
                    level = address.Level
                };

            var currentUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);

            var response = new UserInfo
            {
                name = new UserName
                {
                    first_name = currentUser.FirstName,
                    last_name = currentUser.LastName,
                    middle_name = currentUser.MiddleName
                },
                phone = currentUser.Phone,
                addresses = await userAddresses.OrderBy(ua => ua.locality)
                    .ThenBy(ua => ua.street)
                    .ThenBy(ua => ua.building)
                    .ThenBy(ua => ua.apartment)
                    .ThenBy(ua => ua.entrance)
                    .ThenBy(ua => ua.level)
                    .ToListAsync()
            };

            return Ok(response);
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> ChangeUser([FromRoute] string login, [FromBody] UserInfo userInfo)
        {
            // if (login != User.Identity?.Name)
            // {
            //     return Unauthorized();
            // }
            
            var changedUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            changedUser.FirstName = userInfo.name.first_name;
            changedUser.LastName = userInfo.name.last_name;
            changedUser.MiddleName = userInfo.name.middle_name;
            changedUser.Phone = userInfo.phone;

            await db.SaveChangesAsync();

            var addresses = userInfo.addresses
                .OrderBy(ua => ua.locality)
                .ThenBy(ua => ua.street)
                .ThenBy(ua => ua.building)
                .ThenBy(ua => ua.apartment)
                .ThenBy(ua => ua.entrance)
                .ThenBy(ua => ua.level)
                .ToList();
            
            var currentAddresses = await (from user in db.Users
                join userAddress in db.UserAddresses on user.Login equals userAddress.UserLogin
                join address in db.Addresses on userAddress.AddressId equals address.Id
                join locality in db.Localities on address.LocalityId equals locality.Id
                where user.Login == login
                select new UserAddresses
                {
                    locality = locality.Name,
                    street = address.Street,
                    building =  address.Building,
                    apartment = address.Apartment,
                    entrance = address.Entrance,
                    level = address.Level
                })
                .OrderBy(ua => ua.locality)
                .ThenBy(ua => ua.street)
                .ThenBy(ua => ua.building)
                .ThenBy(ua => ua.apartment)
                .ThenBy(ua => ua.entrance)
                .ThenBy(ua => ua.level)
                .ToListAsync();

            if (addresses.SequenceEqual(currentAddresses))
            {
                return Ok(new { message = "Same addresses" });
            }

            // var intersectAddresses = addresses.Intersect(currentAddresses);
            // var unionAddresses = addresses.Union(currentAddresses);
            
            var exceptAddresses = addresses.Except(currentAddresses);

            // foreach (var address in addresses) // Адреса из запроса
            // {
            //     IEnumerable<UserAddresses> ua = null;
            //     foreach (var currentAddress in currentAddresses) // Текущие адреса пользователя
            //     {
            //         if (currentAddress.Equals(address)) // Если адрес есть, то пропускаем
            //         {
            //             continue;
            //         }
            //         
            //         var addressCheck = new Address
            //         {
            //             LocalityId = (await db.Localities.FirstOrDefaultAsync(l => l.Name == address.locality)).Id,
            //             Street = address.street,
            //             Building = address.building,
            //             Apartment = address.apartment,
            //             Entrance = address.entrance,
            //             Level = address.level
            //         };
            //         
            //         var check = await db.Addresses.FirstOrDefaultAsync(a => // Ищем адрес с теми же данными
            //             a.LocalityId == addressCheck.LocalityId
            //             && a.Street == addressCheck.Street
            //             && a.Building == addressCheck.Building
            //             && a.Apartment == addressCheck.Apartment
            //             && a.Entrance == addressCheck.Entrance
            //             && a.Level == addressCheck.Level);
            //     }
            //     
            //         // if (check == null)
            //         // {
            //         //     await db.Addresses.AddAsync(addressCheck);
            //         // }
            //         //
            //         // await db.UserAddresses.AddAsync(new UserAddress
            //         // {
            //         //     UserLogin = login,
            //         //     Address = addressCheck
            //         // });
            //         //
            //         // break;
            // }

            // await db.SaveChangesAsync();

            return Ok(new {  exceptAddresses });
        }
    }
}