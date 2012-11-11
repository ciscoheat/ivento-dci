using System;

namespace Ivento.Dci.Examples.Djikstra.Data
{
    /// <summary>
    /// A node is equal another if they have the same name.
    /// </summary>
    public class Node : IEquatable<Node>
    {
        public string Name { get; set; }

        #region Equality stuff

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public bool Equals(Node other)
        {
            return !ReferenceEquals(other, null) && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Node lhs, Node rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null)) return true;
            return !ReferenceEquals(lhs, null) && lhs.Equals(rhs);
        }

        public static bool operator !=(Node lhs, Node rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
