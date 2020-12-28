using System.Linq;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly deliveryContext db;
        public UserController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public string GetFirstLol()
        {
            return db.Localities.FirstOrDefault()?.Name;
        }
    }
}