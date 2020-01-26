using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Trail365.Entities;

namespace Trail365
{
    public static class ViewDataExtension
    {
        public static readonly string ShowSearchFieldKey = "ShowSearchField";

        public static readonly string OverallChallengeKey = "OverallChallenge";

        public static void AddTrailsLookupValues(this ViewDataDictionary viewData, string key, Guid? selectedValue, params Trail[] trails)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            if (trails == null)
            {
                trails = new Trail[] { }; //params default
            }

            if (selectedValue.HasValue == false)
            {
                selectedValue = Guid.Empty;
            }

            List<Trail> input = new List<Trail>();
            var emptyItem = new Trail
            {
                ID = Guid.Empty,
                Name = "<Nicht definiert>"
            };

            input.Add(emptyItem);
            input.AddRange(trails);

            var list = new SelectList(input, nameof(Trail.ID), nameof(Trail.Name), selectedValue: selectedValue.Value);
            viewData[key] = list;
        }

        public static void CreateLogCategorySelectList(this ViewDataDictionary viewData, string key, IEnumerable<string> categories, string selectedValue = null)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            var items = categories.Select(s => new SelectListItem()
            {
                Value = s,
                Text = s,
                Selected = (s == selectedValue),
            });

            if (!string.IsNullOrEmpty(selectedValue))
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), selectedValue);
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        public static void CreateLogLevelSelectList(this ViewDataDictionary viewData, string key, LogLevel? selectedValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            var items = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().Where(ll => ll != LogLevel.None).Select(s => new SelectListItem()
            {
                Value = ((int)s).ToString(),
                Text = s.ToDescription(),
                Selected = selectedValue.HasValue && selectedValue.Value == s,
            });

            if (selectedValue.HasValue)
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), ((int)selectedValue.Value).ToString());
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        public static void CreateBlockTypeSelectList(this ViewDataDictionary viewData, string key, StoryBlockType? selectedValue)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            var items = Enum.GetValues(typeof(StoryBlockType)).Cast<StoryBlockType>().Select(s => new SelectListItem()
            {
                Value = ((int)s).ToString(),
                Text = s.ToDescription(),
                Selected = selectedValue.HasValue && selectedValue.Value == s,
            });

            if (selectedValue.HasValue)
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), ((int)selectedValue.Value).ToString());
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        public static void CreateEventStatusSelectList(this ViewDataDictionary viewData, string key, EventStatus? selectedValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            var items = Enum.GetValues(typeof(EventStatus)).Cast<EventStatus>().Select(s => new SelectListItem()
            {
                Value = ((int)s).ToString(),
                Text = s.ToDescription(),
                Selected = selectedValue.HasValue && selectedValue.Value == s,
            });

            if (selectedValue.HasValue)
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), ((int)selectedValue.Value).ToString());
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        private static Tuple<string, string, Blob> GetLookup(StoryBlock block)
        {
            string value = block.Image.ID.ToString();
            string name = block.RawContent;

            if (!string.IsNullOrEmpty(block.Image.OriginalFileName))
            {
                name = $"{block.BlockType.ToString()} {block.Image.OriginalFileName}";
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{block.BlockType.ToString()} {value}";
            }

            return new Tuple<string, string, Blob>(value, name, block.Image);
        }

        public static void CreateStoryCoverImageSelectList(this ViewDataDictionary viewData, string key, Story story)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            var images = story.StoryBlocks.Where(sb => sb.ImageID.HasValue && sb.Image != null).Select(sb => GetLookup(sb)).ToList();

            var items = images.Select(s => new SelectListItem()
            {
                Value = s.Item1,
                Text = s.Item2,
                Selected = story.CoverImageID.HasValue && story.CoverImageID.Value == s.Item3.ID,
            }).ToList();

            items.Insert(0, new SelectListItem()
            {
                Value = Guid.Empty.ToString(),
                Text = "<not set>",
                Selected = !story.CoverImageID.HasValue
            });

            Guid selected = Guid.Empty;

            if (story.CoverImageID.HasValue)
            {
                selected = story.CoverImageID.Value;
            }
            viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), selected.ToString());
        }

        public static void CreateStoryStatusSelectList(this ViewDataDictionary viewData, string key, StoryStatus? selectedValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            var items = Enum.GetValues(typeof(StoryStatus)).Cast<StoryStatus>().Select(s => new SelectListItem()
            {
                Value = ((int)s).ToString(),
                Text = s.ToDescription(),
                Selected = selectedValue.HasValue && selectedValue.Value == s,
            });

            if (selectedValue.HasValue)
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), ((int)selectedValue.Value).ToString());
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        public static void CreateAccessLevelSelectList(this ViewDataDictionary viewData, string key, AccessLevel? selectedValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            var items = Enum.GetValues(typeof(AccessLevel)).Cast<AccessLevel>().Select(s => new SelectListItem()
            {
                Value = ((int)s).ToString(),
                Text = s.ToDescription(),
            });

            if (selectedValue.HasValue)
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), selectedValue.Value);
            }
            else
            {
                viewData[key] = new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            }
        }

        public static void AddPlacesLookupValues(this ViewDataDictionary viewData, string key, Guid? selectedValue, params Place[] places)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            if (places == null)
            {
                places = new Place[] { }; //params default
            }

            if (selectedValue.HasValue == false)
            {
                selectedValue = Guid.Empty;
            }

            List<Place> input = new List<Place>();
            var emptyItem = new Place
            {
                ID = Guid.Empty,
                Name = "<Nicht definiert>"
            };

            input.Add(emptyItem);
            input.AddRange(places);

            var list = new SelectList(input, nameof(Place.ID), nameof(Place.Name), selectedValue: selectedValue.Value);
            viewData[key] = list;
        }

        public static void AddTwoLetterIsoCountryCodeLookupValues(this ViewDataDictionary viewData, string key, string selectedValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));

            if (string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = "??";
            }

            List<Tuple<string, string>> items = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("??", "<nicht definiert>")
            };
            items.AddRange(ISO3166.Country.List.Select(c => new Tuple<string, string>(c.TwoLetterCode, c.Name)));

            viewData[key] = new SelectList(items.Select(c => new SelectListItem()
            {
                Value = c.Item1,
                Text = c.Item2,
            }), nameof(SelectListItem.Value), nameof(SelectListItem.Text), selectedValue);
        }

        public static bool SearchFieldIsVisible(this ViewDataDictionary viewData, bool defaultvalue = false)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            if (viewData.TryGetValue(ShowSearchFieldKey, out var v) == false)
            {
                return defaultvalue;
            }
            return bool.Parse(v.ToString());
        }

        public static void AddSearchFieldOption(this ViewDataDictionary viewData, bool showSearchField)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            viewData[ShowSearchFieldKey] = showSearchField;
        }

        public static void AddOverallChallengeOption(this ViewDataDictionary viewData, ChallengeLevel level)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            viewData[OverallChallengeKey] = level;
        }

        public static ChallengeLevel GetOverallChallenge(this ViewDataDictionary viewData)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            if (viewData.TryGetValue(OverallChallengeKey, out var v) == false)
            {
                throw new InvalidOperationException("OverallChallengeKey not found in ViewDictionary");
            }
            return (ChallengeLevel)v;
        }

        public static ChallengeLevel GetOverallChallengeOrDefault(this ViewDataDictionary viewData, ChallengeLevel defaultValue)
        {
            if (viewData == null) throw new ArgumentNullException(nameof(viewData));
            if (viewData.TryGetValue(OverallChallengeKey, out var v) == false)
            {
                return defaultValue;
            }
            return (ChallengeLevel)v;
        }
    }
}
