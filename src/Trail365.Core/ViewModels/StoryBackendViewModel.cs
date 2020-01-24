using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class StoryBackendViewModel
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public string Excerpt { get; set; }

        public DateTime? Created { get; set; }

        public Guid? CoverImageID { get; set; }

        public DateTime? Modified { get; set; }

        public Story ApplyChangesTo(Story item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.Name = this.Name;
            item.Excerpt = this.Excerpt;

            if (this.CoverImageID.HasValue)
            {
                if (this.CoverImageID.Value == Guid.Empty)
                {
                    item.CoverImage = null;
                    item.CoverImageID = null;
                }
                else
                {
                    item.CoverImage = null;
                    item.CoverImageID = this.CoverImageID.Value;
                }
            }
            else
            {
                item.CoverImage = null;
                item.CoverImageID = null;
            }

            item.PublishedUtc = this.Published.ToUniversalTime();
            item.ListAccess = this.ListAccess;
            if (item.Status != this.Status)
            {
                if (this.Status == StoryStatus.Default)
                {
                    item.PublishedUtc = DateTime.UtcNow;
                }
                item.Status = this.Status;
            }
            
            return item;
        }

        [Display(Name = "Read Access")]
        public AccessLevel ListAccess { get; set; }

        public DateTime? Published { get; set; }

        public StoryStatus Status { get; set; }

        public static StoryBackendViewModel CreateFromStory(Story from)
        {
            var vm = new StoryBackendViewModel
            {
                ID = from.ID,
                Name = from.Name,
                Created = from.CreatedUtc.ToLocalTime(),
                Modified = from.ModifiedUtc?.ToLocalTime(),
                Status = from.Status,
                ListAccess = from.ListAccess,
                Published = from.PublishedUtc.ToLocalTime(),
                Excerpt = from.Excerpt,
                CoverImageID = from.CoverImageID
            };
            vm.Blocks = from.StoryBlocks.OrderBy(sb => sb.SortOrder).Select(sb => new StoryBlockBackendViewModel { ID = sb.ID, BlockType = sb.BlockType, Url = sb.Image?.Url, Content = sb.RawContent, SortOrder = sb.SortOrder, BlockTypeGroup = sb.BlockTypeGroup }).ToList();
            return vm;
        }

        public List<Tuple<string, string>> References { get; set; } = new List<Tuple<string, string>>();

        public List<StoryBlockBackendViewModel> Blocks { get; set; } = new List<StoryBlockBackendViewModel>();

        public LoginViewModel Login { get; set; } = new LoginViewModel();
    }
}
