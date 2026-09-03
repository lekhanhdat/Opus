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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/1/16",
    "yuchenyang@avepoint.com",
    "dwxue@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion

    public class ArchiverBackupService
        : BackupServiceBase<ArchiverBackupJob, ArchiverBackupRequest>
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Open()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupServiceOpenBegin);
            this.BackupJob.Network = this.Network;
            this.DataWriter.Open(this.BackupJob);
            this.SendLastContent();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupServiceOpenEnd);
        }

        public override void Close()
        {
            this.DataWriter.Close(this.CloseInfo);
        }

        void SendLastContent()
        {
            this.Network.SendMessage(ServiceConstants.StringSendToAgent);
        }
    }
}