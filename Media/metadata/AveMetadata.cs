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

namespace AvePoint.Metadata
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    public class AveMetadata
    {
        protected XmlElement mXmlElement;
        protected AveMetadataType mMetadataType;

        public AveMetadata()
        { }

        public AveMetadata(XmlElement xmlElement)
        {
            mXmlElement = xmlElement;
            string sname = mXmlElement.GetAttribute(AveMetadataConstants.COLUMN_NAME);
            if (string.IsNullOrEmpty(sname))
            {
                mMetadataType = AveMetadataType.Unknown;
            }
            else
            {
                try
                {
                    mMetadataType = (AveMetadataType)Enum.Parse(typeof(AveMetadataType), sname, true);
                }
                catch (ArgumentException)
                {
                    mMetadataType = AveMetadataType.Unknown;
                }
            }
        }

        //[Obsolete("will be removed later")]
        public virtual XmlElement XmlElement
        {
            get { return mXmlElement; }
        }

        public virtual AveMetadataType MetadataType
        {
            get { return mMetadataType; }
        }

        public virtual object GetMetadataObject()
        {
            return AveXmlSerializer.Deserialize(mXmlElement);
        }

        public virtual T GetMetadata<T>()
        {
            return (T)AveXmlSerializer.Deserialize(mXmlElement, typeof(T));
        }

        [Obsolete("will be removed later")]
        public virtual void GetMetadata(object value)
        {
            AveXmlSerializer.Deserialize(mXmlElement, value);
        }

        public virtual void GetMetadata(IDictionary dictionary)
        {
            AveXmlSerializer.Deserialize(mXmlElement, dictionary);
        }

        //only for AveSiteInfo class test
        public static object GetMetadataFromHT(string className, Hashtable ht)
        {
            object obj = null;

            Assembly assembly = Assembly.GetAssembly(typeof(AveMetadata));
            Type type = Type.GetType("AvePoint.Wrapper.Common" + className);
            ConstructorInfo cons = type.GetConstructors()[0];
            obj = cons.Invoke(null);

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField);

            foreach (string key in ht.Keys)
            {
                string temp = key;
                if (key.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    temp = key.Substring(1);
                }
                FieldInfo field = type.GetField(temp);
                field.SetValue(obj, ht[key]);
            }

            return obj;
        }

        public static object GetMetadataFromHT(string className, Hashtable ht, Hashtable htMapping)
        {
            object obj = null;

            return obj;
        }
    }
}