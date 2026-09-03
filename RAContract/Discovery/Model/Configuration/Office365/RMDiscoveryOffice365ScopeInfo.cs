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
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Configuration.Office365
{

    public class RMDiscoveryOffice365ScopeInfo
    {
        [JsonProperty("scopeType")]
        public RMDiscoveryOffice365ScopeType ScopeType { get; set; } = RMDiscoveryOffice365ScopeType.DataSource;

        [JsonProperty("contentSources")]
        public List<SourceFlag> ContentSources { get; set; } = [];

        [JsonProperty("specifyContainerIds")]
        public List<Guid> SpecifyContainerIds { get; set; } = [];

        public RMDiscoveryOffice365ScopeInfo CompatibleConvert()
        {
            if (ScopeType != RMDiscoveryOffice365ScopeType.DataSource)
            {
                return this;
            }

            if (!ContentSources.Any())
            {
                ContentSources.Add(SourceFlag.SharePoint);
            }

            return this;
        }
    }

    public enum RMDiscoveryOffice365ScopeType
    {
        None = 0,
        DataSource = 1,
        Specify = 2,
    }
}
