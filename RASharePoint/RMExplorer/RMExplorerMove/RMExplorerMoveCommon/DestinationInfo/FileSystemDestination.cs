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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class FileSystemDestination : DestinationBase
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(FileSystemDestination));
        private IExplorerMoveRestore fsRestore = null;

        public FileSystemDestination(IExplorerMoveRestore restore, string desUrl): base()
        {
            fsRestore = restore;
            base.DestType = RecordFlag.FS;
            base.DestinationContainerUrl = desUrl;
            Init();
        }

        public override void Dispose()
        {
            using(fsRestore as IDisposable) { }
        }

        public override Guid GetDestinationTermId(string columnName)
        {
            throw new NotImplementedException();
        }

        public override Task<JobResult> MoveRestoreAsync(SourceBase source)
        {
            return fsRestore.RestoreAsync(source);
        }

        public override string UpdateBCSColumn(bool useExisting, string columnName, Guid termId)
        {
            throw new NotImplementedException();
        }

        public override Guid UpdateClassificationColumnWithDestination(bool useExisting, string columnName, bool forceSetNull = false)
        {
            throw new NotImplementedException();
        }

        private void Init()
        {
            try
            {
                // Allow this process to circumvent ACL restrictions
                //WinAPIHelper.ModifyPrivilege(PrivilegeName.SeRestorePrivilege, true);
                // Sometimes this is required and other times it works without it. Not sure when.
                //WinAPIHelper.ModifyPrivilege(PrivilegeName.SeTakeOwnershipPrivilege, true);
            }
            catch(Exception ex)
            {
                logger.Warn(string.Format("An error occur in init file system destination, reason : {0}.", ex.ToString()));
            }
        }
    }
}
