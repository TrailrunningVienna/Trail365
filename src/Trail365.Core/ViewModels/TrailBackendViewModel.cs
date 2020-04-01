using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class TrailBackendViewModel
    {
        public Trail ApplyChangesTo(Trail trail)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));
            trail.Description = this.Description;
            trail.InternalDescription = this.InternalDescription;
            trail.Name = this.Name;
            trail.ListAccess = this.ReadAccess;
            trail.GpxDownloadAccess = this.DownloadAccess;
            trail.Excerpt = this.Excerpt;
            trail.DescentMeters = this.DescentMeters;
            trail.AscentMeters = this.AscentMeters;
            trail.DistanceMeters = this.DistanceMeters;
            trail.ModifiedUtc = DateTime.UtcNow;
            trail.ExternalID = this.ExternalID;
            trail.ExternalSource = this.ExternalSource;
            trail.StartPlaceID = this.StartPlaceID;
            trail.EndPlaceID = this.EndPlaceID;

            if (trail.StartPlaceID.Value == Guid.Empty)
            {
                trail.StartPlaceID = null;
            }

            if (trail.EndPlaceID.Value == Guid.Empty)
            {
                trail.EndPlaceID = null;
            }
            return trail;
        }

        public static TrailBackendViewModel CreateFromTrail(Trail from)
        {
            var vm = new TrailBackendViewModel
            {
                ID = from.ID,
                Name = from.Name,
                Created = from.CreatedUtc.ToLocalTime(),
                Description = from.Description,
                InternalDescription = from.InternalDescription,
                ReadAccess = from.ListAccess,
                DownloadAccess = from.GpxDownloadAccess,
                Excerpt = from.Excerpt,
                DistanceMeters = from.DistanceMeters,
                AscentMeters = from.AscentMeters,
                DescentMeters = from.DescentMeters,
                ExternalID = from.ExternalID,
                ExternalSource = from.ExternalSource,
                StartPlaceID = from.StartPlaceID ?? Guid.Empty,
                EndPlaceID = from.EndPlaceID ?? Guid.Empty
            };
            return vm;
        }

        public static TrailBackendViewModel CreateFromTrail(Trail from, Blob[] previewImages)
        {
            var vm = CreateFromTrail(from);


            if (from.GpxBlobID.HasValue)
            {
                vm.GpxUrl = from.GpxBlob.Url;
            }

            if (from.AnalyzerBlobID.HasValue)
            {
                vm.AnalysisUrl = from.AnalyzerBlob.Url;
            }

            if (from.PreviewImageID.HasValue)
            {
                vm.PreviewUrl = previewImages.Single(i => i.ID == from.PreviewImageID.Value).Url;
            }

            if (from.SmallPreviewImageID.HasValue)
            {
                vm.SmallPreviewUrl = previewImages.Single(i => i.ID == from.SmallPreviewImageID.Value).Url;
            }

            if (from.MediumPreviewImageID.HasValue)
            {
                vm.MediumPreviewUrl = previewImages.Single(i => i.ID == from.MediumPreviewImageID.Value).Url;
            }

            if (from.ElevationProfileImageID.HasValue)
            {
                vm.ElevationProfileUrl = previewImages.Single(i => i.ID == from.ElevationProfileImageID.Value).Url;
            }

            if (from.ElevationProfile_Basic_ImageID.HasValue)
            {
                vm.ElevationProfile_Basic_Url = previewImages.Single(i => i.ID == from.ElevationProfile_Basic_ImageID.Value).Url;
            }

            if (from.ElevationProfile_Intermediate_ImageID.HasValue)
            {
                vm.ElevationProfile_Intermediate_Url = previewImages.Single(i => i.ID == from.ElevationProfile_Intermediate_ImageID.Value).Url;
            }

            if (from.ElevationProfile_Advanced_ImageID.HasValue)
            {
                vm.ElevationProfile_Advanced_Url = previewImages.Single(i => i.ID == from.ElevationProfile_Advanced_ImageID.Value).Url;
            }

            if (from.ElevationProfile_Proficiency_ImageID.HasValue)
            {
                vm.ElevationProfile_Proficiency_Url = previewImages.Single(i => i.ID == from.ElevationProfile_Proficiency_ImageID.Value).Url;
            }

            return vm;
        }

        [Display(Name = "Start (Ort)")]
        public Guid StartPlaceID { get; set; } = Guid.Empty;

        [Display(Name = "Ende (Ort)")]
        public Guid EndPlaceID { get; set; } = Guid.Empty;

        public Guid ID { get; set; }

        public string Name { get; set; }


        [Display(Name = "Gpx Url")]
        public string GpxUrl { get; set; }


        [Display(Name = "Analysis Url")]
        public string AnalysisUrl { get; set; }


        [Display(Name = "Preview Url")]
        public string PreviewUrl { get; set; }

        public string SmallPreviewUrl { get; set; }

        public string MediumPreviewUrl { get; set; }

        public DateTime? Created { get; set; }

        public string Description { get; set; }

        public string InternalDescription { get; set; }

        public string Excerpt { get; set; }

        public AccessLevel ReadAccess { get; set; }

        public AccessLevel DownloadAccess { get; set; }

        [Display(Name = "Distanz")]
        public int? DistanceMeters { get; set; }

        [Display(Name = "Aufsteigend")]
        public int? AscentMeters { get; set; }

        [Display(Name = "Abfallend")]
        public int? DescentMeters { get; set; }

        [Display(Name = "Höhenprofil (Allgemein)")]
        public string ElevationProfileUrl { get; set; }

        [Display(Name = "Höhenprofil (Basic)")]
        public string ElevationProfile_Basic_Url { get; set; }

        [Display(Name = "Höhenprofil (Intermediate)")]
        public string ElevationProfile_Intermediate_Url { get; set; }

        [Display(Name = "Höhenprofil (Advanced)")]
        public string ElevationProfile_Advanced_Url { get; set; }

        [Display(Name = "Höhenprofil (Profi)")]
        public string ElevationProfile_Proficiency_Url { get; set; }

        [Display(Name = "ID (extern)")]
        public string ExternalID { get; set; }

        [Display(Name = "Quelle (extern)")]
        public string ExternalSource { get; set; }
    }
}
