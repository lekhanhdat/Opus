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
using System.Reflection;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    abstract class AveClientObject
    {
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientObject));
        private AveClientObjectData m_DataCache;
        protected bool m_ExtraPropertiesInited = false;        

        protected AveClientObject()
        {
            m_DataCache = new AveClientObjectData();            
        }

        protected AveClientObject(bool ingoreCase)
        {
            m_DataCache = new AveClientObjectData(ingoreCase);
        }

        public AveClientObjectData DataCache
        {
            get { return m_DataCache; }
            internal set { m_DataCache = value; }
        }
        public void CopyObjectAve(object dest, object src, string[] sProp, string[] dProp)
        {
            if (null == src) { throw new ArgumentNullException("source can't be null"); }
            if (null == dest) { throw new ArgumentNullException("destination can't be null"); }

            Type srcType = src.GetType();
            Type destType = dest.GetType();

            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            PropertyInfo[] destInfo = destType.GetProperties(bindingFlags);

            FieldInfo srcInfo = null;
            PropertyInfo destIndexInfo = null;
            for (int j = 0; j < destInfo.Length; j++)
            {
                string targetName = destInfo[j].Name;
                srcInfo = srcType.GetField(targetName, bindingFlags);
                if (srcInfo != null)
                {
                    destIndexInfo = destInfo[j];
                    if ((ContainsInEscapes(dProp, srcInfo.Name) && DataCache.PropertyAvailable(targetName)))
                    {
                        if (destIndexInfo.CanWrite)
                        {
                            CopyAve(destIndexInfo, dest, srcInfo, src, null, dProp);
                        }
                    }
                    else if (ContainsInEscapes(sProp, srcInfo.Name) && destIndexInfo.CanWrite)
                    {
                        CopyAve(destIndexInfo, dest, srcInfo, src, sProp, null);
                    }
                }
            }
        }
        private void CopyAve(PropertyInfo destInfo, object dest, FieldInfo srcInfo, object src, string[] sProp, string[] dProp)
        {
            try
            {
                object srcValue = srcInfo.GetValue(src);
                if (srcValue != null)
                {
                    object targetValue = null;
                    if (destInfo.PropertyType.IsEnum)
                    {
                        this.ConvertNEnum(out targetValue, destInfo.PropertyType, srcValue);
                    }
                    //else if (destInfo.PropertyType != srcInfo.FieldType && targetValue != null && srcValue != null)
                    //{
                    //    CopyObjectFP(targetValue, srcValue, sProp, dProp);
                    //}
                    else
                    {
                        targetValue = srcValue;
                    }
                    destInfo.SetValue(dest, targetValue, null);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("An Error occurred when copy properties in CopyAve. \n{0}", ex.ToString());
            }
        }
        public void CopyObjectSetting(object dest, object src, string[] sProp, string[] dProp)
        {
            if (null == src) { throw new ArgumentNullException("source can't be null"); }
            if (null == dest) { throw new ArgumentNullException("destination can't be null"); }

            Type srcType = src.GetType();
            Type destType = dest.GetType();

            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            FieldInfo[] destInfo = destType.GetFields(bindingFlags);
            PropertyInfo srcInfo = null;
            FieldInfo destIndexInfo = null;

            for (int j = 0; j < destInfo.Length; j++)
            {
                destIndexInfo = destInfo[j];
                string targetName = destIndexInfo.Name;
                srcInfo = srcType.GetProperty(targetName, bindingFlags);
                if (srcInfo != null)
                {
                    if (ContainsInEscapes(dProp, srcInfo.Name) && DataCache.PropertyAvailable(srcInfo.Name))
                    {
                        CopySetting(destIndexInfo, dest, srcInfo, src, null, dProp);
                    }
                    else if (ContainsInEscapes(sProp, srcInfo.Name))
                    {
                        CopySetting(destIndexInfo, dest, srcInfo, src, sProp, null);
                    }
                }
            }
        }
        public bool ContainsInEscapes(string[] escapes, string fieldName)
        {
            return escapes != null && Array.Exists(escapes, new Predicate<string>(e => string.Equals(e, fieldName, StringComparison.OrdinalIgnoreCase)));
        }
        private void CopySetting(FieldInfo destInfo, object dest, PropertyInfo srcInfo, object src, string[] sProp, string[] dProp)
        {
            try
            {
                object srcValue = srcInfo.GetGetMethod().Invoke(src, null);
                if (srcValue != null)
                {
                    object targetValue = null;
                    if (srcInfo.PropertyType.IsEnum)
                    {
                        this.ConvertNEnum(out targetValue, destInfo.FieldType, srcValue);
                    }
                    //else if (destInfo.FieldType != srcInfo.PropertyType && targetValue != null && srcValue != null)
                    //{
                    //    CopyObjectPF(targetValue, srcValue, sProp, dProp);
                    //}
                    else
                    {
                        targetValue = srcValue;
                    }

                    destInfo.SetValue(dest, targetValue);
                }

            }
            catch (Exception ex)
            {
                mLogger.Warn("An Error occurred when copy settings in CopySetting. \n{0}", ex.ToString());
            }
        }
        private void ConvertNEnum(out object dest, Type destType, object src)
        {
            if (null == src)
            {
                throw new ArgumentNullException("one argument is null!");
            }
            Type underlyingType = Enum.GetUnderlyingType(src.GetType());
            dest = Convert.ChangeType(src, underlyingType);
        }

        //public virtual void Dispose()
        //{
        //    m_DataCache = null;
        //    m_ExtraPropertiesInited = false;
        //}
    }
}
