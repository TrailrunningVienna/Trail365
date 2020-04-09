using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public static class ViewModelExtension
    {
        public static bool CanDo(this LoginViewModel login, AccessLevel requested)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            if (requested == AccessLevel.Public)
            {
                return true;
            }

            if (login.IsLoggedIn == false)
            {
                return false;
            }

            if (login.IsAdmin)
            {
                return true;
            }

            if ((login.IsModerator) && (requested <= AccessLevel.Moderator))
            {
                return true;
            }

            switch (requested)
            {
                case AccessLevel.Member:
                    return login.IsMember;

                case AccessLevel.Administrator:
                    return login.IsAdmin;

                case AccessLevel.Moderator:
                    return login.IsModerator;

                case AccessLevel.User:
                    return login.IsLoggedIn;

                default:
                    throw new NotImplementedException($"AccessLevel '{requested}' not implemented for {nameof(CanDo)}");
            }
        }

        public static Trail ToTrail(this TrailViewModel model)
        {
            return new Trail() { ID = model.ID, Name = model.Name, Description = model.Description, ListAccess = model.ListAccess, GpxDownloadAccess = model.GpxDownloadAccess, Excerpt = model.Excerpt };
        }

        public static StoryBlockViewModel ToStoryBlockViewModel(this StoryBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            var blvm = new StoryBlockViewModel
            {
                ID = block.ID,
                BlockType = block.BlockType,
                Content = block.RawContent,
                ImageUrl = block.Image?.Url,
                SortOrder = block.SortOrder,
                StoryID = block.StoryID,
                BlockTypeGroup = block.BlockTypeGroup
            };
            return blvm;
        }

        public static StoryViewModel ToStoryViewModel(this Story story, LoginViewModel login)
        {
            if (story == null) throw new ArgumentNullException(nameof(story));
            var vm = new StoryViewModel
            {
                Name = story.Name,
                ID = story.ID,
                Created = story.CreatedUtc.ToLocalTime(),
                Modified = story.ModifiedUtc,
                ListAccess = story.ListAccess,
                Excerpt = story.Excerpt,
                TitleImageUrl = story.CoverImage?.Url,
                TitleImageSize = (story.CoverImage == null ? System.Drawing.Size.Empty : story.CoverImage.GetImageSizeOrDefault())
            };

            vm.Login = login ?? throw new ArgumentNullException(nameof(login));

            foreach (var bl in story.StoryBlocks.OrderBy(sb => sb.SortOrder))
            {
                vm.Blocks.Add(bl.ToStoryBlockViewModel());
            }

            return vm;
        }

        public static PlaceViewModel ToPlaceViewModel(this Place place)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));
            return new PlaceViewModel
            {
                Name = place.Name,
                Country = place.Country,
                City = place.City,
                ID = place.ID,
                IsCityPartOfTheName = place.IsCityPartOfTheName,
                MeetingPoint = place.MeetingPoint,
            };
        }

        public static EventViewModel ToEventViewModel(this Event singleEvent, IUrlHelper helper, LoginViewModel login, Blob[] trailPreviewImages, bool hasTrailPermission)
        {
            if (singleEvent == null) throw new ArgumentNullException(nameof(singleEvent));
            if (helper == null) throw new ArgumentNullException(nameof(helper));
            if (login == null) throw new ArgumentNullException(nameof(login));

            var vm = new EventViewModel
            {
                Name = singleEvent.Name,
                ID = singleEvent.ID,
                Created = singleEvent.CreatedUtc.ToLocalTime(),
                Modified = singleEvent.ModifiedUtc,
                ListAccess = singleEvent.ListAccess,
                Excerpt = singleEvent.Excerpt,
                Description = singleEvent.Description,
                DetailsUrl = helper.GetEventUrl(singleEvent.ID),
                FullDayEvent = singleEvent.FullDayEvent
            };

            if (singleEvent.CoverImage != null)
            {
                vm.CoverImageUrl = singleEvent.CoverImage.Url;
                vm.CoverImageSize = singleEvent.CoverImage.GetImageSizeOrDefault();
            }

            if (singleEvent.TryGetValidDates(out var range))
            {
                vm.StartDate = range.Item1.ToLocalTime().Date;
                vm.EndDate = range.Item2.ToLocalTime().Date;
            }

            vm.StartTime = singleEvent.StartTimeUtc.ToLocalTime();
            vm.EndTime = singleEvent.EndTimeUtc.ToLocalTime();

            if (singleEvent.Place != null)
            {
                vm.Place = singleEvent.Place.ToPlaceViewModel();
            }

            if (singleEvent.EndPlace != null)
            {
                vm.EndPlace = singleEvent.EndPlace.ToPlaceViewModel();
            }

            if (hasTrailPermission)
            {
                if (singleEvent.Trail != null)
                {
                    vm.Trail = singleEvent.Trail.ToTrailViewModel(login, trailPreviewImages == null, trailPreviewImages);
                    vm.Trail.HideName = true; //if used as child of event then hide name o UI!
                    vm.Trail.HideExcerpt = true;
                }
            }

            vm.Login = login;
            return vm;
        }

        public static TrailViewModel ToTrailViewModel(this Trail trail, LoginViewModel login)
        {
            return ToTrailViewModel(trail, login, true, null);
        }

        public static TrailViewModel ToTrailViewModel(this Trail trail, LoginViewModel login, Blob[] previewImages)
        {
            if (previewImages == null) throw new ArgumentNullException(nameof(previewImages));
            return ToTrailViewModel(trail, login, false, previewImages);
        }

        public static TrailViewModel ToTrailViewModel(this Trail trail, LoginViewModel login, bool ignorePreviewImages, params Blob[] previewImages)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));

            var vm = new TrailViewModel
            {
                Name = trail.Name,
                Description = trail.Description,
                ID = trail.ID,
                Created = trail.CreatedUtc.ToLocalTime(),
                Modified = trail.ModifiedUtc,
                GpxUrl = trail.GpxBlob?.Url,
                AnalyzerUrl = trail.AnalyzerBlob?.Url,
                GpxDownloadAccess = trail.GpxDownloadAccess,
                ListAccess = trail.ListAccess,
                Excerpt = trail.Excerpt, //TODO if taril.Excerpt is empty, try to generate one from description!
                Ascent = trail.AscentMeters,
                Descent = trail.DescentMeters,
                UnclassifiedMeters = trail.UnclassifiedMeters,
                UnpavedTrailMeters = trail.UnpavedTrailMeters,
                PavedRoadMeters = trail.PavedRoadMeters,
                AsphaltedRoadMeters = trail.AsphaltedRoadMeters
            };

            if (trail.GpxBlob != null)
            {
                vm.GpxDownloadFileName = trail.GpxBlob.OriginalFileName;
            }

            vm.Login = login ?? throw new ArgumentNullException(nameof(login));

            if (trail.DistanceMeters.HasValue)
            {
                vm.DistanceKm = Convert.ToDouble(trail.DistanceMeters.Value) / 1000.000;
            }

            if (ignorePreviewImages == false)
            {
                if (trail.ElevationProfileImageID.HasValue)
                {
                    vm.ElevationProfileUrl = previewImages.Single(i => i.ID == trail.ElevationProfileImageID.Value).Url;
                }

                if (trail.ElevationProfile_Basic_ImageID.HasValue)
                {
                    vm.ElevationProfile_Basic_Url = previewImages.Single(i => i.ID == trail.ElevationProfile_Basic_ImageID.Value).Url;
                }

                if (trail.ElevationProfile_Intermediate_ImageID.HasValue)
                {
                    vm.ElevationProfile_Intermediate_Url = previewImages.Single(i => i.ID == trail.ElevationProfile_Intermediate_ImageID.Value).Url;
                }

                if (trail.ElevationProfile_Advanced_ImageID.HasValue)
                {
                    vm.ElevationProfile_Advanced_Url = previewImages.Single(i => i.ID == trail.ElevationProfile_Advanced_ImageID.Value).Url;
                }

                if (trail.ElevationProfile_Proficiency_ImageID.HasValue)
                {
                    vm.ElevationProfile_Proficiency_Url = previewImages.Single(i => i.ID == trail.ElevationProfile_Proficiency_ImageID.Value).Url;
                }

                if (trail.PreviewImageID.HasValue)
                {
                    vm.PreviewUrl = previewImages.Single(i => i.ID == trail.PreviewImageID.Value).Url;
                }

                if (trail.SmallPreviewImageID.HasValue)
                {
                    vm.SmallPreviewUrl = previewImages.Single(i => i.ID == trail.SmallPreviewImageID.Value).Url;
                }

                if (trail.MediumPreviewImageID.HasValue)
                {
                    vm.MediumPreviewUrl = previewImages.Single(i => i.ID == trail.MediumPreviewImageID.Value).Url;
                }
            }

            if (trail.TryGetMinimumChallenge(out var level))
            {
                vm.Challenge = level;
            }

            return vm;
        }


        public static TrailNewsViewModel ToTrailNewsViewModel(this Trail trail, LoginViewModel login)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));

            var vm = new TrailNewsViewModel
            {
                Name = trail.Name,
                Description = trail.Description,
                ID = trail.ID,
                //Created = trail.CreatedUtc.ToLocalTime(),
                //Modified = trail.ModifiedUtc,
                //GpxUrl = trail.GpxBlob?.Url,
                //GpxDownloadAccess = trail.GpxDownloadAccess,
                //ListAccess = trail.ListAccess,
                Excerpt = trail.Excerpt, //TODO if taril.Excerpt is empty, try to generate one from description!
                Ascent = trail.AscentMeters,
                Descent = trail.DescentMeters,
            };

            if (trail.StartPlace != null)
            {
                vm.StartPlace = trail.StartPlace.ToPlaceViewModel();
            }

            if (trail.EndPlace != null)
            {
                vm.EndPlace = trail.EndPlace.ToPlaceViewModel();
            }

            vm.Login = login ?? throw new ArgumentNullException(nameof(login));

            if (trail.DistanceMeters.HasValue)
            {
                vm.DistanceKm = Convert.ToDouble(trail.DistanceMeters.Value) / 1000.000;
            }


            return vm;
        }

    }
}
