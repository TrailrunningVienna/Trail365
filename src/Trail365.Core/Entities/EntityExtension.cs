using System;
using System.Collections.Generic;
using System.Linq;

namespace Trail365.Entities
{
    public static class EntityExtension
    {
        public static Role[] GetAllRoleFlags()
        {
            var result = new List<Role>();
            foreach (Role r in Enum.GetValues(typeof(Role)))
            {
                if (r == Role.None) continue;
                result.Add(r);
            }
            return result.ToArray();
        }

        public static AccessLevel[] AllAccessLevels
        {
            get
            {
                var all = System.Enum.GetValues(typeof(AccessLevel)) as AccessLevel[];
                return all;
            }
        }

        public static string[] GetAllRoleNames()
        {
            return GetAllRoleFlags().Select(r => r.ToString()).ToArray();
        }

        public static string[] ToRoleList(this Role value)
        {
            return GetAllRoleFlags().Where(r => value.HasFlag(r)).Select(r => r.ToString()).ToArray();
        }
    }
}
