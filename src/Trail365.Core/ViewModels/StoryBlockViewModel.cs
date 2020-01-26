using System;
using System.Text;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    /// <summary>
    /// This ViewModel is used for two usecases
    /// 1. Block Frontent rendering
    /// 2. Story Block Frontend editing
    /// please create 2 different vm's a soon as needed!
    /// </summary>
    public class StoryBlockViewModel
    {
        public StoryBlock ApplyChangesTo(StoryBlock item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            item.BlockType = this.BlockType;
            item.RawContent = this.Content;
            item.SortOrder = this.SortOrder;
            item.BlockTypeGroup = this.BlockTypeGroup;
            return item;
        }

        public Guid StoryID { get; set; }

        /// <summary>
        /// BlockID (NOT Story ID)
        /// </summary>
        public Guid ID { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public StoryBlockType BlockType { get; set; } = StoryBlockType.Text;

        public string BlockTypeAsText { get => this.BlockType.ToDescription(); }

        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// multiple blocks with the same BlockType (Image) and the same group are handled like a album
        /// group changes are handled like album changes
        /// </summary>
        public int BlockTypeGroup { get; set; } = 0;


        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(this.Content) == false) return false;
            if (string.IsNullOrEmpty(this.ImageUrl) == false) return false;
            return true;
        }
    
    }
}
