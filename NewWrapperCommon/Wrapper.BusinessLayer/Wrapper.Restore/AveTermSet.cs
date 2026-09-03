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
    #region Move to DPM TaxonomyWrapperControl. Please visit JIRA: ADO-133194 to get more information.
    /// <summary>
    ///  TermSet Wrapper Restore
    /// </summary>
    //public class AveTermSet : IMMSRestore, AvePoint.Wrapper.Restore.IAveSPTermSet
    //{
    //    #region << Property >>
    //    private static AveLogger sLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
    //    private AveTaxonomyGroup aveTaxonomyGroup;
    //    private IAveTaxonomyGroup taxonomyGroup;
    //    private IAveTermSet termSet;
    //    public AveObjectModelFactory ObjectModelFactory;
    //    public bool IsNewCreated = false;
    //    private string targetTermSetName = string.Empty;
    //    public Dictionary<string, string> termIdMapping = new Dictionary<string, string>();
    //    private string customSortOrder = string.Empty;

    //    private AveTermSetInfo mTermSetInfo;
    //    private AveRestoreOption mRestoreOption;
    //    private AveTaxonomyUserMapping mUserMapping;
    //    private AveTaxonomyUserMappingUtility mUserMappingUtility;
    //    private Guid targetObjectId;
    //    private bool mUpdateName = false;

    //    public IAveTermSet TermSet
    //    {
    //        get { return this.termSet; }
    //        set { this.termSet = value; }
    //    }

    //    public AveTaxonomyGroup TaxonomyGroup
    //    {
    //        get { return this.aveTaxonomyGroup; }
    //        set { this.aveTaxonomyGroup = value; }
    //    }

    //    public AveTaxonomyUserMappingUtility UserMappingUtility
    //    {
    //        get { return mUserMappingUtility; }
    //    }

    //    #endregion << Property >>

    //    #region << Constructor >>
    //    public AveTermSet(AveTaxonomyGroup group, string targetTermSetName, AveTermSetInfo termSetInfo, AveRestoreOption restoreOption, AveTaxonomyUserMapping userMapping, Guid targetId, bool updateName)
    //        : this(group, targetTermSetName, termSetInfo, restoreOption, userMapping)
    //    {
    //        mUpdateName = updateName;
    //        targetObjectId = targetId;
    //    }
    //    public AveTermSet(AveTaxonomyGroup group, string targetTermSetName, AveTermSetInfo termSetInfo, AveRestoreOption restoreOption, AveTaxonomyUserMapping userMapping)
    //    {
    //        this.aveTaxonomyGroup = group;
    //        this.taxonomyGroup = this.aveTaxonomyGroup.TaxonomyGroup;
    //        this.ObjectModelFactory = this.aveTaxonomyGroup.ObjectModelFactory;
    //        this.targetTermSetName = targetTermSetName;
    //        this.mTermSetInfo = termSetInfo;
    //        this.mRestoreOption = restoreOption;
    //        this.mUserMapping = userMapping;
    //        this.mUserMappingUtility = group.UserMappingUtility;

    //    }
    //    #endregion << Constructor >>

    //    #region << Exists >>
    //    /// <summary>
    //    /// 提供给外围判定对象是否存在
    //    /// </summary>
    //    /// <returns>True:存在 False:不存在</returns>
    //    public bool Exists()
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.Exists"))
    //        {

    //            IAveTermSet tempTermSet = GetTargetTermSet();
    //            if (tempTermSet == null)
    //            {
    //                //目的端选SystemGroup时原端只能选keywords的termset
    //                //按照name找不到对应的termset，那么直接从store里找KeywordsTermSet
    //                if (this.taxonomyGroup.IsSystemGroup)
    //                {
    //                    tempTermSet = this.taxonomyGroup.TermStore.KeywordsTermSet;
    //                }
    //                this.termSet = tempTermSet;
    //                if (tempTermSet == null)
    //                {
    //                    return false;
    //                }
    //                else
    //                {
    //                    return true;
    //                }
    //            }
    //            else
    //            {
    //                return true;
    //            }

    //        }

    //    }
    //    /// <summary>
    //    /// 通过Name或者是Id查找目的端对应的TermSet
    //    /// </summary>
    //    /// <returns></returns>
    //    private IAveTermSet GetTargetTermSet()
    //    {
    //        IAveTermSet targetTermSet = null;
    //        string termSetName = string.Empty;

    //        if (!this.aveTaxonomyGroup.IsNewCreated)
    //        {
    //            try
    //            {
    //                if (string.IsNullOrEmpty(targetTermSetName))
    //                {
    //                    termSetName = mTermSetInfo.Name;
    //                    targetTermSet = taxonomyGroup.TermSets[mTermSetInfo.Name];
    //                }
    //                else if (targetObjectId.Equals(Guid.Empty))
    //                {
    //                    termSetName = targetTermSetName;
    //                    targetTermSet = taxonomyGroup.TermSets[targetTermSetName];
    //                }
    //                else
    //                {
    //                    termSetName = targetTermSetName;
    //                    targetTermSet = taxonomyGroup.TermSets[targetObjectId];
    //                }
    //                this.termSet = targetTermSet;
    //            }
    //            catch (Exception e)
    //            {
    //                sLogger.Info("Cannot get term set in Destination. termSetName:{0}. Reason:{1}.", termSetName, e.ToString());
    //            }
    //        }
    //        return targetTermSet;
    //    }
    //    #endregion << Exists >>

    //    #region << Create >>
    //    /// <summary>
    //    /// 创建Term Set对象
    //    /// </summary>
    //    public void Create()
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.Create"))
    //        {

    //            try
    //            {
    //                IAveTermSet termSet;
    //                if (string.IsNullOrEmpty(this.targetTermSetName))
    //                {
    //                    termSet = this.taxonomyGroup.CreateTermSet(mTermSetInfo.Name, mTermSetInfo.Id);
    //                }
    //                else
    //                {
    //                    termSet = this.taxonomyGroup.CreateTermSet(this.targetTermSetName, mTermSetInfo.Id);
    //                }
    //                this.termSet = termSet;
    //                this.IsNewCreated = true;
    //                this.taxonomyGroup.TermStore.CommitAll();

    //                //更新属性
    //                Update();
    //            }
    //            catch (Exception e)
    //            {
    //                if (string.IsNullOrEmpty(this.targetTermSetName))
    //                {
    //                    sLogger.Warn("An error occurred when creating term set.targetTermSetName is null, termSet Name:{0}, error:{1}", mTermSetInfo.Name, e.ToString());
    //                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_CreateTermSetError , mTermSetInfo.Name, e.Message.ToString());
    //                }
    //                else
    //                {
    //                    sLogger.Warn("An error occurred when creating term set. TermSet name:{0}, error:{1}", this.targetTermSetName, e.ToString());
    //                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_CreateTermSetError, this.targetTermSetName, e.Message.ToString());
    //                }                           
    //            }


    //        }

    //    }
    //    #endregion << Create >>

    //    #region << Update >>
    //    /// <summary>
    //    /// 更新Term Set对象
    //    /// </summary>
    //    public void Update()
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.Update"))
    //        {

    //            if (this.termSet != null)
    //            {
    //                //Restore Property
    //                if (mRestoreOption.TermSetRestoreOption.RESTOREPROPERTIES)
    //                {
    //                    UpdateTermSetProperty(this.termSet, mTermSetInfo, mUpdateName);
    //                }
    //                //Restore Security
    //                if (mRestoreOption.TermSetRestoreOption.RESTORESECURITY)
    //                {
    //                    UpdateTermSetSecurity(this.termSet, mTermSetInfo);
    //                }
    //            }
    //            else
    //            {
    //                if (string.IsNullOrEmpty(this.targetTermSetName))
    //                {
    //                    throw new AveException("TermSet object is null in destination,targetTermSetName is null. TermSet name:{0}.", mTermSetInfo.Name);
    //                }
    //                else
    //                {
    //                    throw new AveException("TermSet object is null in destination. TermSet name:{0}.", this.targetTermSetName);
    //                }
    //            }

    //        }

    //    }
    //    #endregion << Update >>

    //    #region << DeleteChildren >>
    //    /// <summary>
    //    /// 清空子结点支持Replace
    //    /// </summary>
    //    public void DeleteChildren()
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.DeleteChildren"))
    //        {

    //            if (this.termSet != null)
    //            {
    //                foreach (IAveTerm term in termSet.Terms)
    //                {
    //                    term.Delete();
    //                }
    //                termSet.TermStore.CommitAll();
    //            }
    //            else
    //            {
    //                if (string.IsNullOrEmpty(this.targetTermSetName))
    //                {
    //                    throw new AveException("TermSet object is null in destination.targetTermSetName is null. TermSet name:{0}.", mTermSetInfo.Name);
    //                }
    //                else
    //                {
    //                    throw new AveException("TermSet object is null in destination. TermSet name:{0}.", this.targetTermSetName);
    //                }
    //            }


    //        }

    //    }
    //    #endregion << DeleteChildren >>

    //    #region << Update Property >>
    //    /// <summary>
    //    /// 还原Property
    //    /// </summary>
    //    /// <param name="termSet">接口Wrapper对象</param>
    //    /// <param name="termSetInfo">更新内容</param>
    //    private void UpdateTermSetProperty(IAveTermSet termSet, AveTermSetInfo termSetInfo, bool isUpdateName)
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.UpdateTermSetProperty"))
    //        {

    //            try
    //            {
    //                termSet.Description = termSetInfo.Description;
    //                if (isUpdateName)
    //                {
    //                    termSet.Name = termSetInfo.Name;
    //                }
    //                //对于Keywords TermSet不能覆盖Contact和Submission Policy属性
    //                if (termSet.ID != termSet.TermStore.KeywordsTermSet.ID)
    //                    termSet.Contact = termSetInfo.Contact;
    //                if (termSet.ID != termSet.TermStore.KeywordsTermSet.ID)
    //                    termSet.IsOpenForTermCreation = termSetInfo.IsOpenForTermCreation;
    //                termSet.IsAvailableForTagging = termSetInfo.IsAvailableForTagging;
    //                //CustomSortOrder
    //                if (!string.IsNullOrEmpty(termSetInfo.CustomSortOrder))
    //                {
    //                    customSortOrder = termSetInfo.CustomSortOrder;
    //                }

    //                bool isRestoreNavigationProperty = false;
    //                //add for restore custom property                
    //                if (termSetInfo.CustomProperties != null && termSetInfo.CustomProperties.Count > 0)
    //                {
    //                    if ((!termSet.CustomProperties.ContainsKey("_Sys_Nav_IsNavigationTermSet") || termSet.CustomProperties["_Sys_Nav_IsNavigationTermSet"] == "False") &&
    //                        (termSetInfo.CustomProperties.ContainsKey("_Sys_Nav_IsNavigationTermSet") && termSetInfo.CustomProperties["_Sys_Nav_IsNavigationTermSet"] == "True"))
    //                    {
    //                        isRestoreNavigationProperty = true;
    //                    }

    //                    foreach (KeyValuePair<string, string> pair in termSetInfo.CustomProperties)
    //                    {
    //                        if (!isRestoreNavigationProperty && pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
    //                            continue;
    //                        if (pair.Key.StartsWith("_Sys_Facet_IsFacetedTermSet", StringComparison.OrdinalIgnoreCase))
    //                            continue;
    //                        termSet.SetCustomProperty(pair.Key, pair.Value);
    //                    }
    //                }

    //                termSet.TermStore.CommitAll();
    //            }
    //            catch (Exception excep)
    //            {
    //                sLogger.Warn("An error occurred when updating term set. Term set name:" + termSet.Name + "\r\nError:" + excep.ToString());
    //            }


    //        }

    //    }
    //    #endregion << Update Property >>

    //    #region << Update Security >>
    //    /// <summary>
    //    /// 还原Security
    //    /// </summary>
    //    /// <param name="termSet">接口Wrapper对象</param>
    //    /// <param name="termSetInfo">更新内容</param>
    //    public void UpdateTermSetSecurity(IAveTermSet termSet, AveTermSetInfo termSetInfo)
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.UpdateTermSetSecurity"))
    //        {

    //            //UserMapping
    //            bool isSharepointGroup = false;
    //            string userName = string.Empty;

    //            try
    //            {
    //                userName = mUserMappingUtility.GetMappingUserLogin(termSetInfo.Owner, mUserMapping.UserMappings, mUserMapping.DomainMappings, out isSharepointGroup);
    //                if (!string.IsNullOrEmpty(userName)) termSet.Owner = userName;
    //            }
    //            catch (Exception e)
    //            {
    //                try
    //                {
    //                    sLogger.Warn("An error occurred when setting term set owner. term set:{0}, owner:{1}. \r\n error:{2}", termSet.Name, userName, e.ToString());
    //                    if (!isSharepointGroup && !string.IsNullOrEmpty(mUserMapping.TargetDefaultUser))
    //                    {
    //                        termSet.Owner = mUserMapping.TargetDefaultUser;
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    sLogger.Warn("An error occurred when setting term set owner. term set:{0}, owner:{1}. \r\n error:{2}", termSet.Name, mUserMapping.TargetDefaultUser, ex.ToString());
    //                }

    //            }

    //            foreach (string stakeHolder in termSetInfo.Stakeholders)
    //            {
    //                try
    //                {
    //                    //UserMapping
    //                    userName = mUserMappingUtility.GetMappingUserLogin(stakeHolder, mUserMapping.UserMappings, mUserMapping.DomainMappings, out isSharepointGroup);

    //                    //ADO-8048
    //                    bool finded = false;
    //                    foreach (string holders in termSet.Stakeholders)
    //                    {
    //                        bool isGroup = false;
    //                        string currentUser = mUserMappingUtility.GetMappingUserLogin(holders, null, null, out isGroup);
    //                        if (currentUser.Equals(userName, StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            finded = true;
    //                            break;
    //                        }
    //                    }
    //                    if (!finded)
    //                    {
    //                        termSet.AddStakeholder(userName);
    //                    }
    //                }
    //                catch (Exception e)
    //                {
    //                    try
    //                    {
    //                        sLogger.Warn("An error occurred when adding term set stakeholder. Term set:{0}, stakeholder:{1}. \r\n error:{2}.", termSet.Name, stakeHolder, e.ToString());
    //                        if (!isSharepointGroup && !string.IsNullOrEmpty(mUserMapping.TargetDefaultUser))
    //                        {
    //                            bool finded = false;
    //                            foreach (string holders in termSet.Stakeholders)
    //                            {
    //                                bool isGroup = false;
    //                                string currentUser = mUserMappingUtility.GetMappingUserLogin(holders, null, null, out isGroup);
    //                                if (holders.Equals(mUserMapping.TargetDefaultUser, StringComparison.OrdinalIgnoreCase))
    //                                {
    //                                    finded = true;
    //                                    break;
    //                                }
    //                            }
    //                            if (!finded)
    //                            {
    //                                termSet.AddStakeholder(mUserMapping.TargetDefaultUser);
    //                            }
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        sLogger.Warn("An error occurred when adding term set stakeholder. Term set:{0}, stakeholder:{1}. \r\n error:{2}.", termSet.Name, mUserMapping.TargetDefaultUser, ex.ToString());
    //                    }

    //                }
    //            }
    //            termSet.TermStore.CommitAll();


    //        }

    //    }
    //    #endregion << Update Security >>

    //    #region << Update CustomSortOrder >>
    //    /// <summary>
    //    /// 还原CustomSortOrder
    //    /// </summary>
    //    /// <param name="termMapping">Id的Mapping对</param>
    //    public void RestoreCustomSortOrder()
    //    {

    //        using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermSet.RestoreCustomSortOrder"))
    //        {

    //            string currentOrder = string.Empty;
    //            if (!string.IsNullOrEmpty(customSortOrder))
    //            {
    //                //分解源端Order
    //                string[] OrderId = customSortOrder.Split(':');

    //                //替换为目的端Order
    //                for (int i = 0; i < OrderId.Length; i++)
    //                {
    //                    //含有Mapping对
    //                    if (termIdMapping.ContainsKey(OrderId[i]))
    //                    {
    //                        //第一次记录
    //                        if (currentOrder.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            currentOrder = termIdMapping[OrderId[i]];
    //                        }
    //                        else
    //                        {
    //                            currentOrder = currentOrder + ":" + termIdMapping[OrderId[i]];
    //                        }
    //                    }
    //                }
    //            }

    //            //还原CustomSortOrder
    //            if (!string.IsNullOrEmpty(currentOrder))
    //            {
    //                termSet.CustomSortOrder = currentOrder;
    //                termSet.TermStore.CommitAll();
    //            }

    //        }

    //    }
    //    #endregion << Update CustomSortOrder >>


    //    #region IAveSPTermSet Members


    //    IAveSPTaxonomyGroup IAveSPTermSet.TaxonomyGroup
    //    {
    //        get
    //        {
    //            return aveTaxonomyGroup;
    //        }
    //        set
    //        {
    //            aveTaxonomyGroup = value as AveTaxonomyGroup;
    //        }
    //    }

    //    #endregion
    //} 
    #endregion
}