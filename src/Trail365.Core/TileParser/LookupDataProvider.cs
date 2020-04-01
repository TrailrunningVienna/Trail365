using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Trail365
{
    // API Design: NO async patterns on the public surface, assuming that this is called isnide web-requests and there is a higher level task management running
    // this class may be also running as a worker where the engine is handling concurrency and the code can be written in old sync way
    // more precise: no async on the public API but maybe some (transparent) async inside the implementation specially regarding web requests

    public abstract class LookupDataProvider
    {
        /// <summary>
        /// returns a featureCollection with all known ways and classifications inside the given envelope!
        /// </summary>
        /// <param name="envelop"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetClassifiedMapFeatures(Geometry envelope);
    }
}
