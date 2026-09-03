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
using System.IO;

namespace AgentBuildTool.Common
{
    public class WXSFragmentTemplates
    {
        /// <summary>
        /// 0: FileComponents Fragment and all Directory Fragments
        /// 1: Configuration Tool exe fileId = filF6DB40F3B1A08CE6454D1860D34B3E19
        /// 2: Agent Bin directory node id = dir881C8DEA0518EFF0974F076E3EAB57FC
        /// </summary>
        public static readonly string PACKAGE_AGENT_WXS_TEMPLATE = File.ReadAllText("Config/Package_Agent_Template.wxs");
        /// <summary>
        /// 0: all File Components
        /// </summary>
        public const string WXS_Fragment_FileComponents = @"
  <Fragment>
    <ComponentGroup Id=""FileComponents"">{0}
    </ComponentGroup>
  </Fragment>";
        /// <summary>
        /// 0: component id = cmpBC0CCFAC28CA372EA825F2A6FA5430FD
        /// 1: directory id = dir881C8DEA0518EFF0974F076E3EAB57FC
        /// 2: new guid = {D7C30C4B-1144-4375-93C2-8D976A6B7EE9}
        /// 3: file id = filF6DB40F3B1A08CE6454D1860D34B3E19
        /// 4: source = $(var.Type)\Cloud\Agent\bin\AgentCommonUtility.dll
        /// </summary>
        public const string WXS_FileComponent = @"
      <Component Id=""{0}"" Directory=""{1}"" Guid=""{2}"">
        <File Id=""{3}"" KeyPath=""yes"" Source=""{4}"" />
      </Component>";
        /// <summary>
        /// 0: parent directory id = dirBB764EAF498698AE380F11147CB73E41
        /// 1: current directory id = dir881C8DEA0518EFF0974F076E3EAB57FC
        /// 2: current directory name = AppConfig
        /// </summary>
        public const string WXS_DirectoryFragment = @"
  <Fragment>
    <DirectoryRef Id=""{0}"">
      <Directory Id=""{1}"" Name=""{2}"" />
    </DirectoryRef>
  </Fragment>";
    }
}
