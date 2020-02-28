using System;

namespace Trail365.ViewModels
{
    public class TrailViewModelBase
    {
        public LoginViewModel Login { get; set; } = new LoginViewModel();


        public string Excerpt { get; set; }

        public double? DistanceKm { get; set; }

        public int? Ascent { get; set; }
        public int? Descent { get; set; }

        public string Description { get; set; }


        public Guid ID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

    }
}
