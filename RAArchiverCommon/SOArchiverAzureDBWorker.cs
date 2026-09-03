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
using AvePoint.RA.Contract;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    /// <summary>
    /// will delete later
    /// </summary>
    public class SOArchiverAzureDBWorker
    {
        public void UpdateStatus(SOApproveDBStatus dbStatus, Guid nodeId, int archiveLevel, string jobId)
        { }
        public int GetKeepDataStatus(Guid docID)
        {
            return 1;
        }
        public void DeleteAndMoveItems(ScheduleConfiguration config)
        { }

        public void UpdateArchiveDeletionStatus(SOApproveDBStatus sOApproveDBStatus, Guid fileId, int archiveLevel, int uiVersion, string jobId)
        {
        }
    }
}