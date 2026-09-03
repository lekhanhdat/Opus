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
using System.IO;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Restore
{
    /// <summary>
    ///  TermGroup Wrapper Restore
    /// </summary>
    public class AveTaxonomyGroup : IMMSRestore, IDisposable
    {
        #region  << Property >>
        private static AveLogger sLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveTermStore termStore;
        private AveTermStore aveTermStore;
        private IAveTaxonomyGroup taxonomyGroup;
        public AveObjectModelFactory ObjectModelFactory;
        public bool IsNewCreated = false;
        private string targetGroupName = string.Empty;
        private bool isLocalGroup = false;

        private AveMetadataGroupInfo mGroupInfo;
        private AveRestoreOption mRestoreOption;
        private AveTaxonomyUserMapping mUserMapping;
        private AveTaxonomyUserMappingUtility mUserMappingUtility;
        private IAveSite localSite;
        private IAveMetadataServiceApplication metadataServiceApplication;

        public IAveTaxonomyGroup TaxonomyGroup
        {
            get { return this.taxonomyGroup; }
            set { this.taxonomyGroup = value; }
        }

        public AveTermStore TermStore
        {
            get { return this.aveTermStore; }
            set { this.aveTermStore = value; }
        }

        public AveTaxonomyUserMappingUtility UserMappingUtility
        {
            get { return mUserMappingUtility; }
        }

        #endregion  << Property >>

        #region << Constructor >>
        public AveTaxonomyGroup(AveTermStore aveTermStore, string targetGroupName, bool isLocalGroup,
            AveMetadataGroupInfo groupInfo, AveRestoreOption restoreOption, AveTaxonomyUserMapping userMapping)
        {
            this.aveTermStore = aveTermStore;
            this.termStore = this.aveTermStore.TermStore;
            this.ObjectModelFactory = this.aveTermStore.ObjectModelFactory;
            this.targetGroupName = targetGroupName;
            this.isLocalGroup = isLocalGroup;
            this.mGroupInfo = groupInfo;
            this.mRestoreOption = restoreOption;
            this.mUserMapping = userMapping;
            this.mUserMappingUtility = aveTermStore.UserMappingUtility;
        }
        #endregion << Constructor >>

        #region << Exists >>
        /// <summary>
        /// 提供给外围判定对象是否存在
        /// </summary>
        /// <returns>True:存在 False:不存在</returns>
        public bool Exists()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyGroup.Exists"))
            {
#endif
                IAveTaxonomyGroup group = null;
                string groupName = string.Empty;

                try
                {
                    if (string.IsNullOrEmpty(this.targetGroupName))
                    {
                        groupName = mGroupInfo.Name;
                        group = this.termStore.Groups[mGroupInfo.Name];
                    }
                    else
                    {
                        groupName = this.targetGroupName;
                        group = this.termStore.Groups[this.targetGroupName];
                    }
                }
                catch (Exception e)
                {
                    sLogger.Info("Cannot get local term group in Destination. termGroupName:{0}. Reason:{1}.", groupName, e.ToString());
                    try
                    {
                        if (group == null && isLocalGroup)
                        {
                            group = GetLocalGroup();
                        }
                    }
                    catch (Exception ex)
                    {
                        sLogger.Info("Cannot get term group in Destination. termGroupName:{0}. Reason:{1}.", groupName, ex.ToString());
                    }
                }
                if (group == null)
                {
                    //原端选择system group时，如果目的端按照name找不到对应的group，直接得到目的端的system group
                    if (this.mGroupInfo.IsSystemGroup)
                    {
                        group = this.termStore.SystemGroup;
                    }
                    this.taxonomyGroup = group;
                    if (group == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    this.taxonomyGroup = group;
                    return true;
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << Exists >>

        #region << Create >>
        /// <summary>
        /// 创建Term Group对象
        /// </summary>
        public void Create()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyGroup.Create"))
            {
#endif
                try
                {
                    IAveTaxonomyGroup group;
                    if (string.IsNullOrEmpty(this.targetGroupName))
                    {
                        group = termStore.CreateGroup(mGroupInfo.Name);
                    }
                    else
                    {
                        group = termStore.CreateGroup(this.targetGroupName);
                    }
                    this.taxonomyGroup = group;
                    this.IsNewCreated = true;
                    this.termStore.CommitAll();

                    //更新属性
                    Update();
                }
                catch (Exception e)
                {
                    if (string.IsNullOrEmpty(this.targetGroupName))
                    {
                        sLogger.Warn("An error occurred when creating taxonomy group. group name:{0}.\r\n error:{1}", mGroupInfo.Name, e.ToString());
                        throw new AveException("An error occurred when creating taxonomy group. group name:{0}.\r\n error:{1}", mGroupInfo.Name, e.Message.ToString());
                    }
                    else
                    {
                        sLogger.Warn("An error occurred when creating taxonomy group. group name:{0}.\r\n error:{1}", this.targetGroupName, e.ToString());
                        throw new AveException("An error occurred when creating taxonomy group. group name:{0}.\r\n error:{1}", this.targetGroupName, e.Message.ToString());
                    }
                }

#if PerformanceLog
            }
#endif
        }
        #endregion << Create >>

        #region << Update >>
        /// <summary>
        /// 更新Term Group对象
        /// </summary>
        public void Update()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyGroup.ResetAvailableName"))
            {
#endif
                if (this.taxonomyGroup != null)
                {
                    //RestoreProperty
                    if (mRestoreOption.TaxonomyGroupRestoreOption.RESTOREPROPERTIES)
                    {
                        UpdateMetadataGroupProperty(this.taxonomyGroup, mGroupInfo);
                    }

                    //RestoreSecurity
                    if (mRestoreOption.TaxonomyGroupRestoreOption.RESTORESECURITY)
                    {
                        UpdateMetadataGroupSecurity(this.taxonomyGroup, mGroupInfo);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.targetGroupName))
                    {
                        throw new AveException("Term Group object is null in Destination. term group name:{0}.", mGroupInfo.Name);
                    }
                    else
                    {
                        throw new AveException("Term Group object is null in Destination. term group name:{0}.", this.targetGroupName);
                    }
                }

#if PerformanceLog
            }
#endif
        }
        #endregion << Update >>

        #region << DeleteChildren >>
        /// <summary>
        /// 清空子结点支持Replace
        /// </summary>
        public void DeleteChildren()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyGroup.DeleteChildren"))
            {
#endif
                if (this.taxonomyGroup != null)
                {
                    if (this.taxonomyGroup.IsSystemGroup)
                    {
                        IAveTermSet termSet = this.termStore.KeywordsTermSet;
                        foreach (IAveTerm term in termSet.Terms)
                        {
                            term.Delete();
                        }
                    }
                    else
                    {
                        foreach (IAveTermSet termSet in this.taxonomyGroup.TermSets)
                        {
                            termSet.Delete();
                        }
                    }
                    this.termStore.CommitAll();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.targetGroupName))
                    {
                        throw new AveException("TermGroup object is null in Destination. termGroupName:{0}.", mGroupInfo.Name);
                    }
                    else
                    {
                        throw new AveException("TermGroup object is null in Destination. termGroupName:{0}.", this.targetGroupName);
                    }
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << DeleteChildren >>

        #region << Update Property >>
        /// <summary>
        /// 更新Property
        /// </summary>
        /// <param name="group">接口Wrapper对象</param>
        /// <param name="groupInfo">更新内容</param>
        private void UpdateMetadataGroupProperty(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            try
            {
                group.Description = groupInfo.Description;
                group.TermStore.CommitAll();
            }
            catch (Exception ex)
            {
                sLogger.Warn("An error occurred when updating taxonomy group. groupName:{0}. \r\n error:{1}", group.Name, ex.ToString());
            }
        }
        #endregion << Update Property >>

        #region << Update Security >>
        /// <summary>
        /// 更新Security
        /// </summary>
        /// <param name="group">接口Wrapper对象</param>
        /// <param name="groupInfo">更新内容</param>
        private void UpdateMetadataGroupSecurity(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTaxonomyGroup.UpdateMetadataGroupSecurity"))
            {
#endif
                const string prefix = "i:0#.w|";
                AveMetadataGroupInfo desGroupInfo = group.TaxonomyGroupSerializer.GetObjectData();
                foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
                {
                    //UserMapping
                    bool isSharepointGroup = false;
                    string userName = string.Empty;

                    try
                    {
                        userName = mUserMappingUtility.GetMappingUserLogin(groupManager.PrincipalName, mUserMapping.UserMappings, mUserMapping.DomainMappings, out isSharepointGroup);

                        //取出两端的user进行比对，防止同一user不同格式被API加入多次
                        bool finded = false;
                        foreach (AveAceInfo desGroupManager in desGroupInfo.GroupManagers)
                        {
                            bool isGroup = false;
                            string currentUser = mUserMappingUtility.GetMappingUserLogin(desGroupManager.PrincipalName, null, null, out isGroup);
                            if (currentUser.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                currentUser = currentUser.Replace(prefix,"");
                            }
                            if (userName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                userName = userName.Replace(prefix, "");
                            }
                            if (currentUser.Equals(userName, StringComparison.OrdinalIgnoreCase))
                            {
                                finded = true;
                                break;
                            }
                        }
                        if (!finded)
                        {
                            group.AddGroupManager(userName);
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            sLogger.Warn("An error occurred when add a user to manager group. userName:{0}. \r\n error:{1}", userName, e.ToString());
                            if (!isSharepointGroup && !string.IsNullOrEmpty(mUserMapping.TargetDefaultUser))
                            {
                                group.AddGroupManager(mUserMapping.TargetDefaultUser);
                            }
                        }
                        catch (Exception ex)
                        {
                            sLogger.Warn("An error occurred when add a user to manager group. userName:{0}. \r\n error:{1}", mUserMapping.TargetDefaultUser, ex.ToString());
                        }
                    }
                }
                foreach (AveAceInfo contributor in groupInfo.Contributors)
                {
                    //UserMapping
                    bool isSharepointGroup = false;
                    string userName = string.Empty;

                    try
                    {
                        userName = mUserMappingUtility.GetMappingUserLogin(contributor.PrincipalName, mUserMapping.UserMappings, mUserMapping.DomainMappings, out isSharepointGroup);

                        bool finded = false;
                        foreach (AveAceInfo desContributor in desGroupInfo.Contributors)
                        {
                            bool isGroup = false;
                            string currentUser = mUserMappingUtility.GetMappingUserLogin(desContributor.PrincipalName, null, null, out isGroup);
                            if (currentUser.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                currentUser = currentUser.Replace(prefix, "");
                            }
                            if (userName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                userName = userName.Replace(prefix, "");
                            }
                            if (currentUser.Equals(userName, StringComparison.OrdinalIgnoreCase))
                            {
                                finded = true;
                                break;
                            }
                        }
                        if (!finded)
                        {
                            group.AddContributor(userName);
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            sLogger.Warn("An error occurred when add user to contributor group. userName:{0}. \r\n error:{1}", userName, e.ToString());
                            if (!isSharepointGroup && !string.IsNullOrEmpty(mUserMapping.TargetDefaultUser))
                            {
                                group.AddContributor(mUserMapping.TargetDefaultUser);
                            }
                        }
                        catch (Exception ex)
                        {
                            sLogger.Warn("An error occurred when add user to contributor group. userName:{0}. \r\n error:{1}", mUserMapping.TargetDefaultUser, ex.ToString());
                        }

                    }
                }
                group.TermStore.CommitAll();
#if PerformanceLog
            }
#endif
        }
        #endregion << Update Security >>

        #region << GetLocalGroup >>
        private IAveTaxonomyGroup GetLocalGroup()
        {
            this.metadataServiceApplication = this.ObjectModelFactory.CreateMetadataServiceApplication(this.TermStore.AppilicationId);
            AveMetadataGroupInfo groupInfo = this.metadataServiceApplication.GetGroup(this.targetGroupName);
            if (groupInfo.Sites.Count > 0)
            {
                Guid siteid = groupInfo.Sites[0];
                this.localSite = this.ObjectModelFactory.CreateSite(siteid);
                this.termStore = this.ObjectModelFactory.CreateTaxonomySession(this.localSite).TermStores[this.aveTermStore.TermStore.ID];
                return this.termStore.Groups[this.targetGroupName];
            }
            return null;
        }
        #endregion << GetLocalGroup >>

        #region << Dispose >>
        public void Dispose()
        {
            if (this.localSite != null)
            {
                this.localSite.Dispose();
            }
            if (this.metadataServiceApplication != null)
            {
                this.metadataServiceApplication.Dispose();
            }
        }
        #endregion << Dispose >>

    }
}