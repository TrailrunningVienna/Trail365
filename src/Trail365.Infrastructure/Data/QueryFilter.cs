using System;
using Microsoft.Extensions.Caching.Memory;
namespace Trail365.Data
{
    public abstract class QueryFilter
    {
        public IMemoryCache Cache { get; set; }
        public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.Zero;
        public virtual string GetCacheKey()
        {
            throw new NotImplementedException();
        }
    }
}
