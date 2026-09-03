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

namespace Microsoft365.Authentication.TokenProvider.TokenService;

using Util.MSAzure;
using AvePoint.RA.CommonUtil;
using System;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token.Services;

public class TsTokenProviderParameter
{
    /// <summary>
    /// common parameter, required
    /// </summary>
    public IModernTokenService TokenService { get; set; }

    /// <summary>
    /// common parameter, required
    /// </summary>
    public string CustomerId { get; set; }
    /// <summary>
    /// common parameter, required
    /// </summary>
    public AzureEnvironment EnvironmentType { get; set; }
    /// <summary>
    /// common parameter, required
    /// </summary>
    public string TenantId { get; set; }
    /// <summary>
    /// service account authentication
    /// </summary>
    public string ServiceAccountUserName { get; set; }
    /// <summary>
    /// account pool user authentication
    /// </summary>
    public string AccountPoolUserName { get; set; }

    public bool ServiceAccountIsMFA { get; set; }

    public bool AccountPoolIsMFA { get; set; }

    /// <summary>
    /// app only authentication
    /// </summary>
    public IdentityProviderType? AppType { get; set; }
    /// <summary>
    /// app only authentication
    /// </summary>
    public string AppId{ get; set; }

    public string MicrosoftDelegateId { get; set; }

    public string MicrosoftDelegateAppUsername { get; set; }

    public string VivaEngageId { get; set; }
}