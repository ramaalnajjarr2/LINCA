using System;
using System.Threading.Tasks;
using LINCA_v1.Bridge;
using LINCA_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINCA_v1.Controllers
{
    //SellerRequest/Create  
    [Authorize(Roles = "Customer")]
    public class SellerRequestController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly bridge _db;

        public SellerRequestController(UserManager<Users> userManager, bridge db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // already seller
            if (await _userManager.IsInRoleAsync(user, "Seller") || user.isSeller)
            {
                TempData["Error"] = "You are already a seller.";
                return RedirectToAction("Index", "Market");
            }

            // already has store
            var hasMarket = await _db.MarketsTable.AnyAsync(m => m.Ownerid == user.Id);
            if (hasMarket)
            {
                TempData["Error"] = "You already have a store.";
                return RedirectToAction("Index", "Market");
            }

            // check pending in DB (THIS is the truth)
            var pending = await _db.SellerRequestTable
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.Status == RequestStatus.Pending);

            ViewBag.HasPending = (pending != null);

            // if there is a pending request, show it (or show a page with only message)
            if (pending != null)
                return View(pending);

            // no pending -> show empty form
            return View(new SellerRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]

        public async Task<IActionResult> Create(SellerRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // already seller
            if (await _userManager.IsInRoleAsync(user, "Seller") || user.isSeller)
            {
                TempData["Error"] = "You are already a seller.";
                return RedirectToAction("Index", "Market");
            }

            // already has store
            var hasMarket = await _db.MarketsTable.AnyAsync(m => m.Ownerid == user.Id);
            if (hasMarket)
            {
                TempData["Error"] = "You already have a store.";
                return RedirectToAction("Index", "Market");
            }

            // prevent duplicate pending
            var hasPending = await _db.SellerRequestTable
                .AnyAsync(r => r.UserId == user.Id && r.Status == RequestStatus.Pending);

            if (hasPending)
            {
                TempData["Error"] = "You already have a pending request.";
                return RedirectToAction(nameof(Create));
            }

            // server-side fields first
            model.Id = 0;
            model.UserId = user.Id;
            model.Status = RequestStatus.Pending;
            model.CreatedAt = DateTime.UtcNow;

            // remove validation for server-set/navigation fields
            ModelState.Remove(nameof(SellerRequest.UserId));
            ModelState.Remove(nameof(SellerRequest.User));
            ModelState.Remove(nameof(SellerRequest.Status));
            ModelState.Remove(nameof(SellerRequest.CreatedAt));

            // payment proof rule
            if (string.IsNullOrWhiteSpace(model.paymentImageUrl) &&
                string.IsNullOrWhiteSpace(model.paymentDescription))
            {
                ModelState.AddModelError("", "Please provide payment proof (image URL or description).");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.HasPending = false;
                return View(model);
            }

            _db.SellerRequestTable.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your request is Pending. Admin will review it soon.";
            return RedirectToAction(nameof(Create));
        }
    }
}