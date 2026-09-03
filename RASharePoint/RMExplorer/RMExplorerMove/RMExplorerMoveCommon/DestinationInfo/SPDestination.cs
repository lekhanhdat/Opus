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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPDestination : DestinationBase
    {
        internal Guid SiteId { get; private set; }
        private IExplorerMoveRestore spRestore = null;
        public SPDestination(IExplorerMoveRestore restore, string destinationUrl, Guid siteId) : base()
        {
            spRestore = restore;
            SiteId = siteId;
            base.DestType = RecordFlag.SP;
            base.DestinationContainerUrl = destinationUrl;
        }

        public override void Dispose()
        {
            using(spRestore as IDisposable) { }
        }

        public override Task<JobResult> MoveRestoreAsync(SourceBase source)
        {
            return spRestore.RestoreAsync(source);
        }

        public override string UpdateBCSColumn(bool useExisting, string columnName, Guid termId)
        {
            return spRestore.UpdateBCSColumn(useExisting, columnName, termId);
        }

        public override Guid GetDestinationTermId(string columnName)
        {
            return spRestore.GetDestinationTermId(columnName);
        }

        public override Guid UpdateClassificationColumnWithDestination(bool useExisting, string columnName, bool forceSetNull = false)
        {
            return spRestore.UpdateClassificationColumnWithDestination(useExisting, columnName, forceSetNull);
        }

    }
}
