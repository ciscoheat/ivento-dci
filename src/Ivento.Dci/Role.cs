using System;
using System.Collections.Generic;
using ImpromptuInterface;

namespace Ivento.Dci
{
    public static class Role
    {
        public static T ActLike<T>(this object obj) where T : class
        {
            return Impromptu.ActLike<T>(obj);
        }

        public static IEnumerable<T> AllActLike<T>(this IEnumerable<object> objects) where T : class
        {
            return Impromptu.AllActLike<T>(objects);
        }

        public static T IsA<T>(this object rolePlayer) where T : class
        {
            var output = rolePlayer as IActLikeProxy;

            if (output == null)
                throw new ArgumentException(string.Format("{0} is not a RolePlayer using Role.ActLike.", rolePlayer));

            // Since roles can play roles, need to step up from all ActLike calls
            // before finding the original object.
            while (output.Original is IActLikeProxy)
            {
                output = output.Original as IActLikeProxy;
            }

            try
            {
                return (T)output.Original;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(string.Format("Expected RolePlayer to be {0} but was {1}", typeof(T),
                                                          output.Original.GetType()), e);
            }
        }
    }
}
