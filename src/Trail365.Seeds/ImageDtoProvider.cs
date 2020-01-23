using System;
using System.IO;
using Trail365.DTOs;
using Trail365.Internal;

namespace Trail365.Seeds
{
    public class ImageDtoProvider
    {
        //WM 14.09.2019 we have some test that is writing/changing the dto => it should NEVER be shared so ensure that each call to "All" creates new instances!
        public BlobDto[] All
        {
            get
            {
                Guard.AssertNotNull(this.Kahlenberg);
                Guard.AssertNotNull(this.Lindkogel);
                return new BlobDto[] { this.Kahlenberg, this.Lindkogel };
            }
        }

        public static ImageDtoProvider CreateInstance()
        {
            var p = new ImageDtoProvider
            {
                Lindkogel = CreateLindkogel(),
                Kahlenberg = CreateKahlenberg(),
            };

            return p;
        }

        public BlobDto Lindkogel { get; private set; }
        public BlobDto Kahlenberg { get; private set; }

        public static BlobDto CreateLindkogel() => new BlobDto { ID = new Guid("51702F9F-64EF-4DE4-8E67-390B86A042FE"), Data = File.ReadAllBytes(Images.LindkogelAsJpg), SubFolder = "jpg", MimeType = SupportedMimeType.ImageJpg };

        public static BlobDto CreateKahlenberg() => new BlobDto { ID = new Guid("14991F31-8193-43A4-9533-50636A6D0348"), Data = File.ReadAllBytes(Images.KahlenbergAsPng), SubFolder = "png", MimeType = SupportedMimeType.ImagePng };

        public static BlobDto CreateTGHoch() => new BlobDto { ID = new Guid("87EFE47E-2DEA-434E-AE9F-59E3942445D2"), Data = File.ReadAllBytes(Images.TGHochPath), SubFolder = "jpg", MimeType = SupportedMimeType.ImageJpg };

        public static BlobDto CreateTGQuer1() => new BlobDto { ID = new Guid("41BB4AE5-0364-4B62-AB56-F05875ED2D1C"), Data = File.ReadAllBytes(Images.TGQuer1Path), SubFolder = "jpg", MimeType = SupportedMimeType.ImageJpg };

        public static BlobDto CreateTGQuer2() => new BlobDto { ID = new Guid("9849119F-71F7-42CA-8004-E33359AB8E90"), Data = File.ReadAllBytes(Images.TGQuer2PathAsJpg), SubFolder = "jpg", MimeType = SupportedMimeType.ImageJpg };

        public static BlobDto CreateIATF2020() => new BlobDto { ID = new Guid("F35C209E-D3A0-449A-8FD4-950F939B8173"), Data = File.ReadAllBytes(Images.IATF2020AsJpg), SubFolder = "jpg", MimeType = SupportedMimeType.ImageJpg };
    }
}
