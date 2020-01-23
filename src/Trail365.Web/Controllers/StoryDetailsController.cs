using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trail365.Data;
using Trail365.Entities;
using Trail365.Internal;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    [Authorize(Roles = "Admin,User,Member,Moderator")]
    public class StoryDetailsController : Controller
    {
        private readonly TrailContext _context;

        /// <summary>
        /// Shows List of Blocks as entry into the edit process
        /// </summary>
        /// <param name="id">Story-ID</param>
        /// <returns></returns>
        public IActionResult Index(Guid id)
        {
            if (id == Guid.Empty)
            {
                return this.NotFound();
            }

            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(id, true, login.GetListAccessPermissionsForCurrentLogin());
            var item = _context.GetStoriesByFilter(sqf).SingleOrDefault();

            if (item == null)
            {
                return this.NotFound();
            }

            var model = StoryBackendViewModel.CreateFromStory(item);
            model.Login = login;
            return this.View(model);
        }

        public StoryDetailsController(TrailContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Insert(Guid Id, Guid relativeId, string relative)
        {
            if (Id == null)
            {
                return this.NotFound();
            }

            var item = _context.Stories.Where(st => st.ID == Id).Include(st => st.StoryBlocks).ThenInclude(b => b.Image).SingleOrDefault();

            if (item == null)
            {
                return this.NotFound();
            }

            var sortedBlocks = item.StoryBlocks.OrderBy(b => b.SortOrder).ToList();

            StoryBlock insertedBlock = new StoryBlock { StoryID = Id, Story = item };

            StoryBlock lastBefore = null;
            StoryBlock nextAfter = null;

            lastBefore = item.StoryBlocks.SingleOrDefault(b => b.ID == relativeId);
            int lastBeforeIndex = sortedBlocks.IndexOf(lastBefore);
            nextAfter = sortedBlocks[lastBeforeIndex + 1];

            int requiredSortValue = nextAfter.SortOrder;

            //insertAfter:
            for (int i = lastBeforeIndex + 1; i < sortedBlocks.Count; i++)
            {
                StoryBlock b1 = sortedBlocks[i];
                b1.SortOrder += 1;
            }
            Guard.Assert(sortedBlocks.FirstOrDefault(b => b.SortOrder == requiredSortValue) == null);
            insertedBlock.SortOrder = requiredSortValue;
            item.StoryBlocks.Add(insertedBlock);
            _context.StoryBlocks.Add(insertedBlock);
            _context.Stories.Update(item);
            var changes = _context.SaveChanges();
            return this.RedirectToAction("Edit", "StoryDetailEditor", new { item.ID });
        }
    }
}
