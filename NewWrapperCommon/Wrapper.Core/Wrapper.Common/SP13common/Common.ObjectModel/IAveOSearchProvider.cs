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
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    public interface IAveOSearchProvider
    {
        string AcronymDefinitionProviderFlow { get; }
        Guid AcronymDefinitionProviderGuid { get; }
        string AcronymDefinitionProviderId { get; }
        string AcronymDefinitionProviderName { get; }
        string BestBetProviderFlow { get; }
        Guid BestBetProviderGuid { get; }
        string BestBetProviderId { get; }
        string BestBetProviderName { get; }
        string ExchangeSearchProviderFlow { get; }
        Guid ExchangeSearchProviderGuid { get; }
        string ExchangeSearchProviderId { get; }
        string ExchangeSearchProviderName { get; }
        int FlowNameMaxLength { get;}
        string LocalPeopleProviderFlow { get; }
        Guid LocalPeopleProviderGuid { get; }
        string LocalPeopleProviderId { get; }
        string LocalPeopleProviderName { get; }
        string LocalSharePointProviderFlow { get; }
        Guid LocalSharePointProviderGuid { get; }
        string LocalSharePointProviderId { get; }
        string LocalSharePointProviderName { get; }
        int NameMaxLength  { get;}
        string OpenSearchProviderFlow { get; }
        Guid OpenSearchProviderGuid { get; }
        string OpenSearchProviderId { get; }
        string OpenSearchProviderName { get; }
        string PersonalFavoritesProviderFlow { get; }
        Guid PersonalFavoritesProviderGuid { get; }
        string PersonalFavoritesProviderId { get; }
        string PersonalFavoritesProviderName { get; }
        string RemotePeopleProviderFlow { get; }
        Guid RemotePeopleProviderGuid { get; }
        string RemotePeopleProviderId { get; }
        string RemotePeopleProviderName { get; }
        HashSet<Guid> RemoteProviders { get; }
        string RemoteSharePointProviderFlow { get; }
        Guid RemoteSharePointProviderGuid { get; }
        string RemoteSharePointProviderId { get; }
        string RemoteSharePointProviderName { get; }


    }
}
