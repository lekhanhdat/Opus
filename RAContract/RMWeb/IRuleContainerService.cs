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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRuleContainerService
    {
        RuleContainerDto CreateRuleContainer(RuleContainerDto ruleContainer);
        RuleContainerDto EditRuleContainer(RuleContainerDto ruleContainer);
        Task<(List<RuleContainerDto>,int)> GetRuleContainersAsync(RuleContainerQuery query);
        Task<List<RuleContainerDto>> GetAllRuleContainersAsync();
        List<RuleContainerDto> GetRuleContainersByTermId(int termId);
        List<RuleContainerDto> GetRuleContainersForLabel();
        List<RuleContainerDto> GetRuleContainersByIds(List<Guid> conteinerIds);
        List<RuleContainerDto> GetRuleContainersByContainerId(string scopeContainerId, int sourceFlag);
        RAReturnMessage DeleteRuleContainer(Guid containerId);
        string GetRuleTreeForSecurityGroup(QueryRuleObjDto queryDto);
        RAReturnMessage CheckContainerCrossSecurityGroup(string oldContainerId, string newContainerId, string ruleId);
    }
}
