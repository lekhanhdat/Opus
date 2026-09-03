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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    internal class AveMetadataServiceCacheInfo
    {
        internal DateTime LastModifiedTime { get; set; }

        internal DateTime LastAccessTime { get; set; }

        internal AveTermStoreInfo TermStoreInfo { get; set; }

        public AveMetadataServiceCacheInfo()
        {
            LastModifiedTime = DateTime.MinValue;
            LastAccessTime = DateTime.MinValue;
            TermStoreInfo = null;
        }
    }

    public class AveMetadataService
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory objectModelFactory;
        private IAveSite mAveSite = null;

        public AveMetadataService(AveObjectModelFactory objectModelFactory)
        {
            this.objectModelFactory = objectModelFactory;
        }

        public AveMetadataService(IAveSite mAveSite)
        {
            this.mAveSite = mAveSite;
        }

        /// <summary>
        /// Export Term信息时是否忽略Global的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipGlobalTermGroup
        {
            get
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                return mAveSite.MetaDataServiceSerializer.SkipGlobalTermGroup;
            }
            set
            {
                if (mAveSite == null)
                {
                    throw new ArgumentNullException();
                }
                mAveSite.MetaDataServiceSerializer.SkipGlobalTermGroup = value;
            }
        }

        public void Export(IAveBackupStream output, Guid serviceApplicationId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveMetadataService.Export"))
            {
                IAveMetaDataServiceSerializer serilizer = this.objectModelFactory.CreateMetadataServiceSerilizer(serviceApplicationId);
                //等wrapper中serilizer.GetObjectData()方法修改之后再运行这个逻辑
                //AveManagedMetadataServiceApplicationInfo metadataServiceApplicationInfo = serilizer.GetObjectData() as AveManagedMetadataServiceApplicationInfo;
                //output.WriteMetadata(AveMetadataType.MetadataService, metadataServiceApplicationInfo);
                output.WriteMetadata(AveMetadataType.MetadataService, serilizer.GetObjectData());
            }
        }

        public void Export(IAveBackupStream output)
        {
            Export(output, false);
        }

        public void Export(IAveBackupStream output, bool enbaleCache)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.MetadataService"))
            {
                try
                {
                    if (enbaleCache)
                    {
                        mAveSite.MetaDataServiceSerializer.EnableCache = true;
                    }
                    //log
                    List<AveTermStoreInfo> mMetadataInfo = mAveSite.MetaDataServiceSerializer.GetObjectData() as List<AveTermStoreInfo>;
                    //Log
                    log.Debug(mMetadataInfo.ToLogString());
                    output.WriteMetadata(AveMetadataType.MetadataService, mMetadataInfo);
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupMetadataServiceFailedEventMessage(ex));
                }
            }
        }
    }
    public static class AveMedatadaServiceLog
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static string ToLogString(this List<AveTermStoreInfo> termStoreInfos)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (termStoreInfos != null)
            {
                foreach (AveTermStoreInfo info in termStoreInfos)
                {
                    if (info != null)
                    {
                        builder.AppendLine(ToLogString(info));
                    }
                }
            }
            return builder.ToString();
        }

        public static string ToLogString(AveTermStoreInfo store)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (store != null)
            {
                try
                {
                    builder.AppendLine("TermStore:");
                    builder.AppendFormat("[TermStore:{0},{1},{2},{3},{4},{5},{6}]", store.Id, store.Name, store.DefaultLanguage, store.WorkingLanguage, store.LastAccessTime, store.UniqueId, store.OperationType);
                    builder.AppendLine("");
                    builder.AppendLine("     TermGroups:");
                    if (store.Groups != null && store.Groups.Count > 0)
                    {
                        foreach (AveMetadataGroupInfo group in store.Groups)
                        {
                            try
                            {
                                builder.AppendFormat("     [TermGroup:{0},{1},{2},{3},{4},{5}]", group.Id, group.Name, group.IsSiteCollectionGroup, group.IsSystemGroup, group.OperationType, group.Description);
                                builder.AppendLine("");
                                builder.AppendLine("          TermSets:");
                                foreach (AveTermSetInfo set in group.TermSets)
                                {
                                    try
                                    {
                                        builder.AppendFormat("          [TermSet:{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}]", set.Id, set.Name, set.ParentId, set.Contact, set.CustomSortOrder, set.Description, set.IsAvailableForTagging, set.IsOpenForTermCreation, set.OperationType, set.Owner);
                                        builder.AppendLine("");
                                        builder.AppendLine("               Terms:");
                                        foreach (AveTermInfo term in set.Terms)
                                        {
                                            builder.AppendLine(GetTermInfoString(term));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Warn("An error occurred while get term set info string.{0},{1}", set.Name, e);
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while get term group info string.{0},{1}", group.Name, e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while get term store info string.{0},{1}", store.Name, e);
                }
            }
            return builder.ToString();
        }

        private static string GetTermInfoString(AveTermInfo term)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            try
            {
                builder.AppendFormat("               [Term:{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                      term.Id, term.Name, term.IsKeyword, term.IsRoot,
                      term.SourceTermId, term.SourceTermName,
                      term.ParentTermSetId, term.ParentTermId,
                      term.IsAvailableForTagging, term.IsDeprecated, term.IsReused,
                      term.IsPinned, term.PinSourceTermSetId, term.IsSourceTerm,
                      term.Owner, term.CustomSortOrder, term.OperationType);
                if (term.Labels != null)
                {
                    foreach (var label in term.Labels)
                    {
                        builder.AppendFormat("<Label {0},{1},{2},{3}>", label.Value, label.IsDefaultForLanguage, label.Language, label.Description);
                    }
                }
                foreach (var subTerm in term.Terms)
                {
                    builder.Append(GetTermInfoString(subTerm));
                }
                builder.Append("]");
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while get term info string.{0},{1}", term.Name, e);
            }
            return builder.ToString();
        }
    }
}