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




namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Common.FilterEngine.Engines.Box;
    using AvePoint.Common.FilterEngine.Engines.Connector;
    using AvePoint.Common.FilterEngine.Engines.Google;
    using AvePoint.Common.FilterEngine.Engines.Teams;
    using AvePoint.Common.FilterEngine.ObjectInfos;
    using AvePoint.Common.FilterEngine.ObjectInfos.Connector;
    using AvePoint.GCommon.Contract.CommonFilter;
    #endregion

    public class FilterEngine
    {
        private List<FilterPolicy> filterPolicies = new List<FilterPolicy>();
        private Dictionary<PolicyLevel, string> filterConditionExpressions;

        public bool IsFilterOut { get; set; }
        public bool SkipCheckDateTimeMinValue { get; set; } = false;

        public FilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, bool isFilterOut = false, bool skipCheckDateTimeMinValue = false)
        {
            filterPolicies = policyLists;
            filterConditionExpressions = filterConditionExpressionLists;
            IsFilterOut = isFilterOut;
            SkipCheckDateTimeMinValue = skipCheckDateTimeMinValue;
        }

        public bool IsFilterExist(PolicyLevel level)
        {
            foreach (FilterPolicy fp in filterPolicies)
            {
                if (fp.Level == level) return true;
            }
            return false;
        }

        internal virtual IFilterEngine GetFilterEngine(ObjectInfoBase objectInfo)
        {
            if (objectInfo is WebAppInfo)
            {
                return new WebApplicationFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is SiteCollectionInfo)
            {
                return new SiteCollectionFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is SiteInfo)
            {
                return new SiteFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ListInfo)
            {
                return new ListFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is FolderInfo)
            {
                return new FolderFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is DocumentInfo)
            {
                return new DocumentFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is DocumentVersionInfo)
            {
                return new DocumentVersionFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ItemInfo)
            {
                return new ItemFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ItemVersionInfo)
            {
                return new ItemVersionFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is AttachmentInfo)
            {
                return new AttachmentFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is TreeNodeInfo)
            {
                return new TreeNodeFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeMailboxInfo)
            {
                return new ExchangeMailboxFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeFolderInfo)
            {
                return new ExchangeFolderFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeTaskInfo)
            {
                return new ExchangeTaskFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeContactInfo)
            {
                return new ExchangeContactFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeDocumentInfo)
            {
                return new ExchangeDocumentFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeEventInfo)
            {
                return new ExchangeEventFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeJournalInfo)
            {
                return new ExchangeJournalFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeMessageInfo)
            {
                return new ExchangeMessageFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangeNoteInfo)
            {
                return new ExchangeNoteFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is ExchangePostInfo)
            {
                return new ExchangePostFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is PhysicalBoxInfo)
            {
                return new PhysicalBoxFilterEngine(filterPolicies, filterConditionExpressions, this, SkipCheckDateTimeMinValue);
            }
            if (objectInfo is PhysicalFileInfo)
            {
                return new PhysicalFileFilterEngine(filterPolicies, filterConditionExpressions, this, SkipCheckDateTimeMinValue);
            }
            if (objectInfo is FSFileInfo)
            {
                return new FSFileFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is FSFolderInfo)
            {
                return new FSFolderFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is AzureFileInfo)
            {
                return new AzureFileFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is CustomizeConnectorItemInfo)
            {
                return new CustomizeConnectorFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is BoxItemInfo)
            {
                return new BoxFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if (objectInfo is GoogleItemInfo)
            {
                return new GoogleFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            if(objectInfo is TeamsInfo)
            {
                return new TeamsFilterEngine(filterPolicies, filterConditionExpressions, this);
            }
            throw new LevelNotSupportedException(objectInfo.GetType().FullName);
        }


        public bool IsQualified(ObjectInfoBase objectInfo)
        {
            ArgumentNullException.ThrowIfNull(objectInfo);

            using var _ = objectInfo.BeginPropertyCheck();
            using (var filterEngine = GetFilterEngine(objectInfo))
            {
                return filterEngine.IsQualified(objectInfo);
            }
        }
    }
}
