using LINCA_v1.Bridge;
using LINCA_v1.Models;
using LINCA_v1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LINCA_v1.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly bridge _db;

        public CartController(bridge db)
        {
            _db = db;
        }

        private string CurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // =========================
        // GET: /Cart
        // =========================
        [HttpGet]
        [Authorize(Roles = "Seller,Customer")]

        public async Task<IActionResult> Index()
        {
            var buyerId = CurrentUserId();
            if (string.IsNullOrWhiteSpace(buyerId))
                return RedirectToAction("Login", "Account");

            var cart = await _db.CartItemsTable
                .AsNoTracking()
                .Where(c => c.BuyerId == buyerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            int? marketId = cart.FirstOrDefault()?.MarketId;
            if (marketId != null)
            {
                var market = await _db.MarketsTable
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == marketId.Value);

                ViewBag.MarketName = market?.Name ?? "";
                ViewBag.MarketId = marketId.Value;
            }

            return View(cart);
        }

        // =========================
        // POST: /Cart/Add
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller,Customer")]

        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var buyerId = CurrentUserId();
            if (string.IsNullOrWhiteSpace(buyerId))
                return RedirectToAction("Login", "Account");

            var product = await _db.ProductsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                TempData["CartError"] = "Product not found.";
                return RedirectToAction("Shop", "Market");
            }

            // سلة لمتجر واحد فقط
            var existingCartMarketId = await _db.CartItemsTable
                .Where(c => c.BuyerId == buyerId)
                .Select(c => (int?)c.MarketId)
                .FirstOrDefaultAsync();

            if (existingCartMarketId != null && existingCartMarketId.Value != product.MarketId)
            {
                TempData["CartError"] = "Your cart can contain items from only one store. Please clear your cart first.";
                return RedirectToAction(nameof(Index));
            }

            var existingItem = await _db.CartItemsTable
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _db.CartItemsTable.Add(new Cartitems
                {
                    BuyerId = buyerId,
                    MarketId = product.MarketId,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductImage = product.imgurl,
                    Price = product.Price,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            TempData["CartSuccess"] = "Added to cart ✅";
            return RedirectToAction("ByMarket", "Product", new { marketId = product.MarketId });
        }

        // =========================
        // POST: /Cart/UpdateQty
        // =========================
        [HttpPost]
        [Authorize(Roles = "Seller,Customer")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQty(int cartItemId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var buyerId = CurrentUserId();

            var item = await _db.CartItemsTable
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.BuyerId == buyerId);

            if (item == null)
            {
                TempData["CartError"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

            item.Quantity = quantity;
            await _db.SaveChangesAsync();

            TempData["CartSuccess"] = "Quantity updated ✓";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // POST: /Cart/Remove
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller,Customer")]

        public async Task<IActionResult> Remove(int cartItemId)
        {
            var buyerId = CurrentUserId();

            var item = await _db.CartItemsTable
                .FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.BuyerId == buyerId);

            if (item != null)
            {
                _db.CartItemsTable.Remove(item);
                await _db.SaveChangesAsync();
                TempData["CartSuccess"] = "Item removed 🗑️";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // POST: /Cart/Clear
        // =========================
        [HttpPost]
        [Authorize(Roles = "Seller,Customer")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var buyerId = CurrentUserId();

            var items = await _db.CartItemsTable
                .Where(c => c.BuyerId == buyerId)
                .ToListAsync();

            if (items.Any())
            {
                _db.CartItemsTable.RemoveRange(items);
                await _db.SaveChangesAsync();
            }

            TempData["CartSuccess"] = "Cart cleared ✓";
            return RedirectToAction(nameof(Index));
        }

        // ======================================================
        // ✅ NEW: POST /Cart/Checkout  (يستقبل بيانات التوصيل)
        // - بيجمع عناصر الكارت + يحط Phone/Address/Note من vm
        // - وبرجع View الفاتورة (Checkout.cshtml)
        // ======================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller,Customer")]

        public async Task<IActionResult> Checkout(OrderSummaryVM vm)
        {
            var buyerId = CurrentUserId();

            var cart = await _db.CartItemsTable
                .AsNoTracking()
                .Where(c => c.BuyerId == buyerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            if (!cart.Any())
            {
                TempData["CartError"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ تحقق سريع (المطلوب)
            if (string.IsNullOrWhiteSpace(vm.Phone1) || string.IsNullOrWhiteSpace(vm.Address))
            {
                TempData["CartError"] = "Phone 1 and Address are required.";
                return RedirectToAction(nameof(Index));
            }

            var marketId = cart.First().MarketId;

            var marketName = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => m.Id == marketId)
                .Select(m => m.Name)
                .FirstOrDefaultAsync() ?? "";

            var buyerName = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == buyerId)
                .Select(u => (u.FirstName + " " + u.LastName))
                .FirstOrDefaultAsync() ?? "Buyer";

            var sellerName = await _db.MarketsTable
                .AsNoTracking()
                .Where(m => m.Id == marketId)
                .Join(_db.Users, m => m.Ownerid, u => u.Id, (m, u) => (u.FirstName + " " + u.LastName))
                .FirstOrDefaultAsync() ?? "Seller";

            // 🔥 جهّزي VM كامل للفاتورة
            vm.MarketId = marketId;
            vm.MarketName = marketName;
            vm.BuyerId = buyerId;
            vm.BuyerName = buyerName;
            vm.SellerName = sellerName;

            vm.TotalPrice = cart.Sum(x => x.Price * x.Quantity);
            vm.Items = cart.Select(x => new OrderSummaryItemVM
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                ImageUrl = x.ProductImage,
                UnitPrice = x.Price,
                Quantity = x.Quantity
            }).ToList();

            // ✅ هون بتنعرض الفاتورة قبل Confirm
            return View(vm); // Views/Cart/Checkout.cshtml
        }

        // =========================
        // GET: /Cart/Checkout (fallback)
        // =========================
        [HttpGet]
        [Authorize(Roles = "Seller,Customer")]
        public async Task<IActionResult> Checkout()
        {
            // لو حدا فتح /Cart/Checkout مباشرة بدون POST
            TempData["CartError"] = "Please fill delivery info in the cart first.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // POST: /Cart/ConfirmOrder (بننشئ Order فعلي)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller,Customer")]

        public async Task<IActionResult> ConfirmOrder(OrderSummaryVM vm)
        {
            var buyerId = CurrentUserId();

            var cart = await _db.CartItemsTable
                .Where(c => c.BuyerId == buyerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            if (!cart.Any())
            {
                TempData["CartError"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ أهم شي: بيانات التوصيل
            if (string.IsNullOrWhiteSpace(vm.Phone1) || string.IsNullOrWhiteSpace(vm.Address))
            {
                TempData["CartError"] = "Phone1 and Address are required.";
                return RedirectToAction(nameof(Checkout));
            }

            var marketId = cart.First().MarketId;

            var productIds = cart.Select(c => c.ProductId).ToList();
            var products = await _db.ProductsTable
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            var order = new Order
            {
                MarketId = marketId,
                BuyerId = buyerId,
                Status = OrderStatus.Pending,

                Phone1 = vm.Phone1,
                Phone2 = vm.Phone2,
                Address = vm.Address,
                Note = vm.Note,

                TotalPrice = cart.Sum(x => x.Price * x.Quantity),
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            foreach (var ci in cart)
            {
                var p = products.FirstOrDefault(x => x.ProductId == ci.ProductId);
                var sellerId = p?.ApplicationUserId ?? buyerId;

                string? sellerName = null;
                if (!string.IsNullOrWhiteSpace(sellerId))
                {
                    sellerName = await _db.Users
                        .Where(u => u.Id == sellerId)
                        .Select(u => (u.FirstName + " " + u.LastName))
                        .FirstOrDefaultAsync();
                }

                order.Items.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price,
                    SellerId = sellerId,
                    SellerName = sellerName
                });
            }

            _db.OrderTable.Add(order);
            _db.CartItemsTable.RemoveRange(cart);

            await _db.SaveChangesAsync();

            TempData["CartSuccess"] = "Your order is confirmed ✓";

            // ✅ لا تروحي StoreOrders (هذا للسيلر)
            return RedirectToAction("MyOrder", "Order");
            // أو إذا بدك الفاتورة مباشرة:
            // return RedirectToAction("Invoice", "Order", new { id = order.Id });
        }
    }
}