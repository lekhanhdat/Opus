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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Web.UI;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveObjectCollectionDeserializer
    {
        private static Dictionary<string, object> ConvertObjectSet(object[] collection)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            int result = 2;
            if (((collection.Length >= 2) && (collection[1] != null)) && !int.TryParse(collection[1].ToString(), out result))
            {
                result = 2;
            }
            ObjectSetIterator iterator = new ObjectSetIterator(collection);
            while (iterator.GetNextSegment())
            {
                if (((iterator.SegType == ObjectSetIterator.SegmentType.PersonalizableProperties) || (iterator.SegType == ObjectSetIterator.SegmentType.NonPersonalizableProperties)) || ((iterator.SegType == ObjectSetIterator.SegmentType.IPersonalizableProperties) || (iterator.SegType == ObjectSetIterator.SegmentType.AttachedProperties)))
                {
                    for (int i = iterator.ObjectCount(); i > 0; i -= 2)
                    {
                        string key = ConvertTokenizedString(iterator.GetNextObject(), result);
                        object nextObject = iterator.GetNextObject();
                        dictionary.Add(key, nextObject);
                    }
                }
                else
                {
                    iterator.SkipSegment();
                }
            }
            return dictionary;
        }

        private static string ConvertTokenizedString(object value, int serializationMinorVersion)
        {
            bool flag = false;
            int num = -1;
            flag = int.TryParse(value.ToString(), out num);
            if (flag)
            {
                return AveWebPartNameTable.GlobalNameTable().LookupPredefinedString(Convert.ToUInt16(num, CultureInfo.InvariantCulture));
            }
            if ((serializationMinorVersion == 2) && (value is IndexedString))
            {
                return ((IndexedString)value).Value;
            }
            return (string)value;
        }

        public static Dictionary<string, object> DeserializeObjectData(byte[] data)
        {
            if ((data == null) || (data.Length == 0))
            {
                return null;
            }
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            MemoryStream inputStream = new MemoryStream(data);
            while ((inputStream.Position < inputStream.Length) && (data[(int)((IntPtr)inputStream.Position)] != 0xff))
            {
                inputStream.ReadByte();
            }
            object[] collection = (object[])formatter.Deserialize(inputStream);
            inputStream.Close();
            return ConvertObjectSet(collection);
        }

        public static void WriteOutProps(Dictionary<string, object> props, XmlTextWriter xmlWriter)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveObjectCollectionDeserializer.WriteOutProps"))
            {

                if (props != null)
                {
                    foreach (string str in props.Keys)
                    {
                        switch (str)
                        {
                            case "FrameType":
                            case "ImportErrorMessage":
                                {
                                    continue;
                                }
                        }
                        object obj2 = props[str];
                        xmlWriter.WriteStartElement("property");
                        xmlWriter.WriteAttributeString("name", str);
                        if (obj2 != null)
                        {
                            xmlWriter.WriteAttributeString("type", obj2.GetType().ToString());
                            xmlWriter.WriteValue(obj2.ToString());
                        }
                        else
                        {
                            xmlWriter.WriteAttributeString("type", typeof(string).ToString());
                            xmlWriter.WriteValue("");
                        }
                        xmlWriter.WriteEndElement();
                    }
                }

            }

        }

        // Nested Types
        private class ObjectSetIterator
        {
            // Fields
            private int idx = 3;
            private int m_count;
            private SegmentType m_type = SegmentType.Unknown;
            private object[] set;

            // Methods
            public ObjectSetIterator(object[] collection)
            {
                this.set = collection;
            }

            private int GetInt(object value)
            {
                return int.Parse(value.ToString());
            }

            internal object GetNextObject()
            {
                return this.set[this.idx++];
            }

            public bool GetNextSegment()
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveObjectCollectionDeserializer.GetNextSegment"))
                {

                    if ((this.set != null) && (this.idx >= this.set.Length))
                    {
                        return false;
                    }
                    this.m_type = (this.set[this.idx] is SegmentType) ? ((SegmentType)((byte)this.GetShort((SegmentType)this.set[this.idx++]))) : ((SegmentType)((byte)this.GetShort(this.set[this.idx++])));
                    while (this.m_type >= SegmentType.Unknown)
                    {
                        this.idx += 1 + this.GetInt(this.set[this.idx]);
                        if (this.idx >= this.set.Length)
                        {
                            return false;
                        }
                        this.m_type = (this.set[this.idx] is SegmentType) ? ((SegmentType)((byte)this.GetShort((SegmentType)this.set[this.idx++]))) : ((SegmentType)((byte)this.GetShort(this.set[this.idx++])));
                    }
                    return true;

                }

            }

            private short GetShort(object value)
            {
                return short.Parse(value.ToString());
            }

            public int ObjectCount()
            {
                this.m_count = this.GetInt(this.set[this.idx++]);
                return this.m_count;
            }

            public void SkipSegment()
            {
                this.idx += 1 + this.GetInt(this.set[this.idx]);
            }

            // Properties
            public SegmentType SegType
            {
                get
                {
                    return this.m_type;
                }
            }

            // Nested Types
            public enum SegmentType : byte
            {
                AttachedProperties = 3,
                IPersonalizableProperties = 2,
                LinkMap = 4,
                NonPersonalizableProperties = 1,
                PersonalizableProperties = 0,
                Unknown = 5
            }
        }
    }
}
