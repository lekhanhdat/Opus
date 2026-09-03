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
namespace HybridCommonModel.DataModel.Configuration
{
    public  class RegistryConstants
    {
        public const string SubKeyName = @"SYSTEM\CurrentControlSet\Control\Lsa";
        //private static string subKeyName = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa";
        public const string InstallationCodeKey = @"installationCode";
        public const string AgentConfigurationFileKey = @"agentConfiguration";
        public const string ProxySettingKey = @"aveAgentProxy";
        public const string AgentAccountKey = @"aveAgentAccount";

        public const string DefaultEncryptionKey = "a57fb058-acbd-41c9-8d80-c96553f2c82a"; //used as aes key to encrypt/decrypt proxy setting data
    }
}
