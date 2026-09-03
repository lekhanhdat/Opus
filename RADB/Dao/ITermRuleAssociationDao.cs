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
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermRuleAssociationDao : IBaseDao<RMTermRuleAssociation>
    {
        List<RMTermRuleAssociation> GetTermRuleInfoByTermUniqueId(Guid termUniqueId);
        List<string> GetRelatedTermsByRuleId(Guid ruleId);
        List<RMTermRuleAssociation> GetTermRuleInfoByTermid(int termId, SourceFlag sourceFlag = SourceFlag.All);
        List<RMTermRuleAssociation> GetTermRuleInfoByTermIds(List<int> termIds);
        List<RMTermRuleAssociation> GetTermRuleInfoByRuleIds(List<Guid> ruleIds);
        List<RMTermRuleAssociation> GetTermRuleInfoByTermid(int termId, Dictionary<int, List<RMTermRuleAssociation>> termrules, SourceFlag sourceFlag = SourceFlag.All);
        List<Guid> GetAllRules();
        List<string> GetTermNamesByRuleId(Guid ruleId);
        void DeleteTermRuleInfos(int termId);
        void DeleteTermRuleInfos(Guid ruleId);
        List<RMTermRuleAssociation> GetTermWithRule();
        List<RMTermRuleAssociation> GetTermWithRule(int Level);
        List<int> GetTermIdWithRule();
        List<int> GetTermIdsByRuleId(string ruleId);
        List<Guid> GetTermUniqueIdsByRuleId(string ruleId);
        TermSettingsInfo GetParentSettingsByTermId(int termId);
        List<RMTermRuleAssociation> GetTermWithRuleLevel(int level, List<Rule> rule);
        Task<IEnumerable<RMTermRuleAssociation>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermRuleAssociationTableAsync(IEnumerable<RMTermRuleAssociation> termRuleAssociations);
        Task<long> MultiGeoDeleteAllTermRuleAssociationAsync();
    }
}
