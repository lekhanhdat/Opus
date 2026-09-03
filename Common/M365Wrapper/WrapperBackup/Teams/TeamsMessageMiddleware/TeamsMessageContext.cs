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

namespace ExchangeUtility.Graph.Teams;

using System;

using ExchangeCommonWrapper;
using Util.MSAzure;

public class TeamsMessageContext : Ms365TenantContext
{
    public TeamsMessageContext(TeamChatMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        ArgumentNullException.ThrowIfNull(message.Body);
        ArgumentNullException.ThrowIfNull(message.Body.Content);
    }

    public TeamChatMessage Message { get; private set; }

    public ChannelContext ChannelContext { get; set; }
}

public class ChannelContext
{
    public string GroupId { get; set; }

    public string ChannelId { get; set; }

    public string ChannelFilesUrl { get; set; }

    public bool IsPrivate { get; set; }
}

public class Ms365TenantContext
{
    public MicrosoftTeamsAPIBase TeamService { get; set; }

    public MicrosoftTeamsAPIBase TeamService4ServiceAccount { get; set; }

    public AzureEnvironment? Environment { get; set; }
    public bool IsGovernmentEnvironment { get; set; }
}