namespace Trail365.Entities
{
    public enum ChallengeLevel
    {
        //Beginner, Intermediate, Advanced, Proficeency
        //Beginner, Elementary, Intermediate, Advanced, Upper-Intermediate
        //Basic, Intermediate,

        //Expert, Proficiient

        //Elevation Diagrams (Distance, AltitudeDelta)
        // beginners: 10km, 250m
        // intermediate: 20km, 500m (15km ?)
        // hills advanced: 30km, 1000m (800)
        // ultra skyrace: 60km, 2000m

        /// <summary>
        /// 10km/250m
        /// </summary>
        Basic = 0,

        /// <summary>
        /// 20km 500m
        /// </summary>
        Intermediate = 5,

        /// <summary>
        /// 30km, 1000m
        /// </summary>
        Advanced = 10,

        /// <summary>
        /// 60km, 2000m (Delta)
        /// </summary>
        Proficiency = 15
    }
}
