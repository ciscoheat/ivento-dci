using ImpromptuInterface;

namespace Ivento.Dci
{
    public static class Role
    {
        public static T ActLike<T>(this object rolePlayer) where T : class
        {
            return Impromptu.ActLike<T>(rolePlayer);
        }
    }
}
