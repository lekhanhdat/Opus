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

    public static class MicroKernelConstant
    {
        public static readonly String CoreIocContainerIdentifier = "CoreIOCContainerIdentifier";
        public static readonly String MicroKernelProxyIdentifierGUID = "71F639EE-4FC6-4843-9929-462D4B831058";

        public static readonly String ClientCurrentOperatingSystem = "ClientOperatingSystem";
        public static readonly String ClientCurrentDomainName = "ClientCurrentDomainName";
        public static readonly String ClientCurrentLoginUserName = "ClientCurrentLoginUserName";
        public static readonly String ClientCurrentThreadUserIdentity = "ClientCurrentThreadUserIdentity";
        public static readonly String ClientCurrentIpAddress = "ClientCurrentIpAddress";
        public static readonly String ClientCurrentHostName = "ClientCurrentHostName";

        public static readonly String MicroKernelTraceSource = "CommonMicroKernel";
        public static readonly String MicroKernelSectionName = "microKernel";
        public static readonly Int32 MicroKernelTraceSourceEventId = 6000;

        public static readonly String HttpsDefaultEndpointAddress = "https://0.0.0.0:0/ControlCore/ControlCoreService.svc";
        public static readonly String NetTcpDefaultEndpointAddress = "net.tcp://0.0.0.0:0/AgentCoreService";
        public static readonly String DefaultThumbprint = "EFB6AAA03D17268BAD4DE3D4E09FC05E24C1B3C8".ToLower();

        public static readonly String ClientChannelMaintenanceThreadIdentifier = "CoreChannelMaintenanceThread";

        public const String GCommonContactAssemblyName = "CommonContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fffb45e56dd478e3";

        public static readonly String IdentityTypeJobId = "JobId";
        public static readonly String IdentityTypeGroupId = "GroupId";
        public static readonly String IdentityTypeTenant = "Tenant";
    }
}
