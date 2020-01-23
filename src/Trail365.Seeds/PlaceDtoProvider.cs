using Trail365.DTOs;

namespace Trail365.Seeds
{
    public class PlaceDtoProvider
    {
        public static PlaceDto CreateHeiligenstadt()
        {
            var dto = new PlaceDto
            {
                Name = "Bahnhof Wien Heiligenstadt",
                CountryTwoLetterISOCode = "AT",
                City = "Vienna",
                Zip = "1190"
            };
            return dto;
        }

        public PlaceDto[] All { get; private set; }

        public static PlaceDtoProvider CreateInstance()
        {
            var p = new PlaceDtoProvider
            {
                All = new PlaceDto[] { CreateHeiligenstadt()
                        }
            };
            return p;
        }
    }
}
