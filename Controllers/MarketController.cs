using LINCA_v1.Bridge;
using LINCA_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINCA_v1.Controllers
{
    public class MarketController : Controller
    {
        private readonly bridge _db;
        public MarketController(bridge db) => _db = db;
        [Authorize(Roles = "Seller,Customer,Admin")]
        public async Task<IActionResult> shop()
        {
            var markets = await _db.MarketsTable.ToListAsync(); // تأكدي اسم DbSet
            return View(markets);
        }
    }
}