using System;
using System.Collections.Generic;
using Trail365.Entities;

namespace Trail365.ViewModels
{
    public class EventBackendIndexViewModel : DatapagerViewModel
    {
        public List<EventBackendViewModel> Events { get; set; }

        /// <summary>
        /// rowcount without skip/take
        /// </summary>
        public EventStatus? Status { get; set; }

        public string SearchText { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}
