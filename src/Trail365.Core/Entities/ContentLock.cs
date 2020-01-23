using System;

namespace Trail365.Entities
{
    [Flags]
    public enum ContentLock
    {
        None = 0,

        /// <summary>
        /// Import from external Source should NOT overwrite/change this field!
        /// </summary>
        ///
        ImportLock = 1,

        /// <summary>
        /// users should overwrite this field, mor details TBD
        /// </summary>
        //UserLock =2
    }
}
