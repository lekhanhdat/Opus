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
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Service;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceApplicationModel;

    #endregion

    /// <summary>
    /// Provide the functions that register the media itself.
    /// </summary>
    public class RegisterService
         : Startable
    {

        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IMServManageService ServiceManager { get; set; }

        public override void InternalStart()
        {
            this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStartBegin);
            var counter = 1;
            //while (true)
            //{
            //    try
            //    {
            //        this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStartTry,
            //            MediaEnvironment.MediaServer.MediaServerName,
            //            MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
            //            MediaEnvironment.MediaServer.ControlServerAddress,
            //            counter,
            //            MediaEnvironment.MediaServer.MediaServerRegisterMaxTries);

            //        var registerResult = this.ServiceManager.Register(new ServiceDto
            //        {
            //            Type = ServiceType.SERVICE_TYPE_MEDIA,
            //            Id = MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
            //            Name = MediaEnvironment.MediaServer.MediaServerName,
            //            Address = MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
            //            Port = MediaEnvironment.MediaServer.MediaServerControlPort,
            //            DataPort = MediaEnvironment.MediaServer.MediaServerDataPort,
            //            Schema = MediaEnvironment.MediaServer.MediaServerScheme,
            //            Version = MediaEnvironment.MediaServer.MediaServerVersion,
            //            DisplayVersion = MediaEnvironment.MediaServer.MediaServerDisplayVersion,
            //            EnvironmentInfo = MediaEnvironment.OperationSystemName,
            //            CpuHz = MediaEnvironment.CpuHz,
            //            CpuNumber = MediaEnvironment.CpuCount,
            //            CacheInfo = MediaEnvironment.MediaServer.MediaServiceAppliactionCacheDirectoryPath
            //        });

            //        MediaEnvironment.MediaServer.MediaServerId = registerResult.ServiceID;
            //        CryptographyManagement.CryptoMode = registerResult.CryptoMode.ToEnum<CryptoMode>();
            //        CspCommunicationWrapper.CommunicationEncryptionKey = registerResult.CommunicationEncryptionKey;
            //        DataEncryptionInfoManager.DefaultEncryptionInfo =
            //            DataEncryptionInfoManager.PutEncryptionInfo(registerResult.EncryptionProfile).EncryptionInfo;
            //        DefaultAuthInterseption.AuthorizationToken = CspCommunicationWrapper.AuthToken;
            //        MediaEnvironment.AuthorizationKey = CspCommunicationWrapper.AuthToken;
            //        this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStartEnd,
            //            MediaEnvironment.MediaServer.MediaServerName,
            //            MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
            //            MediaEnvironment.MediaServer.ControlServerAddress);

            //        this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStartSuccess);
            //        LoggerInitializer.Initialize();
            //        break;
            //    }
            //    catch (Exception e)
            //    {
            //        ServiceStatusTrackerManager.TraceStatus(
            //            MediaServiceStatus.ErrorOccurred,
            //            String.Format(MediaServiceApplicationModelResource.RegisterServiceStartMediaServiceStatusErrorOccurred, e.ToString()));
            //        this.logger.Error(MediaServiceApplicationModelResource.RegisterServiceStartError, e.ToString());
            //        if (counter++ <= MediaEnvironment.MediaServer.MediaServerRegisterMaxTries)
            //            Thread.Sleep(1000 * MediaEnvironment.MediaServer.MediaServerRegisterWaitSeconds);
            //        else { throw; }
            //    }
            //}
        }

        public override void InternalStop()
        {
            this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStopBegin);
            this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStopTrying,
                  MediaEnvironment.MediaServer.MediaServerName,
                  MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
                  MediaEnvironment.MediaServer.ControlServerAddress);

            //HACK: DO SOME REAL WORK....

            this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStopEnd,
                  MediaEnvironment.MediaServer.MediaServerName,
                  MediaEnvironment.MediaServer.MediaServerHostOrIpAddress,
                  MediaEnvironment.MediaServer.ControlServerAddress);

            this.logger.Info(MediaServiceApplicationModelResource.RegisterServiceStopSuccess);
        }
    }
}