using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Trail365.Entities;
using Trail365.ViewModels;

namespace Trail365.Web.Backend.Controllers
{
    [Area("Backend")]
    [Route("[area]/[controller]/[action]")]
    [Authorize(Roles = BackendSetup.Roles)]
    public partial class UsersController : Controller
    {
        private readonly IdentityContext _context;

        public UsersController(IdentityContext context)
        {
            _context = context;
        }

        // GET: Backend/Users
        public IActionResult Index()
        {
            List<User> users = null;
            using (var tracker = _context.DependencyTracker("Users"))
            {
                users = _context.Users.Include(u => u.Identities).ToList();
            }
            var viewModel = users.Select(u => UserBackendViewModel.CreateFromUser(u, true)).ToList();
            return this.View(viewModel);
        }

        // GET: Backend/Users/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var item = await _context.Users.Where(ev => ev.ID == id).SingleOrDefaultAsync();

            if (item == null)
            {
                return this.NotFound();
            }
            var vm = UserBackendViewModel.CreateFromUser(item, false);
            vm.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            return this.View(UserBackendViewModel.CreateFromUser(item, false));
        }

        // POST: Backend/Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind()] UserBackendViewModel model)
        {
            if (id != model.ID)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var loaded = _context.Users.Find(model.ID);
                    if (loaded == null)
                    {
                        return base.NotFound();
                    }
                    model.ApplyChangesTo(loaded);
                    _context.Update(loaded);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.UserExists(model.ID))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return this.RedirectToAction(nameof(Index));
            }
            return this.View(model);
        }

        // GET: Backend/Users/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.ID == id);

            if (user == null)
            {
                return this.NotFound();
            }
            throw new NotImplementedException(("confirm user deletion"));
            //return this.View(user);
        }

        // POST: Backend/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return this.RedirectToAction(nameof(Index));
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.ID == id);
        }
    }
}
