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
            if (login != User.Identity?.Name)
            {
                return Unauthorized();
            }
            
            var changedUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            changedUser.FirstName = userInfo.name.first_name;
            changedUser.LastName = userInfo.name.last_name;
            changedUser.MiddleName = userInfo.name.middle_name;
            changedUser.Phone = userInfo.phone;

            var addresses = userInfo.addresses.ToList();
            
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
                }).ToListAsync();

            var newUserAddresses = addresses.Where(a => 
                !currentAddresses.Contains(a)).ToList();
            var oldUserAddresses = currentAddresses.Where(a =>
                !addresses.Contains(a)).ToList();

            var newAddresses = (from userAddress in newUserAddresses 
                let address = db.Addresses.FirstOrDefault(a => 
                    a.Locality.Name == userAddress.locality 
                    && a.Street == userAddress.street 
                    && a.Building == userAddress.building 
                    && a.Apartment == userAddress.apartment 
                    && a.Entrance == userAddress.entrance 
                    && a.Level == userAddress.level
                    ) 
                where address == null select userAddress).ToList();
            
            var newAddr = newAddresses.Select(a => new Address
            {
                LocalityId = db.Localities.FirstOrDefault(l => l.Name == a.locality)?.Id,
                Street = a.street,
                Building = a.building,
                Apartment = a.apartment,
                Entrance = a.entrance,
                Level = a.level
            });

            await db.Addresses.AddRangeAsync(newAddr);
            await db.SaveChangesAsync();
            
            List<UserAddress> GetUserAddrs(IEnumerable<UserAddresses> userAddressesList)
            {
                var newUserAddrs = userAddressesList.Select(userAddress =>
                        db.Addresses.FirstOrDefault(a =>
                            a.Locality.Name == userAddress.locality
                            && a.Street == userAddress.street
                            && a.Building == userAddress.building
                            && a.Apartment == userAddress.apartment
                            && a.Entrance == userAddress.entrance
                            && a.Level == userAddress.level))
                    .Where(a => a != null)
                    .Select(addr => new UserAddress
                    {
                        AddressId = addr.Id,
                        UserLogin = login
                    }).ToList();
                
                return newUserAddrs;
            }
            
            var newUserAddrs = GetUserAddrs(newUserAddresses);
            var oldUserAddrs = GetUserAddrs(oldUserAddresses);
            
            await db.UserAddresses.AddRangeAsync(newUserAddrs);
            db.UserAddresses.RemoveRange(oldUserAddrs);
            await db.SaveChangesAsync();
            
            return Ok();
        }

        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUser(string login)
        {
            if (login != User.Identity?.Name)
            {
                return Unauthorized();
            }

            var user = db.Users.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                db.Users.Remove(user);   
            }
            await db.SaveChangesAsync();
            
            
            return NoContent();
        }
    }
}