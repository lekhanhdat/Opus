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




namespace AvePoint.Media.Service.DomainModel
{
    using AvePoint.GCommon.Contract.Tree;
    #region using directives

    using AvePoint.GCommon.Contract.Tree.Object;
    using System;

    #endregion using directives

    public class TreeNodeParameter
    {
        public Boolean IsJustCalculateCount { get; set; }

        public Boolean IsPreview { get; set; }

        public SPTreeNodeDto CurrentTree { get; set; }

        public ExchangeOnlineTreeNodeDto ExchangeTree { get; set; }

        public GoogleDriveTreeNodeDto GoogleDriveTree { get; set; }
        public RestoreJobBase RestoreJob { get; set; }

        public override string ToString()
        {
            return string.Format("TreeNodeParameter : IsJustCalculateCount : {0}, IsPreview: {1}, CurrentTree: {2}, RestoreJob: {3}",
               IsJustCalculateCount, IsPreview, CurrentTree, RestoreJob);
        }
    }
}