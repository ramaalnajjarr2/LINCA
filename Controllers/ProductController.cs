using LINCA_v1.Bridge;
using LINCA_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINCA_v1.Controllers
{
    public class ProductController : Controller
    {
        private readonly bridge _context;
        private readonly UserManager<Users> _userManager;

        public ProductController(bridge context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // ✅ عرض كل المنتجات داخل متجر معيّن
        // /Product/ByMarket?marketId=1
        [AllowAnonymous]
        [HttpGet("Product/ByMarket")]
        [HttpGet("Product/ByMarket/{marketId:int}")]
        public async Task<IActionResult> ByMarket(int marketId)
        {
            var market = await _context.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == marketId);

            if (market == null) return NotFound();

            ViewBag.MarketName = market.Name;
            ViewBag.MarketId = marketId;

            // المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);

            var isSeller = user != null &&
                           (await _userManager.IsInRoleAsync(user, "Seller") || user.isSeller);

            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            ViewBag.IsSeller = isSeller;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUserId = user?.Id;

            // ✅ أهم سطر لحل مشكلة الزر:
            // يظهر فقط إذا المستخدم Seller وهو صاحب هذا المتجر
            ViewBag.CanAddProduct = isSeller && user != null && market.Ownerid == user.Id;

            var products = await _context.ProductsTable
                .AsNoTracking()
                .Where(p => p.MarketId == marketId)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            return View(products);
        }

        // ✅ تفاصيل المنتج
        [HttpGet]
        [Authorize(Roles = "Seller,Customer,Admin")]

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.ProductsTable
                .Include(p => p.Market)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            ViewBag.IsLoggedIn = user != null;
            ViewBag.IsAdmin = User.IsInRole("Admin");

            ViewBag.IsSellerOwner =
                user != null &&
                User.IsInRole("Seller") &&
                product.ApplicationUserId == user.Id;

            return View(product);
        }
        // =========================
        // Create
        // =========================
        [Authorize(Roles = "Seller")]
        // =========================
        // Create (GET)
        // =========================
        [Authorize(Roles = "Seller,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create(int marketId)
        {
            var market = await _context.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == marketId);

            if (market == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            // ✅ Seller لازم يكون صاحب المتجر
            if (!isAdmin && market.Ownerid != currentUserId)
                return Forbid();

            ViewBag.MarketId = market.Id;
            ViewBag.MarketName = market.Name;

            return View(new Productsprop { MarketId = market.Id });
        }

        // =========================
        // Create (POST)
        // =========================
        [Authorize(Roles = "Seller,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Productsprop model)
        {
            // ✅ تأكيد MarketId جاي من الـView
            var market = await _context.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == model.MarketId);

            if (market == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            // ✅ منع السيلر يضيف على متجر غير متجره
            if (!isAdmin && market.Ownerid != currentUserId)
                return Forbid();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // حقول ما تيجي من الفورم
            model.ApplicationUserId = user.Id;
            model.Status = "Available";

            // إزالة فاليديشن عنها (لو موجودة Required بالمودل)
            ModelState.Remove(nameof(Productsprop.ApplicationUserId));
            ModelState.Remove(nameof(Productsprop.Status));
            ModelState.Remove(nameof(Productsprop.Market));

            if (!ModelState.IsValid)
            {
                // ✅ مهم عشان ما يختفي اسم المتجر لما يرجع نفس الصفحة مع أخطاء
                ViewBag.MarketId = market.Id;
                ViewBag.MarketName = market.Name;
                return View(model);
            }

            _context.ProductsTable.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Product added successfully!";
            return RedirectToAction("ByMarket", new { marketId = model.MarketId });
        }

        // =========================
        // Edit
        // =========================
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.ProductsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // السيلر بس يعدّل منتجاته
            if (!isAdmin && product.ApplicationUserId != user.Id)
                return Forbid();

            // اسم المتجر للعرض
            var market = await _context.MarketsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == product.MarketId);

            ViewBag.MarketName = market?.Name ?? "Shop";
            ViewBag.MarketId = product.MarketId;

            return View(product);
        }
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Productsprop model)
        {
            if (id != model.ProductId) return BadRequest();

            var product = await _context.ProductsTable
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin && product.ApplicationUserId != user.Id)
                return Forbid();

            // ✅ احنا بنخلي هذول من الداتابيس مش من الفورم
            model.ApplicationUserId = product.ApplicationUserId;
            model.Status = product.Status;
            model.MarketId = product.MarketId;

            // إزالة فاليديشن للحقول اللي مش من الفورم
            ModelState.Remove(nameof(Productsprop.ApplicationUserId));
            ModelState.Remove(nameof(Productsprop.Status));
            ModelState.Remove(nameof(Productsprop.Market));

            if (!ModelState.IsValid)
            {
                var market = await _context.MarketsTable.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == product.MarketId);

                ViewBag.MarketName = market?.Name ?? "Shop";
                ViewBag.MarketId = product.MarketId;

                return View(model);
            }

            // ✅ تحديث الحقول المسموحة فقط
            product.ProductName = model.ProductName;
            product.Price = model.Price;
            product.imgurl = model.imgurl;
            product.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Product updated successfully!";
            return RedirectToAction("Details", new { id = product.ProductId });
        }

        // =========================
        // Delete
        // =========================
        [Authorize(Roles = "Seller,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.ProductsTable
                .Include(p => p.Market)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = (product.ApplicationUserId == userId || product.Market?.Ownerid == userId);

            if (!isAdmin && !isOwner) return Forbid();

            return View(product);
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.ProductsTable.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();

            var marketId = product.MarketId;

            _context.ProductsTable.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted ✅";
            return RedirectToAction(nameof(ByMarket), new { marketId });
        }
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> CreateMyStore()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // جيبي متجر السيلر
            var marketId = await _context.MarketsTable
                .Where(m => m.Ownerid == user.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync();

            if (marketId == 0)
            {
                TempData["Error"] = "You don't have a store yet.";
                return RedirectToAction("Index", "Market");
            }

            // حوليه على Create الأصلي ومعه marketId
            return RedirectToAction(nameof(Create), new { marketId = marketId });
        }
    }
}