namespace Trail365.Entities
{
    //Ideas for Roles: KeyMaster, Moderator, Partecipant, Spectator, Editor, Author, Subscriber

    /// <summary>
    /// Minimum-Level
    /// Role-Intent or Permission-Intent (intent => ~ requirement)
    /// => PERMISSION inside the enum, the Role-Assignemt is coded inside the CanXxx() Method
    /// </summary>
    public enum AccessLevel
    {
        /// <summary>
        /// Not authenticated
        /// </summary>
        Public = 0,

        /// <summary>
        /// Authenticated
        /// </summary>
        User = 1,

        /// <summary>
        /// Permission Level 1, currently Member
        /// </summary>
        Member = 2,

        Moderator = 5,

        /// <summary>
        /// Permission Level 3
        /// </summary>
        Administrator = 100
    }
}
