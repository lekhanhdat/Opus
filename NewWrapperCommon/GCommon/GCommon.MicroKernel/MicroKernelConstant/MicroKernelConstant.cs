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


namespace AvePoint.GCommon.MicroKernel
{
    #region using directices
    using System;


    #endregion

    /// <summary>
    /// 
    /// </summary>
    public static class MicroKernelConstant
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly String CoreIocContainerIdentifier = "CoreIOCContainerIdentifier";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String MicroKernelProxyIdentifierGUID = "71F639EE-4FC6-4843-9929-462D4B831058";

        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentOperatingSystem = "ClientOperatingSystem";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentDomainName = "ClientCurrentDomainName";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentLoginUserName = "ClientCurrentLoginUserName";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentThreadUserIdentity = "ClientCurrentThreadUserIdentity";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentIpAddress = "ClientCurrentIpAddress";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientCurrentHostName = "ClientCurrentHostName";

        /// <summary>
        /// 
        /// </summary>
        public static readonly String MicroKernelTraceSource = "CommonMicroKernel";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String MicroKernelSectionName = "microKernel";
        /// <summary>
        /// 
        /// </summary>
        public static readonly Int32 MicroKernelTraceSourceEventId = 6000;

        /// <summary>
        /// 
        /// </summary>
        public static readonly String HttpsDefaultEndpointAddress = "https://0.0.0.0:0/ControlCore/ControlCoreService.svc";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String NetTcpDefaultEndpointAddress = "net.tcp://0.0.0.0:0/AgentCoreService";
        /// <summary>
        /// 
        /// </summary>
        public static readonly String DefaultThumbprint = "E17BEDE931C319865ABA0673E153177F5557735B".ToLowerInvariant();

        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientChannelMaintenanceThreadIdentifier = "CoreChannelMaintenanceThread";

        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientPlatformVersion = "MicroKernelClientPlatformVersion";

        /// <summary>
        /// 
        /// </summary>
        public static readonly String ClientPlatformDisplayVersion = "MicroKernelClientPlatformDisplayVersion";
        /// <summary>
        /// 
        /// </summary>
        public const String GCommonContactAssemblyName = "CommonContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fffb45e56dd478e3";
    }
}
