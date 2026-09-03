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
using AvePoint.Wrapper.Common;
namespace AvePoint.Wrapper.Restore
{
    public interface IAveMetadataService
    {
        void CacheTermStoreInfo(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveTermStoreInfo> termStoreInfos);
        AvePoint.Wrapper.Common.IAveTaxonomyGroup CreateMetadataGroup(AvePoint.Wrapper.Common.IAveTermStore termStore, AvePoint.Wrapper.Common.AveMetadataGroupInfo groupInfo);
        AvePoint.Wrapper.Common.IAveTerm CreateSubTerm(AvePoint.Wrapper.Common.IAveTerm term, AvePoint.Wrapper.Common.AveTermInfo termInfo);
        AvePoint.Wrapper.Common.IAveTerm CreateTerm(AvePoint.Wrapper.Common.IAveTermSet termSet, AvePoint.Wrapper.Common.AveTermInfo termInfo);
        AvePoint.Wrapper.Common.IAveTermSet CreateTermSet(AvePoint.Wrapper.Common.IAveTaxonomyGroup group, AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo);
        AvePoint.Wrapper.Common.IAveTerm CreateUsedTermOnly(AvePoint.Wrapper.Common.IAveTermStore termStore, AvePoint.Wrapper.Common.IAveTermSet termSet, Guid termId);
        AvePoint.Wrapper.Common.IReport GetReport();
        void OutputDebugServiceInfo();
        void OutputDestServiceInfo(AvePoint.Wrapper.Common.IAveTaxonomySession session);
        void Restore(List<AveTermStoreInfo> termStoreInfos);
        void Restore(AvePoint.Wrapper.Common.AveManagedMetadataServiceApplicationInfo serviceAppInfo, Guid targetServiceAppId);
        void Restore(List<AveTermStoreInfo> termStoreInfos,AveMappingManager siteMappingManager, bool restoreManagedMetadataNavigation);
        AvePoint.Wrapper.Common.IAveTaxonomyGroup RestoreMetadataGroup(AvePoint.Wrapper.Common.IAveTermStore termStore, AvePoint.Wrapper.Common.AveMetadataGroupInfo groupInfo);
        AvePoint.Wrapper.Common.IAveTaxonomyGroup RestoreMetadataGroup(AvePoint.Wrapper.Common.IAveTermStore termStore, AvePoint.Wrapper.Common.AveMetadataGroupInfo groupInfo, AveTermStoreCacheInfo cacheInfo);
        AvePoint.Wrapper.Common.IAveTaxonomyGroup RestoreMetadataGroupSelf(AvePoint.Wrapper.Common.IAveTermStore termStore, AvePoint.Wrapper.Common.AveMetadataGroupInfo groupInfo);
        AvePoint.Wrapper.Common.IAveTerm RestoreSubTerm(AvePoint.Wrapper.Common.IAveTerm term, AvePoint.Wrapper.Common.AveTermInfo termInfo, bool isNewCreatedTerm);
        AvePoint.Wrapper.Common.IAveTerm RestoreSubTerm(AvePoint.Wrapper.Common.IAveTerm term, AvePoint.Wrapper.Common.AveTermInfo termInfo, bool isNewCreatedTerm, AveTermStoreCacheInfo cacheInfo);
        AvePoint.Wrapper.Common.IAveTerm RestoreSubTermSelf(AvePoint.Wrapper.Common.IAveTerm term, AvePoint.Wrapper.Common.AveTermInfo termInfo);
        AvePoint.Wrapper.Common.IAveTerm RestoreTerm(AvePoint.Wrapper.Common.IAveTermSet termSet, AvePoint.Wrapper.Common.AveTermInfo termInfo, bool isNewCreatedTermSet);
        AvePoint.Wrapper.Common.IAveTerm RestoreTerm(AvePoint.Wrapper.Common.IAveTermSet termSet, AvePoint.Wrapper.Common.AveTermInfo termInfo, bool isNewCreatedTermSet, AveTermStoreCacheInfo cacheInfo);
        AvePoint.Wrapper.Common.IAveTerm RestoreTermSelf(AvePoint.Wrapper.Common.IAveTermSet termSet, AvePoint.Wrapper.Common.AveTermInfo termInfo);
        AvePoint.Wrapper.Common.IAveTermSet RestoreTermSet(AvePoint.Wrapper.Common.IAveTaxonomyGroup group, AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo, bool isNewCreatedGroup);
        AvePoint.Wrapper.Common.IAveTermSet RestoreTermSet(AvePoint.Wrapper.Common.IAveTaxonomyGroup group, AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo, bool isNewCreatedGroup, AveTermStoreCacheInfo cacheInfo);
        AvePoint.Wrapper.Common.IAveTermSet RestoreTermSetSelf(AvePoint.Wrapper.Common.IAveTaxonomyGroup group, AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo);
        AvePoint.Wrapper.Common.IAveTermStore RestoreTermStore(AvePoint.Wrapper.Common.IAveTaxonomySession session, AvePoint.Wrapper.Common.AveTermStoreInfo termStoreInfo);
        AvePoint.Wrapper.Common.IAveTermStore RestoreTermStore(AvePoint.Wrapper.Common.IAveTaxonomySession session, AvePoint.Wrapper.Common.AveTermStoreInfo termStoreInfo, AveTermStoreCacheInfo cacheInfo);
        void RestoreTermStoreByCache(AvePoint.Wrapper.Common.IAveTaxonomySession session, AvePoint.Wrapper.Common.AveTermStoreInfo termStoreInfo);
        bool SkipGlobalTermGroup { get; set; }
        bool SkipLocalTermGroup { get; set; }
        bool RestoreTermProperties { get; set; }
        bool IsGetTermSetFromId { get; set; }
        Dictionary<Guid, Guid> TermStoreIdMapping { get; }
        Dictionary<Guid, Guid> TermGroupIdMapping { get; }
        Dictionary<Guid, Guid> TermSetIdMapping { get; }
        Dictionary<Guid, Guid> TermIdMapping { get; }
        bool TryFindSubTerm(AvePoint.Wrapper.Common.AveTermInfo termInfo, Guid termId, System.Collections.Generic.List<AvePoint.Wrapper.Common.AveTermInfo> listTerms);
        Guid TryResotreTermSet(Guid sspId, Guid groupId, Guid termSetId);
        Guid TryRestoreGroup(Guid sspId, Guid groupId);
        Guid TryRestoreSubTerms(AvePoint.Wrapper.Common.AveTermSetInfo termSetInfo, Guid termId, AvePoint.Wrapper.Common.IAveTermSet termSet);
        Guid TryRestoreTerm(Guid sspId, Guid groupId, Guid termSetId, Guid termId);
        Guid TryRestoreTermStore(Guid sspid);
        bool VerifyMetadataColumnValue(AvePoint.Wrapper.Common.IAveList List, Dictionary<string, string> fieldTermMapping, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping);
    }

    public class MetaDataServiceOption
    {
        public bool EnableCache = false;

        public bool IsFeatureGenerate = false;
        /// <summary>
        /// Restore Term信息时是否忽略Global的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipGlobalTermGroup { get; set; }

        /// <summary>
        /// Restore Term信息时是否忽略Local的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipLocalTermGroup { get; set; }

        /// <summary>
        /// Restore Term信息是是否还原term属性。
        /// </summary>
        public bool RestoreTermSetAndTermProperties { get; set; }
    }

    public class AveTermStoreCacheInfo
    {
        public DateTime LastAccessTime = DateTime.MinValue;
        public Guid UniqueId = Guid.Empty;
        public Dictionary<Guid, Guid> TermStoreIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermGroupIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermSetIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermIdMapping = new Dictionary<Guid, Guid>();
    }
}
