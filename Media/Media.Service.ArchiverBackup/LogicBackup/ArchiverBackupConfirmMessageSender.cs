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
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Network;
    using Merged18NResources.MediaServiceArchiverBackup;

    #endregion

    internal class ArchiverBackupConfirmMessageSender
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Boolean isJobEnded;
        Boolean isSucceed;
        Boolean isAllMessageSended;
        IAveNetwork currentNetWork;
        Queue<string> messagesNeedToSend;

        public ArchiverBackupConfirmMessageSender(IAveNetwork netWork)
        {
            currentNetWork = netWork;
            isJobEnded = false;
            isAllMessageSended = false;
            isSucceed = false;
            messagesNeedToSend = new Queue<string>();
        }

        public void SendConfirmMessageToAgent(List<string> fileHeaders)
        {
            fileHeaders.ForEach(message => this.messagesNeedToSend.Enqueue(message));
        }

        public void Start()
        {
            Thread sendThread = new Thread(new ThreadStart(SendProcess));
            sendThread.Name = "ArchiverSendConfirmMessageThread_" + Thread.CurrentThread.Name;
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        public void SendProcess()
        {
            try
            {
                logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessStart);
                Int32 times = 0;
                while (!isJobEnded)
                {
                    if (times == 60 && this.messagesNeedToSend.Count > 0)
                    {
                        string message = this.messagesNeedToSend.Dequeue();
                        this.currentNetWork.SendMessage(message);
                        var header = new MediaArchiverFileHeader(message);
                        this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessInfo, header.Type, header.Path);
                        this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessSend, message);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        if (times == 60)
                        {
                            times = 0;
                            this.currentNetWork.SendMessage("<KeepAlive />");
                            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessKeepAlive);
                        }
                        else times++;
                    }
                }
                while (this.messagesNeedToSend.Count > 0)
                {
                    string message = this.messagesNeedToSend.Dequeue();
                    this.currentNetWork.SendMessage(message);
                    var header = new MediaArchiverFileHeader(message);
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessInfo, header.Type, header.Path);
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessSend, message);
                }
                currentNetWork.SendMessage("END");
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessSendAll);
                if (this.isSucceed)
                {
                    var msg = currentNetWork.ReceiveMessage();
                    this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessConfirmMessage, msg);
                }
                isAllMessageSended = true;
            }
            catch (System.Exception ex)
            {
                this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderSendProcessException, ex.ToString());
            }
        }

        public void Stop(Boolean isSuccessful)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderStopBegin);
            this.isSucceed = isSuccessful;
            this.isJobEnded = true;
            while (!isAllMessageSended)
            {
                Thread.Sleep(5 * 1000);
            }
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupConfirmMessageSenderStopSuccessfully);
        }
    }
}