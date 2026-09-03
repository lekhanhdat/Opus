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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Net;
    #endregion

    public class MediaServer
    {
        public String ControlServerAddress { get; set; }
        public Int32 ControlServerPort { get; set; }
        public Int32 MediaServerControlPort { get; set; }
        public Int32 MediaServerDataPort { get; set; }
        public Int32 MediaServerRegisterMaxTries { get; set; }
        public Int32 MediaServerRegisterWaitSeconds { get; set; }
        public Int32 MediaServerMaxSearchCount { get; set; }

        private String mMediaServerHostOrIpAddress;
        public String MediaServerId { get; set; }
        public String MediaServerName { get; set; }
        public String MediaServerHostOrIpAddress {
            get { return mMediaServerHostOrIpAddress; }
            set
            {
                mMediaServerHostOrIpAddress = value;
                if ("localhost".Equals(value) || string.IsNullOrEmpty(value))
                {
                    mMediaServerHostOrIpAddress = Dns.GetHostName();
                }
            }
        }
        public String MediaServerVersion { get; set; }
        public String MediaServerDisplayVersion { get; set; }
        public String MediaServerPlatform { get; set; }
        public String MediaServerScheme { get; set; }
        public String MediaServerEnableSSL { get; set; }
        public String MediaServerCredentialThumbprint { get; set; }
        public String MediaServiceApplicationDirectoryPath { get; set; }
        public String MediaServiceAppliactionTempDirectoryPath { get; set; }
        public String MediaServiceAppliactionDataDirectoryPath { get; set; }
        public String MediaServiceAppliactionCacheDirectoryPath { get; set; }
        public String MediaServiceApplicationLogDirectoryPath { get; set; }
        public String MediaServiceApplicationPatchLogDirectoryPath { get; set; }
    }
}