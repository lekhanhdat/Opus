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
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPSource : SourceBase
    {
        private IExplorerMoveBackup moveBackup = null;
        internal bool IsFirstItem = false;
        public string SiteUrl = string.Empty;
        public SPSource(IExplorerMoveBackup backup, SourceRecord record, int nodeType, int nodeLevel, string sourceUrl, string exportFilePath, string fileName, bool isFirstItem = false)
            : base(record.Id, record.ScopeId, record.NodeId, nodeType, nodeLevel, sourceUrl, exportFilePath, "", fileName)
        {
            moveBackup = backup;
            IsFirstItem = isFirstItem;
            SiteUrl = record.SiteUrl;
        }

        public override void Delete()
        {
            moveBackup.Delete();
        }

        public override void MoveBackup()
        {
            moveBackup.MoveBackup();
        }

        public override Guid GetSourceTermId(string columnName)
        {
            return moveBackup.GetSourceTermId(columnName);
        }

    }
}
