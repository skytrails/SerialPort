// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlDocument_Support_Utilities
{
    public class Positional_XMLDocument : XmlDocument
    {
        public IXmlLineInfo lineInfo; // a reference to the XmlReader, only set during load time

        public override XmlComment CreateComment(string data)
        {
            return new Positional_XmlComment(data, this, lineInfo);
        }

        /// <summary>
        /// Creates a PositionXmlElement.
        /// </summary>
        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            return new Positional_XmlElement(prefix, localName, namespaceURI, this, lineInfo);
        }

        /// <summary>
        /// Creates a PositionXmlAttribute.
        /// </summary>
        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
        {
            return new Positional_XmlAttribute(prefix, localName, namespaceURI, this, lineInfo);
        }


        /// <summary>
        /// Loads the XML document from the specified <see cref="XmlReader"/>.
        /// </summary>
        public override void Load(XmlReader reader)
        {
            lineInfo = reader as IXmlLineInfo;
            try
            {
                base.Load(reader);
            }
            finally
            {
                lineInfo = null;
            }
        }
    }

    /// <summary>
    /// XML Element with line/column information.
    /// </summary>
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class Positional_XmlElement : XmlElement, IXmlLineInfo
    {
        internal Positional_XmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc, IXmlLineInfo lineInfo)
            : base(prefix, localName, namespaceURI, doc)
        {
            if (lineInfo != null)
            {
                this.lineNumber = lineInfo.LineNumber;
                this.linePosition = lineInfo.LinePosition;
                this.hasLineInfo = true;
            }
        }

        int lineNumber;
        int linePosition;
        bool hasLineInfo;

        /// <summary>
        /// Gets whether the element has line information.
        /// </summary>
        public bool HasLineInfo()
        {
            return hasLineInfo;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber
        {
            get { return lineNumber; }
        }

        /// <summary>
        /// Gets the line position (column).
        /// </summary>
        public int LinePosition
        {
            get { return linePosition; }
        }
    }

    public class Positional_XmlComment : XmlComment, IXmlLineInfo
    {
        internal Positional_XmlComment(string comment, XmlDocument doc, IXmlLineInfo lineInfo)
            : base(comment, doc)
        {
            if (lineInfo != null)
            {
                this.lineNumber = lineInfo.LineNumber;
                this.linePosition = lineInfo.LinePosition;
                this.hasLineInfo = true;
            }
        }

        int lineNumber;
        int linePosition;
        bool hasLineInfo;

        /// <summary>
        /// Gets whether the element has line information.
        /// </summary>
        public bool HasLineInfo()
        {
            return hasLineInfo;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber
        {
            get { return lineNumber; }
        }

        /// <summary>
        /// Gets the line position (column).
        /// </summary>
        public int LinePosition
        {
            get { return linePosition; }
        }
    }

    /// <summary>
    /// XML Attribute with line/column information.
    /// </summary>
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class Positional_XmlAttribute : XmlAttribute, IXmlLineInfo
    {
        internal Positional_XmlAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc, IXmlLineInfo lineInfo)
            : base(prefix, localName, namespaceURI, doc)
        {
            if (lineInfo != null)
            {
                this.lineNumber = lineInfo.LineNumber;
                this.linePosition = lineInfo.LinePosition;
                this.hasLineInfo = true;
            }
        }

        int lineNumber;
        int linePosition;
        bool hasLineInfo;

        /// <summary>
        /// Gets whether the element has line information.
        /// </summary>
        public bool HasLineInfo()
        {
            return hasLineInfo;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber
        {
            get { return lineNumber; }
        }

        /// <summary>
        /// Gets the line position (column).
        /// </summary>
        public int LinePosition
        {
            get { return linePosition; }
        }
    }
}
