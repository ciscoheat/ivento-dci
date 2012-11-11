using System;
using System.Collections.Generic;
using ImpromptuInterface;

namespace Ivento.Dci
{
    public static class Role
    {
        /// <summary>
        /// Make an object act like a Role.
        /// </summary>
        /// <typeparam name="T">Role Contract for the Role.</typeparam>
        /// <param name="obj">Object that will play the Role.</param>
        /// <returns>The object, duck typed to the RoleContract type T.</returns>
        /// <remarks>
        /// The object returned will have its own type, so to get the original 
        /// object use Role.IsA[T]
        /// </remarks>
        public static T ActLike<T>(this object obj) where T : class
        {
            return Impromptu.ActLike<T>(obj);
        }

        /// <summary>
        /// Make all objects in an IEnumerable act like a Role.
        /// </summary>
        /// <typeparam name="T">Role Contract for the Role.</typeparam>
        /// <param name="objects">Enumerable objects that will play the Role.</param>
        /// <returns>An IEnumerable with all objects acting implementing the Role Contract.</returns>
        public static IEnumerable<T> AllActLike<T>(this IEnumerable<object> objects) where T : class
        {
            return Impromptu.AllActLike<T>(objects);
        }

        /// <summary>
        /// Cast a RolePlayer back to its original type.
        /// </summary>
        /// <typeparam name="T">Original type for the object.</typeparam>
        /// <param name="rolePlayer">The Roleplaying object.</param>
        /// <remarks>
        /// Because ActLike creates a whole new type, this method is needed for 
        /// conversion back to the original type.
        /// </remarks>
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
