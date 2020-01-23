using System;
using Trail365.Internal;

namespace Trail365.ViewModels
{
    /// <summary>
    /// News means that we provide the following information to the user:
    /// - what happened: event added, event canceled, story added, story significant modification, trail added, trail significant modification, general message
    /// - when: how old
    /// - title of the event, story, trail
    /// - image/preview
    /// </summary>
    public class NewsViewModel
    {
        public EventViewModel EventItem { get; set; }

        public NewsViewModel()
        { }

        public NewsViewModel(Guid originID, string itemType, string itemName, DateTime createdUtc, DateTime? modifiedUtc) : this(originID, itemType, itemName, createdUtc, modifiedUtc, null, 0)
        {
        }

        public NewsViewModel(Guid originID, string itemType, string itemName, DateTime createdUtc, DateTime? modifiedUtc, string template, int priority)
        {
            this.OriginID = originID;
            this.ItemType = itemType;
            this.ItemName = itemName;
            this.Time = createdUtc.ToLocalTime();
            this.Priority = priority;

            if (modifiedUtc.HasValue)
            {
                this.Time = modifiedUtc.Value.ToLocalTime();
            }
            string ht = this.GetHumanizedTime();

            Guard.AssertNotNullOrEmptyString(ht);

            if (!string.IsNullOrEmpty(template))
            {
                this.ItemAction = string.Format(template, ht);
                return;
            }

            this.ItemAction = $"wurde {ht} angelegt";

            if (modifiedUtc.HasValue)
            {
                if (modifiedUtc.Value.Subtract(createdUtc).TotalDays > 1.5)
                {
                    this.ItemAction = $"wurde {ht} geändert";
                }
            }
        }

        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemAction { get; set; }

        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? OriginID { get; set; } = null;

        public string ImageUrl { get; set; }

        /// <summary>
        /// LocalTime (Model => ViewModel includes utc to localtime.
        /// StartTime used for wording (ignoring end-time) and sorting!
        /// </summary>
        public string Location { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public string GetHumanizedTime(bool includeTimestamp = false)
        {
            string ts = this.Time.ToShortTimeString();

            if (this.Time.Date == DateTime.Now.Date)
            {
                if (includeTimestamp)
                {
                    return $"heute um {ts}";
                }
                else
                {
                    return "heute";
                }
            }

            if (this.Time.Date == DateTime.Now.AddDays(-1).Date)
            {
                if (includeTimestamp)
                {
                    return $"gestern um {ts}";
                }
                else
                {
                    return "gestern";
                }
            }

            var diff = DateTime.Now.Subtract(this.Time);

            if (diff.TotalSeconds < 0)
            {
                //future
                if (diff.TotalDays > -6)
                {
                    string wt = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(this.Time.DayOfWeek);
                    if (includeTimestamp)
                    {
                        return $"{wt} {ts}";
                    }
                    else
                    {
                        return $"kommenden {wt}";
                    }
                }
                else
                {
                    if (includeTimestamp)
                    {
                        return $"{this.Time.ToShortDateString()} {ts}";
                    }
                    else
                    {
                        if (-diff.TotalDays > 9)
                        {
                            return $"am {this.Time.ToShortDateString()}";
                        }
                        else
                        {
                            return $"in {Convert.ToInt32(-diff.TotalDays)} Tagen";
                        }
                    }
                }
            }
            else
            {
                //past
                if (diff.TotalDays < 6)
                {
                    string wt = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName(this.Time.DayOfWeek);
                    if (includeTimestamp)
                    {
                        return $"{wt} {ts}";
                    }
                    else
                    {
                        return $"vergangenen {wt}";
                    }
                }

                if (includeTimestamp)
                {
                    return $"{this.Time.ToShortDateString()} {ts}";
                }
                else
                {
                    return $"vor {Convert.ToInt32(diff.TotalDays)} Tagen";
                }
            }
        }

        public string HumanizedAge
        {
            get
            {
                //this.Time can be in the future and means event-timing... then the result does not make sense!
                var ts = DateTime.Now.Subtract(this.Time);
                if (ts.TotalDays > 1)
                {
                    return $"{Convert.ToInt32(ts.TotalDays).ToString()} Tg.";
                }
                else
                {
                    if (ts.TotalHours > 1)
                    {
                        return $"{Convert.ToInt32(ts.TotalHours).ToString()} Std.";
                    }
                    else
                    {
                        return $"{Convert.ToInt32(ts.TotalMinutes).ToString()} Min.";
                    }
                }
            }
        }

        public string DetailsUrl { get; set; }

        public int Priority { get; set; } = 0;
    }
}
