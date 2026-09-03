/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class AveCompressedXmlReader : XmlReader
    {             
        private bool _needToPopScope;
        private const int ATTRIBUTE_NIL = -1;
        private ArrayList attributes = new ArrayList();
        private BinaryReader br;
        private int depth;
        private bool eof;
        private byte[] global;
        private int iAttribute = -1;
        private string localName;
        private AveWebPartNameTable nameTable;
        private string ns;
        private XmlNamespaceManager nsManager;
        private byte[] personal;
        private string text;
        private XmlNodeType type;
        private bool usePersonal;

        // Methods
        public AveCompressedXmlReader(XmlNamespaceManager nsManager, byte[] personal, byte[] global)
        {
            this.personal = personal;
            this.global = global;
            this.nameTable = AveWebPartNameTable.GlobalNameTable();
            this.nsManager = nsManager;
            this.SetBinaryReader(personal != null);
        }

        public override void Close()
        {
            this.br.Close();
        }

        public override string GetAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            WebPartXmlAttribute current;
            IEnumerator enumerator = this.attributes.GetEnumerator();
            do
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                current = (WebPartXmlAttribute)enumerator.Current;
            }
            while (!(current.localName == name) || !(current.ns == namespaceURI));
            return current.val;
        }

        public override string LookupNamespace(string prefix)
        {
            return this.nsManager.LookupNamespace(this.nameTable.Get(prefix));
        }

        public override void MoveToAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToElement()
        {
            bool flag = false;
            if (this.iAttribute < 0)
            {
                return flag;
            }
            this.PopToElement();
            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            this.iAttribute = -1;
            if (this.type == XmlNodeType.Element)
            {
                while (this.attributes.Count > 0)
                {
                    this.depth++;
                    this.type = XmlNodeType.Attribute;
                    this.iAttribute = 0;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            switch (this.type)
            {
                case XmlNodeType.Element:
                    return this.MoveToFirstAttribute();

                case XmlNodeType.Attribute:
                    break;

                case XmlNodeType.Text:
                    this.depth--;
                    this.type = XmlNodeType.Attribute;
                    break;

                default:
                    return false;
            }
            if ((this.iAttribute + 1) >= this.attributes.Count)
            {
                return false;
            }
            this.iAttribute++;
            return true;
        }

        private XmlNodeType PeekNodeType()
        {
            return (XmlNodeType)this.br.PeekChar();
        }

        private void PopToElement()
        {
            switch (this.type)
            {
                case XmlNodeType.Attribute:
                    this.depth--;
                    break;

                case XmlNodeType.Text:
                    this.depth -= 2;
                    break;
            }
            this.type = XmlNodeType.Element;
        }

        public override bool Read()
        {
            XmlNodeType type = XmlNodeType.Element;
            if (this.eof)
            {
                return false;
            }
            if (!this._needToPopScope)
            {
                if (this.iAttribute >= 0)
                {
                    this.PopToElement();
                    this.iAttribute = -1;
                    this.attributes.Clear();
                }
            }
            else
            {
                this._needToPopScope = false;
                this.nsManager.PopScope();
            }
            switch (((XmlNodeType)this.br.ReadByte()))
            {
                case XmlNodeType.Element:
                    this.nsManager.PushScope();
                    this.localName = this.ReadPredefinedString();
                    this.ns = this.ReadPredefinedString();
                    if (this.ns.Length > 0)
                    {
                        this.nsManager.AddNamespace(string.Empty, this.ns);
                    }
                    this.text = null;
                    this.depth++;
                    this.ReadAttributes();
                    break;

                case XmlNodeType.Text:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.CDATA:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.EndElement:
                    this.depth--;
                    this._needToPopScope = true;
                    if (this.depth == 0)
                    {
                        this.br = null;
                        if (this.usePersonal && (this.global != null))
                        {
                            this.type = XmlNodeType.None;
                            this.SetBinaryReader(false);
                            this.MoveToContent();
                            this.Read();
                            type = this.type;
                            break;
                        }
                        this.eof = true;
                    }
                    break;
            }
            this.type = type;
            return true;
        }

        private void ReadAttributes()
        {
            this.attributes.Clear();
        Label_000B:
            if (this.PeekNodeType() != XmlNodeType.Attribute)
            {
                this.iAttribute = -1;
            }
            else
            {
                this.br.ReadByte();
                WebPartXmlAttribute attribute2 = new WebPartXmlAttribute();
                attribute2.prefix = this.ReadPredefinedString();
                attribute2.localName = this.ReadPredefinedString();
                attribute2.ns = this.ReadPredefinedString();
                WebPartXmlAttribute attribute = attribute2;
                this.text = null;
                while (this.Read() && (this.type != XmlNodeType.None))
                {
                }
                attribute.val = this.text;
                if (attribute.prefix == "xmlns")
                {
                    this.nsManager.AddNamespace(attribute.localName, attribute.val);
                }
                this.attributes.Add(attribute);
                goto Label_000B;
            }
        }

        public override bool ReadAttributeValue()
        {
            bool flag = false;
            if (this.type != XmlNodeType.Attribute)
            {
                return flag;
            }
            this.depth++;
            this.type = XmlNodeType.Text;
            return true;
        }

        public static string ReadCompressedXML(byte[] personal, byte[] global)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
            XmlDocument document = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
            XmlReader reader = new AveCompressedXmlReader(nsManager, personal, global);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    writer.WriteStartElement(reader.Name, reader.NamespaceURI);
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToNextAttribute();
                        writer.WriteAttributeString(reader.Name, reader.Value);
                    }
                }
                else if (reader.NodeType == XmlNodeType.Attribute)
                {
                    writer.WriteAttributeString(reader.Name, reader.Value);
                }
                else
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        writer.WriteString(reader.Value);
                        continue;
                    }
                    if (reader.NodeType == XmlNodeType.CDATA)
                    {
                        writer.WriteCData(reader.Value);
                        continue;
                    }
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        writer.WriteEndElement();
                    }
                }
            }
            return sb.ToString();
        }

        public override string ReadInnerXml()
        {
            throw new NotImplementedException();
        }

        public override string ReadOuterXml()
        {
            throw new NotImplementedException();
        }

        private string ReadPredefinedString()
        {
            return this.ReadPredefinedString(true);
        }

        private string ReadPredefinedString(bool addToNameTable)
        {
            string predefinedString = null;
            ushort us = this.br.ReadUInt16();
            if (us != 0xffff)
            {
                predefinedString = this.nameTable.LookupPredefinedString(us);
                if (predefinedString != null)
                {
                    return predefinedString;
                }
                if (us == 0x61)
                {
                    return "http://schemas.microsoft.com/WebPart/v2/PivotView";
                }
                if (us != 0x31)
                {
                    return predefinedString;
                }
                return "CaptureMethod";
            }
            if (!addToNameTable)
            {
                return this.br.ReadString();
            }
            return this.nameTable.Add(this.br.ReadString());
        }

        public override string ReadString()
        {
            string str = "";
            while (this.type != XmlNodeType.EndElement)
            {
                if (this.type == XmlNodeType.Text)
                {
                    str = str + this.text;
                }
                if (!this.Read())
                {
                    return str;
                }
            }
            return str;
        }

        public override void ResolveEntity()
        {
            throw new NotImplementedException();
        }

        private void SetBinaryReader(bool usePersonal)
        {
            byte[] personal = this.personal;
            this.usePersonal = usePersonal;
            if (!usePersonal)
            {
                personal = this.global;
            }
            this.br = new BinaryReader(new MemoryStream(personal));
        }

        // Properties
        public override int AttributeCount
        {
            get
            {
                return this.attributes.Count;
            }
        }

        public override string BaseURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override int Depth
        {
            get
            {
                return this.depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.eof;
            }
        }

        public override bool HasValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string LocalName
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.localName;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).localName;
                }
                return null;
            }
        }

        public override string Name
        {
            get
            {
                if (this.Prefix.Length != 0)
                {
                    return (this.Prefix + ":" + this.LocalName);
                }
                return this.LocalName;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                string ns = string.Empty;
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        ns = this.ns;
                        break;

                    case XmlNodeType.Attribute:
                        ns = ((WebPartXmlAttribute)this.attributes[this.iAttribute]).ns;
                        break;
                }
                if (ns.Length != 0)
                {
                    return ns;
                }
                if (this.Prefix.Length <= 0)
                {
                    return this.nsManager.DefaultNamespace;
                }
                return this.LookupNamespace(this.Prefix);
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.type;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.type != XmlNodeType.Attribute)
                {
                    return string.Empty;
                }
                return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ReadState ReadState
        {
            get
            {
                if (!this.eof)
                {
                    return ReadState.Interactive;
                }
                return ReadState.EndOfFile;
            }
        }

        public override string Value
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.text;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;

                    case XmlNodeType.Text:
                        if (this.iAttribute >= 0)
                        {
                            return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;
                        }
                        return this.text;

                    case XmlNodeType.CDATA:
                        if (this.text.StartsWith("<![CDATA[",StringComparison.OrdinalIgnoreCase) && this.text.EndsWith("]]>",StringComparison.OrdinalIgnoreCase))
                        {
                            this.text = this.text.Substring(9, this.text.Length - 12);
                        }
                        return this.text;
                }
                return null;
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // Nested Types
        private class WebPartXmlAttribute
        {
            // Fields
            public string localName;
            public string ns;
            public string prefix;
            public string val;
        }
    }
}
