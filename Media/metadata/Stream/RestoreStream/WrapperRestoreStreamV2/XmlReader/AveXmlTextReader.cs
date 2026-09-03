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
namespace AvePoint.Metadata;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Util;

public class AveXmlTextReader : XmlReader
{
    protected string TempFolder { get; set; }

    public AveXmlTextReader(XmlReader reader, string cacheDirectory)
    {
        internalReader = reader;
        EnsureTempDirectory(cacheDirectory);
        TempFolder = cacheDirectory;
    }

    public AveXmlTextReader(StreamReader reader, XmlReaderSettings setting, string cacheDirectory)
    {
        internalReader = Create(reader, setting);
        EnsureTempDirectory(cacheDirectory);
        TempFolder = cacheDirectory;
    }

    private static void EnsureTempDirectory(string tempFolder)
    {
        if (!Directory.Exists(tempFolder))
        {
            Directory.CreateDirectory(tempFolder);
        }
    }

    private XmlReader internalReader;

    public override XmlNodeType NodeType => internalReader.NodeType;

    public override string LocalName => internalReader.LocalName;

    public override string NamespaceURI => internalReader.NamespaceURI;

    public override string Prefix => internalReader.Prefix;

    public override string Value => internalReader.Value;

    public override int Depth => internalReader.Depth;

    public override string BaseURI => internalReader.BaseURI;

    public override bool IsEmptyElement => internalReader.IsEmptyElement;

    public override int AttributeCount => internalReader.AttributeCount;

    public override bool EOF => internalReader.EOF;

    public override ReadState ReadState => internalReader.ReadState;

    public override XmlNameTable NameTable => internalReader.NameTable;

    public override void Close()
    {
        internalReader.Close();
    }

    public override string GetAttribute(string name)
    {
        return internalReader.GetAttribute(name);
    }

    public override string GetAttribute(string name, string namespaceURI)
    {
        return internalReader.GetAttribute(name, namespaceURI);
    }

    public override string GetAttribute(int i)
    {
        return internalReader.GetAttribute(i);
    }

    public override string LookupNamespace(string prefix)
    {
        return internalReader.LookupNamespace(prefix);
    }

    public override bool MoveToAttribute(string name)
    {
        return internalReader.MoveToAttribute(name);
    }

    public override bool MoveToAttribute(string name, string ns)
    {
        return internalReader.MoveToAttribute(name, ns);
    }

    public override bool MoveToElement()
    {
        return internalReader.MoveToElement();
    }

    public override bool MoveToFirstAttribute()
    {
        return internalReader.MoveToFirstAttribute();
    }

    public override bool MoveToNextAttribute()
    {
        return internalReader.MoveToNextAttribute();
    }

    public override bool Read()
    {
        return internalReader.Read();
    }

    public override bool ReadAttributeValue()
    {
        return internalReader.ReadAttributeValue();
    }

    public override void ResolveEntity()
    {
        internalReader.ResolveEntity();
    }

    public string ReadXmlToFile(AveMetadataType metadataType)
    {
        if (this.ReadState != ReadState.Interactive)
        {
            return null;
        }
        if (this.NodeType != XmlNodeType.Attribute && this.NodeType != XmlNodeType.Element)
        {
            this.Read();
            return null;
        }
        string path = SecurePath.Combine(TempFolder, metadataType.ToString() + "_" + Guid.NewGuid().ToString());
        using (var writerStream = File.OpenWrite(path))
        using (StreamWriter streamWriter = new StreamWriter(writerStream, Encoding.UTF8))
        using (XmlWriter xmlWriter = CreateWriterForInnerOuterXml(streamWriter))
        {
            try
            {
                if (this.NodeType == XmlNodeType.Attribute)
                {
                    xmlWriter.WriteStartAttribute(this.Prefix, this.LocalName, this.NamespaceURI);
                    this.WriteAttributeValue(xmlWriter);
                    xmlWriter.WriteEndAttribute();
                }
                else
                {
                    xmlWriter.WriteNode(this, false);
                }

            }
            finally
            {
                xmlWriter.Close();
            }
        }
        return path;
    }


    private void WriteAttributeValue(XmlWriter xtw)
    {
        string name = this.Name;
        while (this.ReadAttributeValue())
        {
            if (this.NodeType == XmlNodeType.EntityReference)
            {
                xtw.WriteEntityRef(this.Name);
            }
            else
            {
                xtw.WriteString(this.Value);
            }
        }
        this.MoveToAttribute(name);
    }

    private XmlWriter CreateWriterForInnerOuterXml(TextWriter sw)
    {
        XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
        this.SetNamespacesFlag(xmlTextWriter);
        return xmlTextWriter;
    }

    // System.Xml.XmlReader
    private void SetNamespacesFlag(XmlTextWriter xtw)
    {
        var textReader = internalReader as XmlTextReader;
        if (textReader != null)
        {
            textReader.Namespaces = xtw.Namespaces;
        }
    }
}