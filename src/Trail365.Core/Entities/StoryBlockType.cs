namespace Trail365.Entities
{
    public enum StoryBlockType
    {
        /// <summary>
        /// text first, then image
        /// </summary>
        Text = 0,

        /// <summary>
        /// Image first, text (formatted as title afterwards)
        ///
        /// </summary>
        ///
        Title = 1,

        Excerpt = 2,
        Image = 3
    }
}
