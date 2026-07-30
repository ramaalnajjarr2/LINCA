using LINCA_v1.Bridge;
using LINCA_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINCA_v1.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly bridge _db;
        private readonly UserManager<Users> _userManager;

        public OrderController(bridge db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // =========================
        // BUYER: My Orders
        // =========================
        [Authorize(Roles = "Customer,Seller")]
        [HttpGet]
        public async Task<IActionResult> MyOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var orders = await _db.OrderTable
                .AsNoTracking()
                .Where(o => o.BuyerId == user.Id)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var marketIds = orders.Select(o => o.MarketId).Distinct().ToList();
            var markets = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => marketIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name);

            ViewBag.MarketNames = markets;

            return View(orders);
        }

        // =========================
        // SELLER: Orders for my store (Dashboard)
        // =========================
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> StoreOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var myMarket = await _db.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Ownerid == user.Id);

            if (myMarket == null)
            {
                TempData["Error"] = "No store found for this seller.";
                return RedirectToAction("Dashboard", "Seller");
            }

            var orders = await _db.OrderTable
                .AsNoTracking()
                .Where(o => o.MarketId == myMarket.Id)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.MarketName = myMarket.Name;
            ViewBag.MarketId = myMarket.Id;

            return View(orders);
        }

        // =========================
        // ADMIN: All orders (Read-only)
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AdminOrders()
        {
            var orders = await _db.OrderTable
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var marketIds = orders.Select(o => o.MarketId).Distinct().ToList();
            var markets = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => marketIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name);

            ViewBag.MarketNames = markets;

            return View(orders);
        }

        // =========================
        // Seller: Change Status
        // =========================
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int orderId, OrderStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.OrderTable.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            var myMarket = await _db.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Ownerid == user.Id);

            if (myMarket == null || order.MarketId != myMarket.Id)
                return Forbid();

            if (order.Status == OrderStatus.Completed)
            {
                TempData["Error"] = "Completed orders can't be changed.";
                return RedirectToAction(nameof(StoreOrders));
            }

            if (status == OrderStatus.Completed && order.Status != OrderStatus.Accepted)
            {
                TempData["Error"] = "Order must be Accepted before Completed.";
                return RedirectToAction(nameof(StoreOrders));
            }
           
            

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Order #{order.Id} updated to {status}.";
            return RedirectToAction(nameof(StoreOrders));
        }

        // =========================
        // Details
        // =========================
        [Authorize(Roles = "Customer,Seller,Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.OrderTable
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (User.IsInRole("Customer") && !User.IsInRole("Admin") && !User.IsInRole("Seller"))
            {
                if (order.BuyerId != user.Id) return Forbid();
            }

            if (User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                var myMarket = await _db.MarketsTable
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Ownerid == user.Id);

                if (myMarket == null || order.MarketId != myMarket.Id) return Forbid();
            }

            var marketName = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => m.Id == order.MarketId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync() ?? "";

            ViewBag.MarketName = marketName;

            return View(order);
        }

        // =========================
        // Delete/Cancel Order 
        // =========================
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.OrderTable
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (User.IsInRole("Customer") && !User.IsInRole("Admin"))
            {
                if (order.BuyerId != user.Id) return Forbid();
            }

            // ✅ ممنوع إذا Accepted أو Completed
            if (order.Status == OrderStatus.Accepted || order.Status == OrderStatus.Completed)
            {
                TempData["CartError"] = "This order is already being prepared and cannot be cancelled.";
                return RedirectToAction(nameof(MyOrder));
            }

            if (_db.OrderItemTable != null)
                _db.OrderItemTable.RemoveRange(order.Items);
            else
                _db.RemoveRange(order.Items);

            _db.OrderTable.Remove(order);
            await _db.SaveChangesAsync();

            TempData["CartSuccess"] = "Order deleted successfully 🗑️";
            return RedirectToAction(nameof(MyOrder));
        }

        // =========================
        // Invoice View
        // =========================
        [Authorize(Roles = "Customer,Seller,Admin")]
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _db.OrderTable
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var marketName = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => m.Id == order.MarketId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync() ?? "";

            ViewBag.MarketName = marketName;
            return View(order);
        }
    }
}