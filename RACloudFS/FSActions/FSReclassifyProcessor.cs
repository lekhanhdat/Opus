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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RACloudFS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvePoint.RA.RACloudFS.FSActions
{
    public class FSReclassifyProcessor
    {
        private IRALogger logger = RALogger.GetInstance(typeof(FSReclassifyProcessor));
        public IRuleManagerService RuleManagerService { get; set; }
        public IFileSystemSettingDao FileSystemSettingDao { get; set; }
        public ITermRuleAssociationDao TermRuleAssociationDao { get; set; }
        public IFSConnectionDao FSConnectionDao { get; set; }
        public ITermDao TermDao { get; set; }
        public ITermSetDao TermSetDao { get; set; }


        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        //key: Term(as t1) ID, value: (key: "Term Or TermSet"(as t2) ID, value: if t1 is sub term of t2)
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();

        private List<RMFileSystemSetting> mAllsettings = new List<RMFileSystemSetting>();
        private List<Rule> mAllRulesFromDA;
        private List<Rule> mResultRules;
        public FSReclassifyProcessor()
        {
            RuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
            FileSystemSettingDao = (IFileSystemSettingDao)PlatformWindsorManager.GetService(typeof(IFileSystemSettingDao));
            TermRuleAssociationDao = (ITermRuleAssociationDao)PlatformWindsorManager.GetService(typeof(ITermRuleAssociationDao));
            FSConnectionDao = (IFSConnectionDao)PlatformWindsorManager.GetService(typeof(IFSConnectionDao));
            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            TermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));

            mAllRulesFromDA = RuleManagerService.GetRulesFromRecords();
        }

        public List<Record> ChangeFSRecordTermAction(List<Record> records, int termIntId, string termName, Guid termId, bool isNewLogicAccount, ref List<Guid> failedIds)
        {
            List<Record> successRecords = new List<Record>();
            var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
            mAllsettings = FileSystemSettingDao.LoadAllSetting();
            mAllsettings.ForEach(o => { o.FullPath = EncodeUtil.DecryptByCommunicationKey(o.FullPath); });
            var rules = mAllRulesFromDA;
            var ruleInfos = TermRuleAssociationDao.GetTermRuleInfoByTermid(termIntId).OrderBy(t => t.RuleOrder);
            var ruleIds = ruleInfos.Select(t => t.RuleId).ToList();
            var resustRules = new List<Rule>();
            foreach (var ruleId in ruleIds)
            {
                var rule = rules.Where(r => r.Id.Equals(ruleId.ToString())).FirstOrDefault();
                if (rule != null && rule.FSRule != null)
                {
                    resustRules.Add(rule);
                }
            }
            mResultRules = resustRules;           
            logger.Info($"3.1 all the term associate rules are {string.Join(", ", mResultRules.Select(r => r?.Id))}");
            var ruleUtil = new FSRuleUtil(mResultRules);
            ExplorerDao explorerDao = new ExplorerDao();
            foreach (var recList in recDic.Values)
            {
                if (recList.Count > 0)
                {
                    try
                    {
                        var connId = recList[0].AveSiteId;
                        var connObj = FSConnectionDao.GetConnectionById(new Guid(connId));
                        if (connObj == null)
                        {
                            logger.Error($"can not get connection by Id: {connId}");
                            throw new Exception("Connection Not Found.");
                        }

                        var groupSetting = mAllsettings.FirstOrDefault(s => s.ScopeId == connObj.GroupId);
                        if (groupSetting == null)
                        {
                            throw new Exception("Group setting not init.");
                        }
                        var nodeBinds = mAllsettings.Where(s => s.IdPath.Contains(connId)).ToDictionary(s => s.FullPath);

                        RMFileSystemSetting bindSetting;
                        string settingPath;
                        bool bTemp;
                        Guid previousTermId = Guid.Empty;
                        foreach (Record rd in recList)
                        {
                            settingPath = rd.NodeType == (int)NodeLevel.FSFile ? rd.DirPath : Path.Combine(rd.DirPath, rd.LeafName);
                            var tempPath = settingPath;
                            do
                            {
                                bTemp = nodeBinds.TryGetValue(tempPath, out bindSetting);
                                if (bTemp)
                                {
                                    break;
                                }
                                tempPath = tempPath.Substring(0, tempPath.LastIndexOf('\\'));
                            } while (tempPath.Length >= connObj.UNCPath.Length);
                            if (!bTemp)
                            {
                                bindSetting = groupSetting;
                                nodeBinds[settingPath] = bindSetting;
                            }
                            if (CheckTermValue(bindSetting, termId))
                            {
                                previousTermId = rd.TermId;
                                rd.TermId = termId;
                                rd.TermName = termName;
                                if(isNewLogicAccount && previousTermId != termId) rd.RemoveManualFields();
                                ruleUtil.AssembleRule(rd);
                                explorerDao.AddOrUpdateRecordWithKeepManual(rd, true, isKeepManualColumn: false);
                                successRecords.Add(rd);
                            }
                            else
                            {
                                failedIds.Add(rd.Id);
                                logger.Warn("[FileSystem]Update item term failed, Term [{1}] can't set to item, it's not a right subterm. Records ID: {0}.", rd?.Id, rd?.TermName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        failedIds.AddRange(recList.Select(t => t.Id));
                        logger.Warn("change term action failed {0}", e.ToString());
                    }
                }
            }
            return successRecords;
        }

        #region Check Term
        private bool CheckTermValue(RMFileSystemSetting setting, Guid termId)
        {
            bool bindTermSet = setting.TermId == Guid.Empty;
            var parentId = bindTermSet ? setting.TermSetId : setting.TermId;
            return CheckTermValue(bindTermSet, parentId, termId);
        }

        private bool CheckTermValue(bool bindTermSet, Guid parentId, Guid termId)
        {
            string termPath = null;
            if (!mTermPaths.TryGetValue(termId, out termPath))
            {
                termPath = TermDao.GetTermIdPath(termId);
                mTermPaths[termId] = termPath;
            }

            if (string.IsNullOrEmpty(termPath))
            {
                return false;
            }

            Dictionary<Guid, bool> parentNodes = null;
            if (!mTermAllowToParent.TryGetValue(termId, out parentNodes))
            {
                parentNodes = new Dictionary<Guid, bool>();
                mTermAllowToParent[termId] = parentNodes;
            }

            string parentNodePath = null;
            bool isSubTerm = false;
            if (!parentNodes.TryGetValue(parentId, out isSubTerm))
            {
                if (bindTermSet)
                {
                    parentNodePath = (TermSetDao.GetRMTermSetByGuid(parentId)?.Id)?.ToString() + "/";
                }
                else
                {
                    parentNodePath = TermDao.GetTermIdPath(parentId) + "/";
                }
                isSubTerm = termPath.StartsWith(parentNodePath, StringComparison.OrdinalIgnoreCase);
                parentNodes[parentId] = isSubTerm;
            }
            return isSubTerm;
        }

        #endregion

    }
}
