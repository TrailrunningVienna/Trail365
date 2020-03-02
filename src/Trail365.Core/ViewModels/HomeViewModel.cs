using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class HomeViewModel
    {
        //public TrailCollectionViewModel ToCollection()
        //{
        //    return new TrailCollectionViewModel
        //    {
        //        Items = this.IndexTrails,
        //    };
        //}

        [HiddenInput]
        public LoginViewModel Login { get; set; } = new LoginViewModel();

        public List<TrailViewModel> IndexTrails { get; set; } = new List<TrailViewModel>();

        public string SearchText { get; set; }

        public bool HasSearchText()
        {
            return !string.IsNullOrEmpty(this.SearchText);
        }

        private static readonly char[] SearchSplitDelimiter = new char[] { ' ' };

        public ChallengeLevel GetOverallChallengeLevel()
        {
            if (this.IndexTrails.Count == 0)
            {
                return ChallengeLevel.Basic;
            }
            return this.IndexTrails.Max(it => it.Challenge);
        }

        public string[] GetSearchTextSplitsInLowerCase()
        {
            if (!this.HasSearchText()) return new string[] { };
            var result = this.SearchText.Split(SearchSplitDelimiter, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).Distinct();
            return result.ToArray();
        }
    }
}
