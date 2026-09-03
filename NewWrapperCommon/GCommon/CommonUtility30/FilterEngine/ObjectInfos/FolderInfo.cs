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
    using System.Collections;

    #endregion

    /// <summary>
    /// Folder级别信息对象类
    /// </summary>
    public class FolderInfo : CommonInfoBase
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool InheritPermission { get; set; }
        public bool EnableAuditing { get; set; }
        public string ContentType { get; set; }
        public string ContentTypeId { get; set; }
        /// <summary>
        /// Column的信息对
        /// </summary>
        public Hashtable ColumnInfosOfDisplayName { get; set; }
        public Hashtable ColumnInfosOfInternalName { get; set; }
        public Hashtable IntrNameToDispName { get; set; }
        public Hashtable SpecailColumnInfosOfDisplayName { get; set; }

        public Hashtable TermInfosOfDisplayName { get; set; }

        internal override FilterLevel Level
        {
            get { return FilterLevel.Folder; }
        }
    }
}
