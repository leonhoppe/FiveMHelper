using System.Xml;

namespace MloFinder {
    public static class XmlNodeExtensions {

        public static XmlNode FindChild(this XmlNode parent, string name) {
            foreach (XmlNode child in parent.ChildNodes) {
                if (child.Name.Equals(name))
                    return child;
            }

            return null;
        }
        
    }
}