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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;
using Aspose.Email.Storage.Pst;

namespace AvePoint.RA.SharePoint.OneDriveExplorerSync.Utils
{
    public class RMOneDriveRuleChecker
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMOneDriveRuleChecker));
        private readonly RuleCollection _rules;
        private RuleManagement ruleManagement = null;

        public RMOneDriveRuleChecker(RuleCollection rules)
        {
            ruleManagement = new RuleManagement(rules);
            _rules = rules;
        }


        public SyncItemRuleInfo CheckDisposalRule(IAveSite site)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();
            IAveWeb aveWeb = site.RootWeb;
            string disposalAction = string.Empty;
            if (NeedCheckRule(aveWeb.Url))
            {
                itemRule.Rule = ruleManagement.CheckSiteCollectionCriteria(site);
                if (itemRule.Rule != null)
                {
                    itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";
                }
                else
                {
                    itemRule.Rule = ruleManagement.GetDueDisposalRule(site, ref disposalAction);
                    itemRule.DisposalAction = disposalAction;
                }
            }
            return itemRule;

        }

        public SyncItemRuleInfo CheckDisposalRule(AveDiscoverWeb web, SyncItemRuleInfo parentItemRule = null)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();
            IAveWeb aveWeb = web.AveWeb;
            DateTime parentDateTime;
            if (NeedCheckRule(aveWeb.Url))
            {
                if (parentItemRule.Rule != null && !DateTime.TryParse(parentItemRule.DisposalAction, out parentDateTime))
                {
                    logger.Info("fit Parent rule now {0}:{1}", parentItemRule.Rule.Name, parentItemRule.DisposalAction);
                    //parent fit rule no need check rule .no need check rule again.
                    return parentItemRule;
                }
                else if (parentItemRule.Rule != null && !string.IsNullOrEmpty(parentItemRule.DisposalAction))
                {
                    //parent get due disposal rule. and current disposal rule time maybe More recent
                    SyncItemRuleInfo webItemRule = CheckDisposalRule(web);
                    string webDueDisposalTime = webItemRule.DisposalAction;
                    if (webItemRule.Rule != null && (webDueDisposalTime == "RDM_RecordsExporer_Status_NextJob" || webDueDisposalTime == "Next Job") 
                            || (webItemRule.Rule != null && ((Convert.ToDateTime(webDueDisposalTime) < Convert.ToDateTime(parentItemRule.DisposalAction)))))
                    {
                        return webItemRule;
                    }
                    else
                    {
                        return parentItemRule;
                    }
                }
                else
                {
                    if (aveWeb.IsRootWeb)
                    {
                        logger.Info("Root web no need check web rule {0}", aveWeb.Title);
                        return itemRule;
                    }

                    itemRule.Rule = ruleManagement.CheckSiteCriteria(aveWeb);
                    if (itemRule.Rule != null)
                    {
                        itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";
                    }
                    else
                    {
                        string disposalAction = string.Empty;
                        itemRule.Rule = ruleManagement.GetDueDisposalRule(aveWeb, ref disposalAction);
                        itemRule.DisposalAction = disposalAction;
                    }
                }
            }
            return itemRule;
        }

        public SyncItemRuleInfo CheckDisposalRule(AveDiscoverList list, IAveList aveList, SyncItemRuleInfo parentItemRule = null)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();
            
            if (NeedCheckRule(list?.RootFolderUrl))
            {
                DateTime parentDateTime;

                if (parentItemRule.Rule != null && !DateTime.TryParse(parentItemRule.DisposalAction, out parentDateTime))
                {
                    logger.Info("fit Parent rule now {0}:{1}", parentItemRule.Rule.Name, parentItemRule.DisposalAction);
                    //parent fit rule no need check rule .no need check rule again.
                    return parentItemRule;
                }
                else if (parentItemRule.Rule != null && !string.IsNullOrEmpty(parentItemRule.DisposalAction))
                {
                    //parent get due disposal rule. and current disposal rule time maybe More recent
                    
                    SyncItemRuleInfo listRule = CheckDisposalRule(list, aveList);
                    string listDueDisposalTime = listRule.DisposalAction;
                    if ((listRule != null && (listDueDisposalTime == "RDM_RecordsExporer_Status_NextJob" || listDueDisposalTime == "Next Job"))
                            || (listRule != null && ((Convert.ToDateTime(listDueDisposalTime) < Convert.ToDateTime(parentItemRule.DisposalAction)))))
                    {
                        return listRule;
                    }
                    else
                    {
                        return parentItemRule;
                    }
                }
                else
                {
                    itemRule.Rule = ruleManagement.CheckListCriteria(aveList);
                    if (itemRule.Rule != null)
                    {

                        itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";//to do next
                    }
                    else
                    {
                        string disposalAction = string.Empty;
                        itemRule.Rule = ruleManagement.GetDueDisposalRule(aveList, ref disposalAction);
                        itemRule.DisposalAction = disposalAction;
                    }

                }
            }
            return itemRule;
        }

        public SyncItemRuleInfo CheckDisposalRule(AveDiscoverFolder folder, SyncItemRuleInfo parentItemRule = null)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();
            
            DateTime parentDateTime;
            if (NeedCheckRule(folder?.FullUrl))
            {
                if (parentItemRule.Rule != null && !DateTime.TryParse(parentItemRule.DisposalAction, out parentDateTime))
                {
                    logger.Info("fit Parent rule now {0}:{1}", parentItemRule.Rule.Name, parentItemRule.DisposalAction);
                    //parent fit rule no need check rule .no need check rule again.
                    return parentItemRule;
                }
                else if (parentItemRule.Rule != null && !string.IsNullOrEmpty(parentItemRule.DisposalAction))
                {
                    //parent get due disposal rule. and current disposal rule time maybe More recent
                    SyncItemRuleInfo folderRule = CheckDisposalRule(folder);
                    string folderDueDisposalTime = folderRule.DisposalAction;
                    if ((folderRule.Rule != null && (folderDueDisposalTime == "RDM_RecordsExporer_Status_NextJob" || folderDueDisposalTime == "Next Job"))
                            || (folderRule != null && ((Convert.ToDateTime(folderDueDisposalTime) < Convert.ToDateTime(parentItemRule.DisposalAction)))))
                    {
                        return folderRule;
                    }
                    else
                    {
                        return parentItemRule;
                    }
                }
                else
                {
                    ArgumentNullException.ThrowIfNull(folder);
                    //Guid termId = Guid.Empty;
                    IAveFolder aveFolder = folder?.AveFolder;
                    itemRule.Rule = ruleManagement.CheckFolderCriteria(aveFolder);
                    if (itemRule.Rule != null)
                    {

                        itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";
                    }
                    else
                    {
                        string disposalAction = string.Empty;
                        itemRule.Rule = ruleManagement.GetDueDisposalRule(aveFolder, ref disposalAction);
                        itemRule.DisposalAction = disposalAction;
                    }
                }
            }
            return itemRule;

        }

        public SyncItemRuleInfo CheckDisposalRule(IAveFolder folder, SyncItemRuleInfo parentItemRule = null)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();

            DateTime parentDateTime;
            if (NeedCheckRule(folder?.Url))
            {
                if (parentItemRule?.Rule != null && !DateTime.TryParse(parentItemRule.DisposalAction, out parentDateTime))
                {
                    logger.Info("fit Parent rule now {0}:{1}", parentItemRule.Rule.Name, parentItemRule.DisposalAction);
                    //parent fit rule no need check rule .no need check rule again.
                    return parentItemRule;
                }
                else if (parentItemRule?.Rule != null && !string.IsNullOrEmpty(parentItemRule.DisposalAction))
                {
                    //parent get due disposal rule. and current disposal rule time maybe More recent
                    SyncItemRuleInfo folderRule = CheckDisposalRule(folder);
                    string folderDueDisposalTime = folderRule.DisposalAction;
                    if ((folderRule.Rule != null && (folderDueDisposalTime == "RDM_RecordsExporer_Status_NextJob" || folderDueDisposalTime == "Next Job"))
                            || (folderRule != null && ((Convert.ToDateTime(folderDueDisposalTime) < Convert.ToDateTime(parentItemRule.DisposalAction)))))
                    {
                        return folderRule;
                    }
                    else
                    {
                        return parentItemRule;
                    }
                }
                else
                {
                    //Guid termId = Guid.Empty;
                    itemRule.Rule = ruleManagement.CheckFolderCriteria(folder);
                    if (itemRule.Rule != null)
                    {

                        itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";
                    }
                    else
                    {
                        string disposalAction = string.Empty;
                        itemRule.Rule = ruleManagement.GetDueDisposalRule(folder, ref disposalAction);
                        itemRule.DisposalAction = disposalAction;
                    }
                }
            }
            return itemRule;

        }

        public SyncItemRuleInfo CheckDisposalRule(IAveListItem aveItem, SyncItemRuleInfo parentItemRule = null)
        {
            SyncItemRuleInfo itemRule = new SyncItemRuleInfo();
            
            Guid termId = Guid.Empty;
            DateTime parentDateTime;
            ArgumentCheck.CheckNotNull(aveItem);
            if (NeedCheckRule(aveItem?.Url))
            {
                if (parentItemRule != null && parentItemRule.Rule != null)
                {
                    if (parentItemRule.Rule != null && !DateTime.TryParse(parentItemRule.DisposalAction, out parentDateTime))
                    {
                        logger.Info("fit Parent rule now {0}:{1}", parentItemRule.Rule.Name, parentItemRule.DisposalAction);
                        //parent fit rule no need check rule .no need check rule again.
                        return parentItemRule;
                    }
                    else if (parentItemRule.Rule != null && !string.IsNullOrEmpty(parentItemRule.DisposalAction))
                    {
                        //parent get due disposal rule. and current disposal rule time maybe More recent
                        SyncItemRuleInfo currentRule = CheckDisposalRule(aveItem);
                        string itemDueDisposalTime = currentRule.DisposalAction;
                        if ((currentRule != null && (itemDueDisposalTime == "RDM_RecordsExporer_Status_NextJob" || itemDueDisposalTime == "Next Job"))
                            || (currentRule != null && ((Convert.ToDateTime(itemDueDisposalTime) < Convert.ToDateTime(parentItemRule.DisposalAction)))))
                        {
                            return currentRule;
                        }
                        else
                        {
                            return parentItemRule;
                        }
                    }
                }
                
                else if (!aveItem.CheckHasHold())
                {
                    using (new RA.Common.PerformanceScope("RMOneDriveRuleChecker.CheckItemCriteria", string.Format("RMOneDriveRuleChecker.CheckItemCriteria:{0}", aveItem?.ID/*discoverItem.FullUrl*/),true))
                    {
                        itemRule.Rule = ruleManagement.CheckItemCriteria(aveItem?.UniqueId ?? Guid.Empty, aveItem);
                    }
                    if (itemRule.Rule != null)
                    {
                        itemRule.DisposalAction = "RDM_RecordsExporer_Status_NextJob";
                    }
                    else
                    {
                        string disposalAction = string.Empty;
                        itemRule.Rule = ruleManagement.GetDueDisposalRule(aveItem, ref disposalAction);
                        itemRule.DisposalAction = disposalAction;
                    }
                    //RECO-2557
                    if (itemRule.Rule != null)
                    {
                        if (aveItem.CheckIsRecord())
                        {
                            if (!itemRule.Rule.DeleteRecords && !aveItem.IsBlockDeleteOnlyRecord() && !RuleHelper.CheckMoveRule(itemRule.Rule) && !RuleHelper.CheckArchiveOnlyRule(itemRule.Rule))
                            {
                                itemRule.Rule = null;
                                itemRule.DisposalAction = string.Empty;
                            }
                        }
                        if (aveItem.IsHaveRecordLabel())
                        {
                            if (!itemRule.Rule.IncludeDeleteRecordLabel && !RuleHelper.CheckMoveRule(itemRule.Rule) && !RuleHelper.CheckArchiveOnlyRule(itemRule.Rule))
                            {
                                itemRule.Rule = null;
                                itemRule.DisposalAction = string.Empty;
                            }
                        }
                    }

                }
            }
            return itemRule;

        }

        /*private List<Rule> CloneRules(List<Rule> rules)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rules);
            return SerializerHelper.DeserializeByDataContractSerializer<List<Rule>>(xml);
        }*/


        private bool NeedCheckRule(string url)
        {
            bool haveRule = _rules.Rules.Count != 0;
            if (!haveRule)
            {
                logger.Info("No rules realted {0}", url);
            }
            return haveRule;
        }

    }
}
