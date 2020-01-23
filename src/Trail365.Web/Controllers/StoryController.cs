using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Data;
using Trail365.ViewModels;

namespace Trail365.Web.Controllers
{
    public class StoryController : Controller
    {
        private readonly TrailContext _context;

        public StoryViewModel InitStoryViewModel(Guid storyID, bool includeImages)
        {
            var login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            StoryQueryFilter sqf = StoryQueryFilter.GetByID(storyID, true, login.GetListAccessPermissionsForCurrentLogin());
            var story = _context.GetStoriesByFilter(sqf).SingleOrDefault();

            if (story == null)
            {
                throw new InvalidOperationException($"Story with ID '{storyID}' does not exist.");
            }

            return story.ToStoryViewModel(login);
        }

        public IActionResult Index(StoryRequestViewModel requestModel)
        {
            if (requestModel.ID.HasValue == false) return base.BadRequest();

            var model = this.InitStoryViewModel(requestModel.ID.Value, includeImages: true);

            if (requestModel.NoConsent.HasValue)
            {
                model.NoConsent = requestModel.NoConsent.Value;
            }

            return this.View(model);
        }

        public StoryController(TrailContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public StoryCreationViewModel InitStory(StoryCreationViewModel model, Guid? storyID)
        {
            if (model == null)
            {
                model = new StoryCreationViewModel();
            }
            model.Login = LoginViewModel.CreateFromClaimsPrincipalOrDefault(this.User);
            if (storyID.HasValue)
            {
                model.ID = storyID.Value;
            }
            return model;
        }
    }
}
