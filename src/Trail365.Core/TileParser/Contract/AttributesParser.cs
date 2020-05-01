using System.Collections.Generic;
using System.Linq;

namespace Trail365.TileParser.Contract
{
    public static class AttributesParser
    {
        public static List<KeyValuePair<string, object>> Parse(List<string> keys, List<TileValue> values, List<uint> tags)
        {
            var result = new List<KeyValuePair<string, object>>();
            var odds = tags.GetOdds().ToList();
            var evens = tags.GetEvens().ToList();

            for (var i = 0; i < evens.Count; i++)
            {
                var key = keys[(int)evens[i]];
                var val = values[(int)odds[i]];
                var valObject = GetAttribute(val);
                result.Add(new KeyValuePair<string, object>(key, valObject));
            }
            return result;
        }

        private static object GetAttribute(TileValue value)
        {
            object res = null;

            if (value.HasBoolValue)
            {
                res = value.BoolValue;
            }
            else if (value.HasDoubleValue)
            {
                res = value.DoubleValue;
            }
            else if (value.HasFloatValue)
            {
                res = value.FloatValue;
            }
            else if (value.HasIntValue)
            {
                res = value.IntValue;
            }
            else if (value.HasStringValue)
            {
                res = value.StringValue;
            }
            else if (value.HasSIntValue)
            {
                res = value.SintValue;
            }
            else if (value.HasUIntValue)
            {
                res = value.UintValue;
            }
            return res;
        }
    }
}