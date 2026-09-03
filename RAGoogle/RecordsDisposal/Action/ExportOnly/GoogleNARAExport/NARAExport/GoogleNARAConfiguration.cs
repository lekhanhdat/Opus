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
using System.Text;
using System.Xml.Serialization;

namespace RAGoogle
{
    [XmlRoot("GoogleNARAConfig")]
    public class GoogleNARAConfig
    {
        [XmlElement("ColumnMapping")]
        public List<GoogleNARAMetaInfo> MetaInfos { get; set; }
        public virtual string ToXmlString()
        {
            XmlSerializer xs = new XmlSerializer(GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                xs.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static T GetXmlObject<T>(string xml)
        {
            T t;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                object obj = serializer.Deserialize(reader);
                if (obj != null)
                {
                    t = (T)obj;
                }
                else
                {
                    t = default;
                }
            }
            return t;
        }
    }
    public class GoogleNARAMetaInfo
    {
        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; }
        [XmlAttribute("MappedKey")]
        public string MappedKey { get; set; }
        [XmlAttribute("DateFormat")]
        public string DateFormat { get; set; }
        [XmlAttribute("Prefix")]
        public string Prefix { get; set; }
        [XmlAttribute("DefaultValue")]
        public string DefaultValue { get; set; }
        [XmlAttribute("AdditionalMetadata")]
        public bool AdditionalMetadata { get; set; }
    }
}
