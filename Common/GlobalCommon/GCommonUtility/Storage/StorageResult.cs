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
    internal class StorageResult
    {
        private static Type StorageResultType = Type.GetType("AvePoint.Media.Storage.StorageResult, Storage");

        #region ConstructorInfos
        private static ConstructorInfo DefaultConstructorInfo = StorageResultType.GetConstructor(new Type[0]);
        #endregion

        #region PropertyInfos
        private static PropertyInfo IsCommittedPropertyInfo = StorageResultType.GetProperty("IsCommited");
        #endregion

        private object target;

        public StorageResult()
        {
            this.target = DefaultConstructorInfo.Invoke(null);
        }

        public StorageResult(object target)
        {
            this.target = target;
        }

        public bool IsCommitted
        {
            get
            {
                return (bool)IsCommittedPropertyInfo.GetValue(target, null);
            }
            set
            {
                IsCommittedPropertyInfo.SetValue(target, value, null);
            }
        }
    }
}
