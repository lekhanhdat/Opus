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
using System.Threading.Tasks;

namespace AvePoint.Media.Service.ArchiverBackup.Restore
{
    using AvePoint.RA.Contract.JobMonitor;
    using GCommon.Contract.Media.TCPRequest;

    public interface IArchiverRestoreService : IDisposable
    {
        void HandleRequest(MediaTCPRequest request, ArchiverRestoreDataBlockManger restoreDataBlockManager, Action<long> updateProgress);

        SimulateResotreResult HandleSimulateRequest(MediaTCPRequest request, CancellationToken cancellationToken);

        // Runs the same preview/simulate restore flow, but notifies onItemProcessed synchronously after each
        // processed item (level + content length) instead of aggregating a result internally. Aggregation is the
        // caller's responsibility (e.g. AveItemPreviewRestoreMain tracking its own live size/level count).
        void HandlePreviewRequest(MediaTCPRequest request, CancellationToken cancellationToken, Action<int, long> onItemProcessed);
    }
}
