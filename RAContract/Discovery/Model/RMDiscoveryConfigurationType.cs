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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model
{
    public enum RMDiscoveryConfigurationType
    {
        None = 0,
        Office365NewlyScope = 1,
        Office365InactiveDefinition = 2,
        Office365ROTDefinition = 3,
        Office365RuleChanged = 4,
        Office365CostSaving = 5,
        Office365AppendScope = 6,
        Office365Exclusion = 7,
        SalesforceNewlyScope = 8,
        GoogleNewlyScope = 9,
        GoogleROTDefinition = 10,
        AOSPNewlyScope = 11,
        AOSPInactiveDefinition = 12,
        AOSPROTDefinition = 13,
        AOSPRuleChanged = 14,
        AOSPO365TenantId = 15,
        AOSPCostSaving = 16,
        FileSystemNewlyScope = 17,
        FileSystemInactiveDefinition = 18,
        FileSystemROTDefinition = 19,
        AOSPRescanScope = 20,
        AOSPAllowLockedSites = 21,
    }
}
