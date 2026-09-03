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




namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.TCPRequest;
    using AvePoint.GCommon.FileTransfer;
    using AvePoint.GCommon.Network;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.SupportabilityModel;

    #endregion using directives

    /// <summary>
    /// Provide the main logic of the backup service
    /// </summary>
    /// <typeparam name="TBackupJob">the specific job type of backup</typeparam>
    /// <typeparam name="TRequest">the media request which sent by agent</typeparam>
    public abstract class BackupServiceBase<TBackupJob, TRequest>
        : RequestHandlerBase
        , IBackupRequestHandler
        where TBackupJob : BackupJobBase
        where TRequest : MediaTCPRequest
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        IBlockReader blockReceiver;
        String errorMessage = String.Empty;
        BackupCloseInfo closeInfo = new BackupCloseInfo();

        public virtual IDataWriter<TBackupJob> DataWriter { get; set; }

        public TBackupJob BackupJob { get; private set; }

        public BackupCloseInfo CloseInfo { get { return this.closeInfo; } }

        public override void HandleRequest(MediaTCPRequest request, IAveNetwork network)
        {
            base.HandleRequest(request, network);
            try
            {
                this.CloseInfo.BackupStatus = BackupStatus.Succeed;
                this.CloseInfo.ErrorMessage = string.Empty;
                this.BackupJob = Activator.CreateInstance(typeof(TBackupJob), request) as TBackupJob;
                Thread.CurrentThread.Name = String.Format("{0}_{1}", this.BackupJob.JobId, DateTime.Now.ToLongTimeString());
                this.Open();
                this.Backup();
            }
            catch (Exception e)
            {
                this.CloseInfo.BackupStatus = BackupStatus.Failed;
                errorMessage = CatchHelper.ProcessException(e);
                this.logger.Error(MediaServiceApplicationModelResource.BackupServiceBaseHandleRequestError, e.ToString());
                throw new Exception(errorMessage, e);
            }
            finally
            {
                this.Close();
                this.errorMessage += this.CloseInfo.ErrorMessage;
                if (null != blockReceiver)
                {
                    this.blockReceiver.Close(errorMessage);
                }
                else
                {
                    var closeBlock = new AveDataBlock();
                    closeBlock.Type = AveDataBlockType.CLOSE_CONNECTION_TYPE;
                    closeBlock.PutString(errorMessage);
                    network.SendDataBlock(closeBlock);
                }
            }
        }

        public abstract void Open();

        public virtual void Backup()
        {
            this.blockReceiver = new DataBlockReceiver(this.Network);
            var dataBlock = new AveDataBlock();
            while (true)
            {
                this.blockReceiver.ReadDataBlock(dataBlock);
                if (dataBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {
                    this.logger.Info(MediaServiceApplicationModelResource.BackupServiceBaseBackupInformation, dataBlock.RetrieveString());
                    break;
                }
                this.DataWriter.Write(dataBlock);
            }
        }

        public abstract void Close();
    }
}