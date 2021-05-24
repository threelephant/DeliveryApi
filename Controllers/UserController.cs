using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts;
using Delivery.Contracts.Account;
using Delivery.Contracts.User;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Enum;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы для обычного пользователя/клиента
    /// </summary>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly deliveryContext db;
        public UserController(deliveryContext db)
        {
            this.db = db;
        }

        [AuthorizeByLogin]
        [HttpGet("{login}/role")]
        public async Task<IActionResult> GetRole(string login)
        {
            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == login);

            return Ok(new
                {
                    login = user.Login,
                    role = user.Role.Name,
                }
            );
        }

        /// <summary>
        /// Возвращает информацию о пользователе
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Возвращает информацию о пользователе</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpGet("{login}")]
        public async Task<IActionResult> GetUser(string login)
        {
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

        /// <summary>
        /// Изменяет информацию о пользователе
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Возвращает информацию о пользователе</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpPut("{login}")]
        public async Task<IActionResult> ChangeUser([FromRoute] string login, [FromBody] UserInfo userInfo)
        {
            var changedUser = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            changedUser.FirstName = userInfo.name.first_name;
            changedUser.LastName = userInfo.name.last_name;
            changedUser.MiddleName = userInfo.name.middle_name;
            changedUser.Phone = userInfo.phone;

            await db.SaveChangesAsync();

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
                where address == null
                select userAddress);
            
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
            
            IEnumerable<UserAddress> GetUserAddrs(IEnumerable<UserAddresses> userAddressesList)
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
                    });
                
                return newUserAddrs;
            }
            
            var newUserAddrs = GetUserAddrs(newUserAddresses);
            var oldUserAddrs = GetUserAddrs(oldUserAddresses);
            
            await db.UserAddresses.AddRangeAsync(newUserAddrs);
            db.UserAddresses.RemoveRange(oldUserAddrs);
            await db.SaveChangesAsync();
            
            return Ok();
        }

        [AuthorizeByLogin]
        [HttpPut("{login}/maininfo")]
        public async Task<IActionResult> ChangeMainInfoUser([FromRoute] string login, [FromBody] UserMainInfo userMainInfo)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            user.FirstName = userMainInfo.first_name;
            user.LastName = userMainInfo.last_name;
            user.MiddleName = userMainInfo.middle_name;
            user.Phone = userMainInfo.phone;

            await db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Удаляет пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="204">Удаляет пользователя</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUser(string login)
        {
            var user = db.Users.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                db.Users.Remove(user);   
            }
            await db.SaveChangesAsync();
            
            return NoContent();
        }

        /// <summary>
        /// Возвращает адреса пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Возвращает адреса пользователя</response>
        [HttpGet("address")]
        public async Task<IActionResult> GetUserAddresses()
        {
            var addresses = await db.Addresses
                .Include(a => a.Locality)
                .Join(db.UserAddresses,
                a => a.Id,
                ua => ua.AddressId,
                (a, ua) => new { a, ua })
                .Where(aua => aua.ua.UserLogin == User.Identity.Name)
                .Select(a => new
                {
                    id = a.a.Id,
                    locality = a.a.Locality.Name,
                    street = a.a.Street,
                    building = a.a.Building,
                    apartment = a.a.Apartment,
                    entrance = a.a.Entrance,
                    level = a.a.Level,
                })
                .ToListAsync();

            return Ok(addresses);
        }
        
        /// <summary>
        /// Возвращает корзину пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Возвращает корзину пользователя</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpGet("{login}/cart")]
        public async Task<IActionResult> GetCart([FromRoute] string login)
        {
            var response = db.Carts
                .Where(c => c.UserLogin == login)
                .Select(c => new
                {
                    id = c.Product.Id,
                    title = c.Product.Title,
                    price = c.Product.Price,
                    weight = c.Product.Weight,
                    count = c.Count,
                    store_title = c.Product.Store.Title
                });

            return Ok(response);
        }
        
        /// <summary>
        /// Изменяет корзину пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <response code="200">Сообщение об успешном изменении</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [AuthorizeByLogin]
        [HttpPatch("{login}/cart")]
        public async Task<IActionResult> ChangeCart([FromRoute] string login, [FromBody] IEnumerable<CartPatch> cart) 
        {
            foreach (var cartItem in cart)
            {
                TryParse(cartItem.Op, out PatchOperation op);
                switch (op)
                {
                    case PatchOperation.add:
                    {
                        var crtItm = await db.Carts
                            .FirstOrDefaultAsync(c => c.ProductId == cartItem.Product.Id
                                                      && c.UserLogin == login);

                        if (crtItm != null)
                        {
                            crtItm.Count += cartItem.Product.Count;
                            db.Carts.Update(crtItm);
                            
                            break;
                        }
                        
                        await db.Carts.AddAsync(new Cart
                        {
                            UserLogin = login,
                            ProductId = cartItem.Product.Id,
                            Count = cartItem.Product.Count
                        });
                        break;
                    }
                    case PatchOperation.remove:
                    {
                        var crtItm = await db.Carts
                            .FirstOrDefaultAsync(c => c.ProductId == cartItem.Product.Id
                                                      && c.UserLogin == login);

                        if (crtItm != null)
                        {
                            crtItm.Count -= cartItem.Product.Count;
                            db.Carts.Update(crtItm);

                            if (crtItm.Count <= 0)
                            {
                                db.Carts.Remove(crtItm);
                                await db.SaveChangesAsync();
                            }
                        }

                        break;
                    }
                    default: return NotFound("Operation not found");
                }
            }

            await db.SaveChangesAsync();
            return Ok();
        }

        [AuthorizeByLogin]
        [HttpPost("{login}/cart/{id}/add")]
        public async Task<IActionResult> AddCartProduct([FromRoute] string login, [FromRoute] long id)
        {
            var crtItm = await db.Carts
                .FirstOrDefaultAsync(c => c.ProductId == id && c.UserLogin == login);

            if (crtItm != null)
            {
                crtItm.Count += 1;
                db.Carts.Update(crtItm);
                await db.SaveChangesAsync();
                            
                return Ok(crtItm);
            }

            var newCrtItm = new Cart
            {
                UserLogin = login,
                ProductId = id,
                Count = 1,
            };
                        
            await db.Carts.AddAsync(newCrtItm);
            await db.SaveChangesAsync();

            return Ok(newCrtItm);
        }

        [AuthorizeByLogin]
        [HttpPost("{login}/cart/{id}/remove")]
        public async Task<IActionResult> RemoveCartProduct([FromRoute] string login, [FromRoute] long id)
        {
            var crtItm = await db.Carts
                .FirstOrDefaultAsync(c => c.ProductId == id && c.UserLogin == login);

            if (crtItm != null)
            {
                crtItm.Count -= 1;
                db.Carts.Update(crtItm);

                if (crtItm.Count <= 0)
                {
                    db.Carts.Remove(crtItm);
                }
            }
            
            await db.SaveChangesAsync();
            return Ok(crtItm);
        }

        [AuthorizeByLogin]
        [HttpGet("{login}/cart/{id}")]
        public async Task<IActionResult> GetCartProduct([FromRoute] string login, [FromRoute] long id)
        {
            var crtItm = await db.Carts
                .FirstOrDefaultAsync(c => c.ProductId == id && c.UserLogin == login);

            if (crtItm == null)
            {
                return NotFound();
            }

            return Ok(crtItm);
        }

        [AuthorizeByLogin]
        [HttpDelete("{login}/cart")]
        public async Task<IActionResult> ClearCart([FromRoute] string login)
        {
            var userCart = db.Carts.Where(c => c.UserLogin == login);
            db.Carts.RemoveRange(userCart);

            await db.SaveChangesAsync();
            return NoContent();
        }

        [AuthorizeByLogin]
        [HttpPost("{login}/address")]
        public async Task<IActionResult> AddAddress([FromRoute] string login, [FromBody] UserAddresses addressRes)
        {
            var add = await db.Addresses.FirstOrDefaultAsync(a => a.Street == addressRes.street
                                                                            && a.Building == addressRes.building
                                                                            && a.Apartment == addressRes.apartment
                                                                            && a.Entrance == addressRes.entrance
                                                                            && a.Level == addressRes.level);

            if (add != null)
            {
                var userAdd = new UserAddress
                {
                    Address = add,
                    UserLogin = login,
                };
                await db.UserAddresses.AddAsync(userAdd);
                await db.SaveChangesAsync();

                return Ok();
            }
            
            var address = new Address
            {
                LocalityId = 1,
                Street = addressRes.street,
                Building = addressRes.building,
                Apartment = addressRes.apartment,
                Entrance = addressRes.entrance,
                Level = addressRes.level,
            };

            await db.Addresses.AddAsync(address);
            await db.UserAddresses.AddAsync(new UserAddress
            {
                Address = address,
                UserLogin = login,
            });

            
            await db.SaveChangesAsync();

            var newAddress = await db.Addresses.FirstOrDefaultAsync(a => a.Street == addressRes.street
                                                                          && a.Building == addressRes.building
                                                                          && a.Apartment == addressRes.apartment
                                                                          && a.Entrance == addressRes.entrance
                                                                          && a.Level == addressRes.level);

            var addressResponse = new
            {
                id = newAddress.Id,
                street = newAddress.Street,
                building = newAddress.Building,
                apartment = newAddress.Apartment,
                entrance = newAddress.Entrance,
                level = newAddress.Level,
            };
            return Ok(addressResponse);
        }

        [AuthorizeByLogin]
        [HttpPut("{login}/address/{id}")]
        public async Task<IActionResult> ChangeAddress([FromRoute] string login, 
            [FromRoute] long id, 
            [FromBody] UserAddresses addressReq)
        {
            var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id);
            address.Street = addressReq.street;
            address.Building = addressReq.building;
            address.Apartment = addressReq.apartment;
            address.Entrance = addressReq.entrance;
            address.Level = addressReq.level;
            
            db.Addresses.Update(address);
            await db.SaveChangesAsync();

            var addressRes = new
            {
                id = address.Id,
                street = address.Street,
                building = address.Building,
                apartment = address.Apartment,
                entrance = address.Entrance,
                level = address.Level,
            };

            return Ok(addressRes);
        }

        [AuthorizeByLogin]
        [HttpDelete("{login}/address/{id}")]
        public async Task<IActionResult> DeleteAddress([FromRoute] string login, [FromRoute] long id)
        {
            var userAddress = await db.UserAddresses.FirstOrDefaultAsync(ua => ua.UserLogin == login
                                                                            && ua.AddressId == id);

            if (userAddress == null)
            {
                return NotFound();
            }

            db.UserAddresses.Remove(userAddress);
            await db.SaveChangesAsync();

            return NoContent();
        }
    }
}