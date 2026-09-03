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
    /// <summary>
    /// Error code for Wrapper
    /// </summary>
    public enum WrapperErrorCode : int
    {
        /// <summary>
        /// Unsupport
        /// </summary>
        UnsupportedMode,
        /// <summary>
        /// 
        /// </summary>
        SiteExistInDifferentWebApp,
        /// <summary>
        /// 有重复的源端
        /// </summary>
        DuplicatedLanguageMapping,
        /// <summary>
        /// Web App does not exist
        /// </summary>
        WebAppNotFound,
        /// <summary>
        /// User info is not found
        /// </summary>
        UserInfoNotFound,
        /// <summary>
        /// Unsupported search scope rule
        /// </summary>
        UnsupportedSearchScopeRule,
        /// <summary>
        /// Managed Property Not Found
        /// </summary>
        ManagedPropertyNotFound,
        /// <summary>
        /// Resolve O365Authentications Failed
        /// </summary>
        ResolveO365AuthenticationsFailed,
        /// <summary>
        /// deployment API not available
        /// </summary>
        DeploymentAPINotAvailable,
        /// <summary>
        /// Instance is not available
        /// </summary>
        InstanceNotAvailable,
        /// <summary>
        /// Authentication is not available
        /// </summary>
        AuthenticationNotAvailable,
        /// <summary>
        /// Content database does not exist
        /// </summary>
        ContentDatabaseNotFound,
    }
}
