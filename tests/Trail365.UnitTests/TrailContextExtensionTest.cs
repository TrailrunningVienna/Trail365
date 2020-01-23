using System.Collections.Generic;
using Trail365.Data;
using Trail365.DTOs;
using Trail365.Entities;
using Xunit;

namespace Trail365.UnitTests
{
    [Trait("Category", "BuildVerification")]
    public class TrailContextExtensionTest
    {
        [Theory]
        [InlineData("n1", "c1", null, null, null, true)]
        [InlineData("n1", "c1", null, "src1", "id1", true)]
        [InlineData("n1", "c1", "ct1", null, null, true)]
        [InlineData("n1", "c1", "ct1", "src1", "id1", true)]
        public void ShouldGetPlaceCandidate(string name, string country, string city, string externalSource, string externalID, bool shouldMatch)
        {
            List<Place> source = new List<Place>
            {
                new Place
                {
                    Name = name,
                    Country = country,
                    City = city,
                    ExternalSource = externalSource,
                    ExternalID = externalID
                }
            };

            PlaceDto dto = new PlaceDto
            {
                Name = name,
                Country = country,
                City = city
            };

            var entity = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto, true, "s1", "id1");
            Assert.Equal(shouldMatch, entity != null);
        }

        [Fact]
        public void ShouldGetPlaceCandidate_WithCountry()
        {
            List<Place> source = new List<Place>
            {
                new Place
                {
                    Name = "N1",
                    Country = null,
                    City = null,
                    ExternalSource = "src1",
                    ExternalID = "id1"
                },
                new Place
                {
                    Name = "N1",
                    Country = "CNT",
                    City = null,
                    ExternalSource = "src1",
                    ExternalID = "id1"
                }
            };

            PlaceDto dto = new PlaceDto
            {
                Name = "N1",
                Country = null,
                City = null
            };

            var entity1 = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto, true, null, null);
            Assert.NotNull(entity1);
            var entity2 = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto, true, "src1", "id1");
            Assert.NotNull(entity2);

            PlaceDto dto2 = new PlaceDto
            {
                Name = "N1",
                Country = "CNT",
                City = null
            };
            var entity3 = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto2, true, null, null);
            Assert.NotNull(entity3);
        }

        [Fact]
        public void ShouldGetPlaceCandidate_WithCity()
        {
            List<Place> source = new List<Place>
            {
                new Place
                {
                    Name = "N1",
                    Country = null,
                    City = "CTY",
                    ExternalSource = "src1",
                    ExternalID = "id1"
                },
                new Place
                {
                    Name = "N1",
                    Country = null,
                    City = "CTZ",
                    ExternalSource = "src1",
                    ExternalID = "id1"
                }
            };

            PlaceDto dto = new PlaceDto
            {
                Name = "N1",
                Country = null,
                City = "CTZ"
            };

            var entity1 = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto, true, null, null);
            Assert.Null(entity1); //null because not unique

            var entity2 = TrailContextExtension.GetPlaceCandidateOrDefault(source, dto, false, null, null);
            Assert.NotNull(entity2);
        }
    }
}
