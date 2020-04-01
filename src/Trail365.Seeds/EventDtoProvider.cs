using System;
using System.Collections.Generic;
using System.Text;
using Trail365.DTOs;
using Trail365.Entities;

namespace Trail365.Seeds
{
    public class EventDtoProvider
    {
        private static string CreateTemplateMarkdown(string scope = null)
        {
            if (string.IsNullOrEmpty(scope))
            {
                scope = "";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"# H1 Header Text {scope}".TrimEnd());
            sb.AppendLine("Default text line");
            sb.AppendLine("## H2 Header Text {scope}".TrimEnd());
            sb.AppendLine("Default text line");
            sb.AppendLine("- List element 1 on root level");
            sb.AppendLine("- List element 2 on root level");
            sb.AppendLine("    - List element 1 on  level 1");
            sb.AppendLine("    - List element 2 on  level 1");
            sb.AppendLine("Default text line");
            sb.AppendLine("### H3 Header Text {scope}".TrimEnd());
            sb.AppendLine("Default text line");
            sb.AppendLine("Default text line with [Link to Google](https://www.google.com).");
            sb.AppendLine("Default text line");
            sb.AppendLine("Default text line with a badge: ![kasd](https://img.shields.io/badge/trail-easy-lightgreen) ");
            sb.AppendLine("");
            sb.AppendLine("### H3 Header after empty row and before table");
            sb.AppendLine("col1|col2|col3");
            sb.AppendLine("---|---|---");
            sb.AppendLine("value1|value2|value3");
            sb.AppendLine("ola|ole|oops");
            return sb.ToString();
        }

        public static EventDtoProvider CreateFromEventDtos(params EventDto[] eventDtos)
        {
            if (eventDtos == null) throw new ArgumentNullException(nameof(eventDtos));
            var p = new EventDtoProvider
            {
                All = eventDtos
            };

            return p;
        }

        public static EventDto NeujahrU4U4()
        {
            EventDto e = new EventDto
            {
                Name = "Neujahrs U4-U4",
                CoverImage = ImageDtoProvider.CreateTGQuer2().AssignNewID(),
            };
            return e;
        }

        public static EventDto CorsaDellaBora()
        {
            EventDto e = new EventDto
            {
                Name = "Trailrunning Vienna goes to Corsa della Bora",
                CoverImage = ImageDtoProvider.CreateLindkogel().AssignNewID(),
            };
            return e;
        }

        public static EventDto IATF2020()
        {
            EventDto e = new EventDto
            {
                ID = new Guid("A0147F0B-7446-4143-B10C-803CC4CC8899"),
                Name = "Trailrunning Vienna goes to IATF 2020",
                Place = new PlaceDto
                {
                    City = "Innsbruck",
                    CountryTwoLetterISOCode = "AT"
                },
                OrganizerPermalink = @"https://innsbruckalpine.at/",
                StartTimeUtc = new DateTime(2020, 04, 30, 0, 0, 0, DateTimeKind.Utc),
                EndTimeUtc = new DateTime(2020, 05, 2, 0, 0, 0, DateTimeKind.Utc),
                Excerpt = "Innsbruck Alpine Trailrun Festival",
                Description = string.Format("Natürlich sind wir in Innsbruck wieder dabei!{0} Dieses Event dient dazu die gemeinsamen Aktivitäten wie Anreise, Hotel, Abendessen etc. zu koordinieren.{0}{0}"
                + "Es wird auch wieder eine Teamwertung geben. Auf der Anmeldung gibt es dazu unter anderem ein Feld 'Verein', dort könnt ihr im Prinzip eintragen was ihr wollt, wichtig für das konkrete Event wäre es im Feld 'Team' 'Trailrunning Vienna' ein zu tragen, daraus ergibt sich die Zugehörigkeit zu dieser Wertung.", Environment.NewLine)
            };
            e.CoverImage = ImageDtoProvider.CreateIATF2020().AssignNewID();
            return e;
        }

        public static EventDto MarkdownDemo()
        {
            EventDto e = new EventDto
            {
                ID = new Guid("37E3923B-BFD7-42C7-B301-5AC2628AB237"),
                Name = "Sample with markdown in excerpt and content",
                Place = new PlaceDto
                {
                    City = "Atlantis",
                    CountryTwoLetterISOCode = "AT"
                },
                OrganizerPermalink = @"http://news.orf.at",
                StartTimeUtc = new DateTime(2020, 04, 30, 0, 0, 0, DateTimeKind.Utc),
                EndTimeUtc = new DateTime(2020, 05, 2, 0, 0, 0, DateTimeKind.Utc),
                CoverImage = ImageDtoProvider.CreateKahlenberg().AssignNewID()
            };
            e.Excerpt = CreateTemplateMarkdown("EXCERPT-FIELD");
            e.Description = CreateTemplateMarkdown("DESCRIPTION-FIELD");
            e.ListAccess = AccessLevel.Public;
            return e;
        }

        public static EventDto VipavaValley()
        {
            EventDto e = new EventDto
            {
                Name = "Trailrunning Vienna goes to UTVV",
                CoverImage = ImageDtoProvider.CreateLindkogel().AssignNewID(),
            };
            return e;
        }

        public static EventDtoProvider CreateTRVEvents2020()
        {
            List<EventDto> list2020 = new List<EventDto>
            {
                NeujahrU4U4(),
                CorsaDellaBora(),
                IATF2020(),
                VipavaValley(),
                MarkdownDemo()
            };
            list2020.ForEach(e => e.ListAccess = AccessLevel.Public);

            var p = new EventDtoProvider
            {
                All = list2020.ToArray()
            };
            return p;
        }

        public static EventDto CreateEventWithExternalSource()
        {
            int i = 9999;
            var startUtc = DateTime.UtcNow;
            var ev1 = new EventDto
            {
                Name = $"Demo Event ##{i}",
                Excerpt = $"Demo Event Excerpt ##{i}" + System.Environment.NewLine + "lorem ipsum second row on Event",
                ListAccess = AccessLevel.Public,
                StartTimeUtc = startUtc,
                EndTimeUtc = startUtc.AddDays(i - 1).AddHours(i),
                ExternalID = "externID",
                ExternalSource = "externSource",
            };
            return ev1;
        }

        public static EventDtoProvider CreateDummyForPublicSeeds(int count)
        {
            if (count < 1) throw new ArgumentNullException(nameof(count));
            List<EventDto> list = new List<EventDto>();

            Random r = new Random();
            int daysToAdd = 0;
            for (int i = 1; i < count; i++)
            {
                string sequence = DateTime.Now.Ticks.ToString().PadLeft(12, '0').Substring(4);
                int time = r.Next(0, 48);
                int hour = Convert.ToInt32(Math.Floor((double)time / 2));
                int minute = 30 * (time % 2);

                daysToAdd += r.Next(0, 2);

                var startUtc = DateTime.UtcNow.Date.AddDays(daysToAdd).AddHours(hour).AddMinutes(minute);
                var duration = TimeSpan.FromHours(r.Next(2, 72) / 2);
                var ev1 = new EventDto
                {
                    Name = TextSeedingHelper.GetNameDummy(),
                    Excerpt = TextSeedingHelper.GetExcerptDummy(),
                    ListAccess = AccessLevel.Public,
                    StartTimeUtc = startUtc,
                    EndTimeUtc = startUtc.Add(duration),
                    ExternalID = $"externID{i:000000}",
                    ExternalSource = "externSource",
                };
                list.Add(ev1);
            }

            var p = new EventDtoProvider
            {
                All = list.ToArray()
            };
            return p;
        }

        public EventDto[] All { get; private set; }
    }
}
