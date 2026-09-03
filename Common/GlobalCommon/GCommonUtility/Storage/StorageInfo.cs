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
using System.Reflection;

namespace AvePoint.GCommon.Utility.Storage
{
    internal class StorageInfo
    {
        private static Type StorageInfoType = Type.GetType("AvePoint.Media.Storage.StorageInfo, Storage");

        #region ConstructorInfos
        private static ConstructorInfo defaultConstructorInfo = StorageInfoType.GetConstructor(new Type[0]);
        #endregion

        #region MethodInfos

        #endregion

        #region PropertyInfos
        private static PropertyInfo HighNameProperty = StorageInfoType.GetProperty("HighName");

        private static PropertyInfo LowNameProperty = StorageInfoType.GetProperty("LowName");

        private static PropertyInfo LengthProperty = StorageInfoType.GetProperty("Length");
        #endregion

        private object target;
        public object Target
        {
            get
            {
                return target;
            }
        }

        public StorageInfo()
        {
            target = defaultConstructorInfo.Invoke(null);
        }

        public StorageInfo(object target)
        {
            this.target = target;
        }

        public string HighName
        {
            get
            {
                return HighNameProperty.GetValue(target, null) as string;
            }
            set
            {
                HighNameProperty.SetValue(target, value, null);
            }
        }

        public string LowName
        {
            get
            {
                return LowNameProperty.GetValue(target, null) as string;
            }
            set
            {
                LowNameProperty.SetValue(target, value, null);
            }
        }

        public long Length
        {
            get
            {
                return (long)LengthProperty.GetValue(target, null);
            }
            set
            {
                LengthProperty.SetValue(target, value, null);
            }
        }
    }
}
