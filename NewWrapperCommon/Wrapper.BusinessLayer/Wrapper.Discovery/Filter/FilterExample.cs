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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;

namespace AvePoint.Wrapper.Discovery
{
    class FilterExample
    {
        public static void CommonExample()
        {
            List<FilterPolicy> policies = new List<FilterPolicy>();
            Dictionary<PolicyLevel, string> expressions = new Dictionary<PolicyLevel, string>();

            FilterPolicy siteCollectionPolicy = new FilterPolicy
            {
                SequenceNo = 1,
                Level = PolicyLevel.SiteCollection,
                Rule = new OwnerRule(),
                Condition = PolicyCondition.Contains,
                Value = new PolicyValue("AVEPOINT\\Sid")
            };
            policies.Add(siteCollectionPolicy);
            expressions.Add(PolicyLevel.SiteCollection, "(1)");

            FilterPolicy listPolicy = new FilterPolicy
            {
                SequenceNo = 2,
                Level = PolicyLevel.List,
                Rule = new UrlRule(),
                Condition = PolicyCondition.Contains,
                Value = new PolicyValue("Lists/Announcements")
            };
            policies.Add(listPolicy);
            expressions.Add(PolicyLevel.List, "(2)");

            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory("", null);
           
            using (IAveSite site = factory.CreateSite("http://SourceSite"))
            {
                using (AveDiscoverSite disSite = new AveDiscoverSite(site, null, AveDiscoveryKind.Database, DiscoverModule.Item))
                {
                    //想要对Discover进行过滤调用SetFilter方法即可.  FilterResultMode决定是否过滤
                    disSite.SetFilter(policies, expressions, FilterResultMode.Trim);//*******从哪个级别进入Discover，就得在哪个级别设置过滤
                    if (disSite.IsQualified())
                    {
                        //... do backup site
                        foreach (AveDiscoverWeb web in disSite.GetWebs().Values)
                        {
                            //... do backup web

                            //.....同理一直往下，所有返回的数据都是过滤的数据
                            web.GetLists();//这时候返回的List是已经过滤好的web
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Archiver独立分析过滤
        /// </summary>
        public static void ArchiverExample()
        {
            #region Init Rule
            FilterPolicy listPolicy = new FilterPolicy
            {
                SequenceNo = 1,
                Level = PolicyLevel.List,
                Rule = new UrlRule(),
                Condition = PolicyCondition.Contains,
                Value = new PolicyValue("Lists/Announcements")
            };
            var listRulePolices = new List<FilterPolicy>() { listPolicy };
            var listRuleExpressions = new Dictionary<PolicyLevel, string>();
            listRuleExpressions.Add(PolicyLevel.List, "(1)");
            FilterEngine listFilterEngine = new FilterEngine(listRulePolices, listRuleExpressions);

            FilterPolicy documentPolicy = new FilterPolicy
            {
                SequenceNo = 1,
                Level = PolicyLevel.Document,
                Rule = new TitleRule(),
                Condition = PolicyCondition.Contains,
                Value = new PolicyValue("Start use team site")
            };
            var documentRulePolices = new List<FilterPolicy>() { documentPolicy };
            var documentRuleExpressions = new Dictionary<PolicyLevel, string>();
            documentRuleExpressions.Add(PolicyLevel.Document, "(1)");
            FilterEngine documentFilterEngine = new FilterEngine(documentRulePolices, documentRuleExpressions);
            
            FilterPolicy versionPolicy = new FilterPolicy
            {
                SequenceNo = 1,
                Level = PolicyLevel.DocumentVersion,
                Rule = new VersionsRule(),
                Condition = PolicyCondition.OnlyLastNVersions,
                Value = new PolicyValue("5")
            };
            var versionRulePolices = new List<FilterPolicy>() { versionPolicy };
            var versionRuleExpressions = new Dictionary<PolicyLevel, string>();
            versionRuleExpressions.Add(PolicyLevel.DocumentVersion, "(1)");
            FilterEngine versionFilterEngine = new FilterEngine(versionRulePolices, versionRuleExpressions);
            #endregion

            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory("", null);

            using (IAveSite site = factory.CreateSite("http://SourceSite"))
            {
                using (AveDiscoverSite disSite = new AveDiscoverSite(site, null, AveDiscoveryKind.Database, DiscoverModule.Archive))
                {
                    //不调SetFilter方法

                    foreach (AveDiscoverWeb web in disSite.GetWebs().Values)
                    {
                        foreach (AveDiscoverList list in web.GetLists().Values)
                        {
                            ListInfo listInfo = (ListInfo)list.GetFilterObjectInfo(listRulePolices);
                            if(listFilterEngine.IsQualified(listInfo))
                            {
                                //... do archiver list
                                continue;
                            }
                            foreach (AveDiscoverItem item in list.GetRootFolder().GetItems())
                            {
                                if (item.ObjType == ItemType.Document)
                                {
                                    var docInfo = item.GetFilterObjectInfo(documentRulePolices);
                                    if (documentFilterEngine.IsQualified(docInfo))
                                    {
                                        //... do archiver document.
                                        continue;
                                    }
                                    foreach (AveVersionObject version in item.GetVersions())
                                    {
                                        ObjectInfoBase versionInfo = item.GetVersionObjectInfo(versionRulePolices, version.Uiversion);//得到Item的Version
                                        if (versionFilterEngine.IsQualified(versionInfo))
                                        {
                                            //... do archiver version.
                                        }
                                    }
                                }
                                else
                                {
                                    //item filter
                                    //Attachments
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
