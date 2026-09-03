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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Common
{
    internal class Constants
    {
        internal const string WrapperCoreName = "AgentCommonWrapperCore";
        
        internal const string SP2010LanguageMappingFile = @"data\WrapperCommon\SP2010WrapperLanguageMapping.xml";
        internal const string SP2013LanguageMappingFile = @"data\WrapperCommon\SP2013WrapperLanguageMapping.xml";
        internal const string Office365LanguageMappingFile = @"data\WrapperCommon\AgentCommonOffice365WrapperLanguageMapping.xml";

        internal const string WrapperResourcesFolderName = "Resources";
        internal const string WrapperConfigFileName = "AgentCommonWrapperCore.config";
        internal const string WrapperCoreConfigurationFile = "AgentCommonWrapperCoreConfiguration.config";

        /*
         * SharePoint User Account & Permission Role
         */
        internal const int SYSTEM_ACCOUNT_ID = 1073741823;
        internal const int LIMIT_ACCESS_ROLE_ID = 1073741825;
    }
}
