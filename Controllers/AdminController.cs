using LINCA_v1.Bridge;
using LINCA_v1.Models;
using LINCA_v1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINCA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly bridge _bridge;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(bridge bridge, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _bridge = bridge;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult ProfileSettings()
        {
            return View();
        }

        // =========================
        // Products Moderation
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingProducts()
        {
            var pendingproducts = await _bridge.ProductsTable
                .Where(p => p.Status == "Pending")
                .ToListAsync();

            return View(pendingproducts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Approve(int id)
        {
            var product = _bridge.ProductsTable.FirstOrDefault(p => p.ProductId == id);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Productsprop productModel)
        {
            var product = await _bridge.ProductsTable.FindAsync(productModel.ProductId);
            if (product == null) return NotFound();

            if (product.Status != "Pending")
            {
                ViewBag.ErrorMessage = "Product is not pending";
                return View(product);
            }

            product.Status = "Approved";
            await _bridge.SaveChangesAsync();

            return RedirectToAction(nameof(PendingProducts));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Reject(int id)
        {
            var product = _bridge.ProductsTable.FirstOrDefault(p => p.ProductId == id);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Productsprop productModel)
        {
            var product = await _bridge.ProductsTable.FindAsync(productModel.ProductId);
            if (product == null) return NotFound();

            if (product.Status == "Pending")
            {
                product.Status = "Rejected";
                await _bridge.SaveChangesAsync();
            }

            return RedirectToAction(nameof(PendingProducts));
        }

        // ✅ Delete any product (safety)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _bridge.ProductsTable.FindAsync(id);
            if (product == null) return NotFound();

            _bridge.ProductsTable.Remove(product);
            await _bridge.SaveChangesAsync();

            TempData["Success"] = "Product deleted.";
            return RedirectToAction(nameof(PendingProducts));
        }

        // =========================
        // Seller Upgrade Requests
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SellerRequests()
        {
            var requests = await _bridge.SellerRequestTable
                .Include(r => r.User)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSellerRequest(int id)
        {
            var req = await _bridge.SellerRequestTable
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();
            if (req.Status != RequestStatus.Pending) return BadRequest("Not pending.");
            if (req.User == null) return BadRequest("Request user not found.");

            // ✅ إذا اليوزر أصلاً Seller
            if (await _userManager.IsInRoleAsync(req.User, "Seller"))
            {
                req.Status = RequestStatus.Approved;
                await _bridge.SaveChangesAsync();

                TempData["Success"] = "User is already a Seller.";
                return RedirectToAction(nameof(SellerRequests));
            }

            // ✅ إذا عنده متجر أصلاً (منع متجرين)
            var hasStore = await _bridge.MarketsTable.AnyAsync(m => m.Ownerid == req.UserId);
            if (hasStore)
            {
                req.Status = RequestStatus.Approved;
                await _bridge.SaveChangesAsync();

                TempData["Success"] = "User already has a store.";
                return RedirectToAction(nameof(SellerRequests));
            }

            // ✅ تأكيد وجود Role Seller
            if (!await _roleManager.RoleExistsAsync("Seller"))
                await _roleManager.CreateAsync(new IdentityRole("Seller"));

            // ✅ إنشاء المتجر
            var market = new Marketprop
            {
                Name = req.StoreName,
                Ownerid = req.UserId,
                Description = req.StoreDescription,
                imgurl = req.StoreImageUrl
            };

            _bridge.MarketsTable.Add(market);


            // update user role + flag
            req.User.isSeller = true;
            await _userManager.AddToRoleAsync(req.User, "Seller");

            // ✅ تحديث اليوزر (بدون حذف Customer)
            req.User.isSeller = true;
            await _userManager.AddToRoleAsync(req.User, "Seller");

            // تحديث حالة الطلب

            req.Status = RequestStatus.Approved;

            await _bridge.SaveChangesAsync();
            TempData["Success"] = "Seller request approved & store created.";

            return RedirectToAction(nameof(SellerRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectSellerRequest(int id)
        {
            var req = await _bridge.SellerRequestTable.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();

            req.Status = RequestStatus.Rejected;
            await _bridge.SaveChangesAsync();

            TempData["Success"] = "Seller request rejected.";
            return RedirectToAction(nameof(SellerRequests));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UsersPanel()
        {
            var allRoles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(n => n != null)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var vm = new List<UserRole>();
            foreach (var u in users)
            {
                var roles = (await _userManager.GetRolesAsync(u)).ToList();

                vm.Add(new UserRole
                {
                    UserId = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    Roles = roles,
                    SelectedRole = roles.FirstOrDefault() ?? "Customer",
                    AllRoles = allRoles
                });
            }

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetUserRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
                return BadRequest("Missing userId or role.");

            role = role.Trim();

            // فقط هالأدوار
            var allowedRoles = new[] { "Admin", "Seller", "Customer" };
            if (!allowedRoles.Contains(role))
                return BadRequest("Invalid role.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // تأكدي الدور موجود
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();

            // ====== 1) لو بدنا نخليه Admin: لازم يكون Admin فقط ======
            if (role == "Admin")
            {
                // إذا كان Seller -> احذف متجره (وإيش متعلق فيه)
                if (currentRoles.Contains("Seller"))
                {
                    var myMarket = await _bridge.MarketsTable
                        .FirstOrDefaultAsync(m => m.Ownerid == user.Id);

                    if (myMarket != null)
                    {
                        // (اختياري) احذف منتجات المتجر
                        var products = await _bridge.ProductsTable
                            .Where(p => p.MarketId == myMarket.Id)
                            .ToListAsync();

                        if (products.Any())
                            _bridge.ProductsTable.RemoveRange(products);

                        // احذف المتجر
                        _bridge.MarketsTable.Remove(myMarket);

                        await _bridge.SaveChangesAsync();
                    }
                }

                // شيل كل الأدوار وخلي Admin فقط
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                await _userManager.AddToRoleAsync(user, "Admin");

                user.isSeller = false;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "User is now Admin only (store removed if existed).";
                return RedirectToAction(nameof(UsersPanel));
            }

            // ====== 2) لو المستخدم Admin وحابة تحوّليه Seller/Customer ======
            // ممنوع Admin + غيره، فبنشيله من Admin أولاً
            if (currentRoles.Contains("Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                currentRoles.Remove("Admin");
            }

            // ====== 3) Seller + Customer مسموح ======
            // Toggle style: إذا كان عنده الدور بنشيله، إذا ما عنده بنضيفه
            if (currentRoles.Contains(role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);

                if (role == "Seller")
                    user.isSeller = false;

                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"Role {role} removed.";
                return RedirectToAction(nameof(UsersPanel));
            }
            else
            {
                await _userManager.AddToRoleAsync(user, role);

                if (role == "Seller")
                    user.isSeller = true;

                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"Role {role} added.";
                return RedirectToAction(nameof(UsersPanel));
            }
        }
    }

}