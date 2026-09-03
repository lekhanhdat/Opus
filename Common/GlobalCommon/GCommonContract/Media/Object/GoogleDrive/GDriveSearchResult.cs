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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directive
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Tree;
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;
    #endregion

    public class GDriveSearchResult
    {
        public List<GoogleDriveTreeNodeDto> NodeList { get; set; }
        /// <summary>
        /// Job Id for TimeBaseSearch, cycle id for ObjectBaseSearch
        /// </summary>
        public String BackupJobId { get; set; }
        public AdvanceSearchType SearchType { get; set; }

        public override string ToString()
        {
            return String.Format("Backup Job Id: {0}, Search Type: {1}",
                this.BackupJobId,
                this.SearchType);
        }
    }
}
