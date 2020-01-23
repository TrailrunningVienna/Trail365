using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trail365.Data;
using Trail365.Entities;

namespace Trail365.Web
{
    public abstract class EventImporter<T>
    {
        public readonly DateTime UtcNow = DateTime.UtcNow;

        protected readonly TrailContext DbContext;

        protected readonly IUrlHelper Url;

        public EventImporter(TrailContext context, IUrlHelper helper)
        {
            this.DbContext = context ?? throw new ArgumentNullException(nameof(context));
            this.Url = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public List<Event> UpdatedEvents { get; private set; } = new List<Event>();
        public List<Event> InsertedEvents { get; private set; } = new List<Event>();
        public List<Place> InsertedPlaces { get; private set; } = new List<Place>();
        public List<Place> UpdatedPlaces { get; private set; } = new List<Place>();

        public List<Blob> InsertedImages { get; private set; } = new List<Blob>();

        public abstract Task Import(T data, ILogger logger, CancellationToken cancellationToken = default(CancellationToken));
    }
}
