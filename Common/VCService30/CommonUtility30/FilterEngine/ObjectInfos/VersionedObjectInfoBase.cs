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




namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public class VersionedObjectInfoBase : CommonInfoBase
    {

        /// <summary>
        /// 比当前version大的version数量
        /// </summary>
        public int VersionSequenceNo { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// 比当前version大的major version数量
        /// </summary>
        public int MajorVersionSequenceNo { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// 当前UIVersion
        /// </summary>
        public int UIVersion { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// Item的Approval Status
        /// </summary>
        public bool Approved { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// 当前Version如果在AllDocs表中，则为true
        /// </summary>
        public bool IsCurrentVersion { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        /// <summary>
        /// 当前major version下,大于当前version的minor version数量
        /// </summary>
        public int CurrentMinorVersionSequenceNo { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }

        public bool IsLastMajorVersion { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
    }
}
