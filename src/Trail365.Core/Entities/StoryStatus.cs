using System.ComponentModel;

namespace Trail365.Entities
{
    public enum StoryStatus : int
    {
        /// <summary>
        /// published, active, released, in the past or in the future doesn't matter.
        /// </summary>
        [Description("Published")]
        Default = 0,

        Draft = 1,

        /// <summary>
        /// Block Upload is "work in progress", status should change after that
        /// </summary>
        Upload = 4,
    }
}
