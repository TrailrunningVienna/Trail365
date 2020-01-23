using System.ComponentModel;

namespace Trail365.Entities
{
    public enum EventStatus : int
    {
        /// <summary>
        /// published, active, released, in the past or in the future doesn't matter.
        /// </summary>
        [Description("Published")]
        Default = 0,

        Draft = 1,
        Canceled = 2,
    }
}
