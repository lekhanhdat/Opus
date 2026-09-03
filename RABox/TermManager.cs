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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RADataSynchronize.TermCheck;
using RADataSynchronize.TermCheck.Model;

namespace RABox
{
    public class TermManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(TermManager));
        private readonly IRMChangeClassificationDao _changeClassificationDao;
        private readonly ITermDao _termDao;
        private readonly ITermSetMembershipDao _termSetMembershipDao;
        private readonly Dictionary<Guid, RMTerm> _termsCache = new Dictionary<Guid, RMTerm>();
        private Dictionary<Guid, RMTermIdentity> _usageTermInfoCache = new Dictionary<Guid, RMTermIdentity>();

        public TermManager()
        {
            _changeClassificationDao = PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
            _termDao = PlatformWindsorManager.GetService<ITermDao>();
            _termSetMembershipDao = PlatformWindsorManager.GetService<ITermSetMembershipDao>();
        }

        public Dictionary<Guid, long> GetHasChangedTermIds(long ticks)
        {
            var res = new Dictionary<Guid, long>();

            List<RMChangeClassification> changedTerms = _changeClassificationDao.GetAllChangedInfo(ticks, (int)TermChangeType.TermRule);

            foreach (var changedTerm in changedTerms)
            {
                res[changedTerm.TermId] = changedTerm.ChangeTime;

                var subTerms = _termDao.GetAllSubTermUniqueIds(changedTerm.TermId);

                foreach (var subTerm in subTerms)
                {
                    res[subTerm] = changedTerm.ChangeTime;
                }
            }

            return res;
        }

        public TermInfo GetMatchedTermInfo(BoxItemProxy? item, Record? record, BoxSettingDto setting, BoxTreeNode? topNode = null)
        {
            if (setting.DeployTermMethod == DeployTermMethod.NoDefaultTerm)
            {
                return new TermInfo
                {
                    IsManually = true
                };
            }
            else if (setting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
            {
                return new TermInfo
                {
                    IsManually = false,
                    TermId = setting.DefaultTermId.ToString(),
                    TermName = setting.DefaultTermName,
                    TermIsRemoved = setting.IsDefaultTermRemoved,
                    TermIsDeprecated = setting.IsDefaultTermDeprecated,
                };
            }
            else
            {
                return GetAutoMatchedTermInfo(item, record, setting, topNode);
            }
        }

        private TermInfo GetAutoMatchedTermInfo(BoxItemProxy? item, Record? record, BoxSettingDto setting, BoxTreeNode? topNode)
        {
            Dictionary<ArchiverFilterRuleType, object> values = new();
            if (item != null)
            {
                values = GetRuleTypeMappingValue(item, topNode);
            }
            else if (record != null)
            {
                values = GetRuleTypeMappingValue(record);
            }

            if (!TermCriteriaChecker.TryGetAccordWithTermInfo(setting.AutoClassificationRules, values, out var termInfo))
            {
                throw new Exception($"The item [{item.Id}] find related term has an error.");
            }

            return termInfo;
        }

        private Dictionary<ArchiverFilterRuleType, object> GetRuleTypeMappingValue(Record record)
        {
            var nameArr = record.LeafName.Split('.');
            var extension = nameArr.Length > 1 ? nameArr.Last() : "";

            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);

            return new Dictionary<ArchiverFilterRuleType, object>
            {
                { ArchiverFilterRuleType.Name, record.LeafName },
                { ArchiverFilterRuleType.DocumentSize, metaInfo.FileSize },
                { ArchiverFilterRuleType.ModifiedTime, record.TimeModified },
                { ArchiverFilterRuleType.CreatedTime, record.TimeCreated },
                //{ ArchiverFilterRuleType.LastAccessedTime, item.LastAccessTime }, 
                { ArchiverFilterRuleType.Type, extension },
                { ArchiverFilterRuleType.FilePath, record.DirPath }
            };
        }

        private Dictionary<ArchiverFilterRuleType, object> GetRuleTypeMappingValue(BoxItemProxy item, BoxTreeNode topNode)
        {
            var nameArr = item.Name.Split('.');
            var extension = nameArr.Length > 1 ? nameArr.Last() : "";
            return new Dictionary<ArchiverFilterRuleType, object>
            {
                { ArchiverFilterRuleType.Name, item.Name },
                { ArchiverFilterRuleType.DocumentSize, item.Size },
                { ArchiverFilterRuleType.ModifiedTime, item.Modified },
                { ArchiverFilterRuleType.CreatedTime, item.Created },
                //{ ArchiverFilterRuleType.LastAccessedTime, item.LastAccessTime },
                { ArchiverFilterRuleType.Type, extension },
                { ArchiverFilterRuleType.FilePath,  item.Id == topNode.RealId || item.Id == BoxUtility.BoxRootFolderId ? topNode.FullPath : item.CombinePath(topNode.FullPath, item.FullPath)}
            };
        }

        public Dictionary<Guid, RMTerm> LoadTerms()
        {
            try
            {
                _logger.Info("Begin to load terms.");
                _termsCache.AddRangeInternal(_termDao.GetAllTermsForce().ToDictionary(t => t.UniqueId), true);
                _logger.Info("Loaded {0} terms.", _termsCache.Count);

                return _termsCache;
            }
            catch (Exception e)
            {
                _logger.Error($"LoadTerms Error: {e}");
                throw new Exception(I18NEntity.GetString("RM_JS_DocAve_CommunicationError"));
            }
        }

        public async Task<Dictionary<int, List<int>>> LoadTermSetMemberShips()
        {
            return (await _termSetMembershipDao.FindListWithColumnsAsync(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved))
                .GroupBy(t => t.ParentTermId, v => v.TermId)
                .ToDictionary(t => t.Key, v => v.ToList());
        }

        public bool TryGetTerm(Guid termId, out RMTerm term)
        {
            return _termsCache.TryGetValue(termId, out term);
        }

        public List<JMTermSelection> GetTermSelections(Dictionary<Guid, RMTermIdentity> termIdentities)
        {
            _usageTermInfoCache = termIdentities;
            return _usageTermInfoCache.Values.Select(term => new JMTermSelection
            {
                Term = term.Name,
                TermFullPath = term.FullPath
            }).ToList();
        }

        public bool TryGetUsageTermInfo(Guid termId, out RMTermIdentity termIdentity)
        {
            return _usageTermInfoCache.TryGetValue(termId, out termIdentity);
        }

        public bool CheckHasAnyUsageTermInfo()
        {
            return _usageTermInfoCache != null && _usageTermInfoCache.Count > 0;
        }
    }
}
