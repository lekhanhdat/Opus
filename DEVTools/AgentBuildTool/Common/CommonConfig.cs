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
using AgentBuildTool.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace AgentBuildTool.Common
{
    public class CommonConfig
    {
        public static readonly bool AGENT_MajorVersionBuild = "true".Equals(ConfigurationManager.AppSettings["MajorVersionBuild"], StringComparison.OrdinalIgnoreCase);

        public static readonly string AGENT_PACKAGE_WXS_PATH = ConfigurationManager.AppSettings["package_agent_wxs"];

        public static readonly string AGENT_LIC_PATH = ConfigurationManager.AppSettings["agent_lic_path"];

        public static readonly string AGENT_BIN_OUTPUT_PATH = ConfigurationManager.AppSettings["agentbin_output"];

        public static readonly string AGENT_PACKAGE_DLLS_SIGNNAME_CONFIG = ConfigurationManager.AppSettings["package_dlls_signname"];

        public static readonly string AGENT_PACKAGE_DLLS_OBFUSCATE_CONFIG = ConfigurationManager.AppSettings["package_dlls_obfuscate"];

        public static readonly string AGENT_CONFIGURATION_TOOLNAME = ConfigurationManager.AppSettings["ConfigurationToolName"];

        public static string GetWXSNodeId(WXSNodeIdType type)
        {
            string prefix;
            switch (type)
            {
                case WXSNodeIdType.Component:
                    prefix = "cmp";
                    break;
                case WXSNodeIdType.File:
                    prefix = "fil";
                    break;
                case WXSNodeIdType.Directory:
                    prefix = "dir";
                    break;
                default:
                    prefix = string.Empty;
                    break;
            }
            return $"{prefix}{Guid.NewGuid().ToString("N").ToUpper()}";
        }
    }

}
