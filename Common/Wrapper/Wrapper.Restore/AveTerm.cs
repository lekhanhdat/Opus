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
    /// Term Wrapper Restore
    /// </summary>
    public class AveTerm : IMMSRestore
    {
        #region << Property >>
        private static AveLogger sLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public AveObjectModelFactory ObjectModelFactory;
        private IAveTermSet termSet;
        private IAveTerm term;
        private AveTermSet aveTermSet;
        public bool IsNewCreated = false;
        private string targetTermName = string.Empty;
        public Guid parentTermId = Guid.Empty;
        private IAveTerm parentTerm;
        private string customSortOrder = string.Empty;
        private AveTermInfo mTermInfo;
        private AveRestoreOption mRestoreOption;
        private AveTaxonomyUserMapping mUserMapping;
        private AveTaxonomyUserMappingUtility mUserMappingUtility;

        public AveTermSet TermSet
        {
            get { return this.aveTermSet; }
            set { this.aveTermSet = value; }
        }

        public IAveTerm Term
        {
            get { return this.term; }
            set { this.term = value; }
        }
        #endregion << Property >>

        #region << Constructor >>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aveTermSet"></param>
        /// <param name="targetTermName">支持out of place</param>
        /// <param name="parentTermId">如果还原到Term下，则需要外围传入parentTermId;如果还原到TermSet下，则传入空的Guid</param>
        /// <param name="userMapping">需要外围传入相应的UserMapping</param>
        public AveTerm(AveTermSet aveTermSet, string targetTermName, Guid parentTermId, AveTermInfo termInfo, AveRestoreOption restoreOption, AveTaxonomyUserMapping userMapping)
        {
            this.aveTermSet = aveTermSet;
            this.termSet = this.aveTermSet.TermSet;
            this.ObjectModelFactory = aveTermSet.ObjectModelFactory;
            this.targetTermName = targetTermName;
            this.parentTermId = parentTermId;
            this.mTermInfo = termInfo;
            this.mRestoreOption = restoreOption;
            this.mUserMapping = userMapping;
            this.mUserMappingUtility = aveTermSet.UserMappingUtility;
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
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.Exists"))
            {
#endif

                IAveTerm tmpTerm = null;
                string tmpName = string.Empty;

                try
                {
                    if (this.parentTermId == Guid.Empty)
                    {
                        if (string.IsNullOrEmpty(this.targetTermName))
                        {
                            tmpName = mTermInfo.Name;
                            tmpTerm = this.termSet.Terms[mTermInfo.Name];
                        }
                        else
                        {
                            tmpName = targetTermName;
                            tmpTerm = this.termSet.Terms[this.targetTermName];
                        }
                    }
                    else
                    {
                        this.parentTerm = this.termSet.GetTerm(this.parentTermId);
                        if (string.IsNullOrEmpty(this.targetTermName))
                        {
                            tmpName = mTermInfo.Name;
                            tmpTerm = this.parentTerm.Terms[mTermInfo.Name];
                        }
                        else
                        {
                            tmpName = targetTermName;
                            tmpTerm = this.parentTerm.Terms[this.targetTermName];
                        }
                    }

                }
                catch (Exception ex)
                {
                    sLogger.Info("Cannot get term in Destination. termName:{0}. Reason:{1}.", tmpName, ex.ToString());
                }
                if (tmpTerm == null)
                {
                    return false;
                }
                else
                {
                    this.term = tmpTerm;

                    //记录TermIdMapping
                    if (!this.aveTermSet.termIdMapping.ContainsKey(mTermInfo.Id.ToString()))
                    {
                        this.aveTermSet.termIdMapping.Add(mTermInfo.Id.ToString(), term.ID.ToString());
                    }
                    return true;
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << Exists >>

        #region << Create >>
        /// <summary>
        /// 创建Term 对象
        /// </summary>
        public void Create()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.Create"))
            {
#endif
                try
                {
                    IAveTerm tmpTerm = null;
                    if (this.parentTerm == null)
                    {
                        if (string.IsNullOrEmpty(this.targetTermName))
                        {
                            tmpTerm = this.termSet.CreateTerm(mTermInfo.Name, this.aveTermSet.TaxonomyGroup.TermStore.DefaultLCID);
                        }
                        else
                        {
                            tmpTerm = this.termSet.CreateTerm(this.targetTermName, this.aveTermSet.TaxonomyGroup.TermStore.DefaultLCID);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.targetTermName))
                        {
                            tmpTerm = this.parentTerm.CreateTerm(mTermInfo.Name, this.aveTermSet.TaxonomyGroup.TermStore.DefaultLCID);
                        }
                        else
                        {
                            tmpTerm = this.parentTerm.CreateTerm(this.targetTermName, this.aveTermSet.TaxonomyGroup.TermStore.DefaultLCID);
                        }
                    }
                    this.term = tmpTerm;
                    this.IsNewCreated = true;
                    this.termSet.TermStore.CommitAll();

                    //记录TermIdMapping
                    if (!this.aveTermSet.termIdMapping.ContainsKey(mTermInfo.Id.ToString()))
                    {
                        this.aveTermSet.termIdMapping.Add(mTermInfo.Id.ToString(), term.ID.ToString());
                    }

                    //更新属性
                    Update();
                }
                catch (Exception e)
                {
                    if (string.IsNullOrEmpty(this.targetTermName))
                    {
                        sLogger.Warn("An error occurred when creating term. term Name:{0}. \r\n error:{1}", mTermInfo.Name, e.ToString());
                        throw new AveException("An error occurred when creating term. term Name:{0}. \r\n error:{1}", mTermInfo.Name, e.Message.ToString());
                    }
                    else
                    {
                        sLogger.Warn("An error occurred when creating term. term Name:{0}. \r\n error:{1}", this.targetTermName, e.ToString());
                        throw new AveException("An error occurred when creating term. term Name:{0}. \r\n error:{1}", this.targetTermName, e.Message.ToString());
                    }
                }

#if PerformanceLog
            }
#endif
        }
        #endregion << Create >>

        #region << Update Property >>
        /// <summary>
        /// 还原Property
        /// </summary>
        private void UpdateTermProperty()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.UpdateTermProperty"))
            {
#endif
                try
                {
                    term.IsAvailableForTagging = mTermInfo.IsAvailableForTagging;

                    if (!string.IsNullOrEmpty(mTermInfo.Description))
                    {
                        term.SetDescription(mTermInfo.Description, this.aveTermSet.TaxonomyGroup.TermStore.DefaultLCID);
                    }

                    foreach (AveLableInfo labelInfo in mTermInfo.Labels)
                    {
                        bool findLabel = false;
                        foreach (IAveLabel destLabel in term.Labels)
                        {
                            if (destLabel.Value.Equals(labelInfo.Value, StringComparison.OrdinalIgnoreCase) && destLabel.Language == labelInfo.Language)
                            {
                                findLabel = true;
                                break;
                            }
                        }
                        if (!term.TermStore.Languages.Contains(labelInfo.Language))
                        {
                            continue;
                        }
                        //ADO-7216
                        if (!string.IsNullOrEmpty(labelInfo.Description))
                        {
                            term.SetDescription(labelInfo.Description, labelInfo.Language);
                        }
                        if (!findLabel)
                        {
                            term.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                        }
                    }
                    //ADO-7324
                    if (!string.IsNullOrEmpty(mTermInfo.CustomSortOrder))
                    {
                        customSortOrder = mTermInfo.CustomSortOrder;
                    }
                    //ADO-7411
                    term.Deprecate(mTermInfo.IsDeprecated);
                    term.TermStore.CommitAll();
                }
                catch (Exception ex)
                {
                    sLogger.Warn("An error occurred when updating term property. term Name:{0}. \r\n error:{1}", mTermInfo.Name, ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << Update Property >>

        #region << Update Security >>
        /// <summary>
        /// 还原Security
        /// </summary>
        public void UpdateTermSecurity()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.UpdateTermSecurity"))
            {
#endif
                //UserMapping
                bool isSharepointGroup = false;
                string userName = string.Empty;

                try
                {
                    userName = mUserMappingUtility.GetMappingUserLogin(mTermInfo.Owner, mUserMapping.UserMappings, mUserMapping.DomainMappings, out isSharepointGroup);
                    if (!string.IsNullOrEmpty(userName)) term.Owner = userName;
                }
                catch (Exception e)
                {
                    try
                    {
                        sLogger.Warn("An error occurred when setting term owner. term:{0}, owner:{1}. \r\n error:{2}", term.Name, userName, e.ToString());
                        if (!isSharepointGroup && !string.IsNullOrEmpty(mUserMapping.TargetDefaultUser))
                        {
                            term.Owner = mUserMapping.TargetDefaultUser;
                        }
                    }
                    catch (Exception ex)
                    {
                        sLogger.Warn("An error occurred when setting term owner. term:{0}, owner:{1}. \r\n error:{2}", term.Name, mUserMapping.TargetDefaultUser, ex.ToString());
                    }

                }
                term.TermStore.CommitAll();

#if PerformanceLog
            }
#endif
        }
        #endregion << Update Security >>

        #region << Update >>
        /// <summary>
        /// 更新Term 对象
        /// </summary>
        public void Update()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.Update"))
            {
#endif
                if (this.term != null)
                {
                    //Restore Property
                    if (mRestoreOption.TermRestoreOption.RESTOREPROPERTIES)
                    {
                        UpdateTermProperty();
                    }
                    //Restore Security
                    if (mRestoreOption.TermRestoreOption.RESTORESECURITY)
                    {
                        UpdateTermSecurity();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.targetTermName))
                    {
                        throw new AveException("Term object is null in Destination. term name:{0}.", mTermInfo.Name);
                    }
                    else
                    {
                        throw new AveException("Term object is null in Destination. term name:{0}.", this.targetTermName);
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
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.DeleteChildren"))
            {
#endif
                if (this.term != null)
                {
                    foreach (IAveTerm tmpTerm in this.term.Terms)
                    {
                        tmpTerm.Delete();
                    }
                    term.TermStore.CommitAll();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.targetTermName))
                    {
                        throw new AveException("Term object is null in Destination. term name:{0}.", mTermInfo.Name);
                    }
                    else
                    {
                        throw new AveException("Term object is null in Destination. term name:{0}.", this.targetTermName);
                    }
                }

#if PerformanceLog
            }
#endif
        }
        #endregion << DeleteChildren >>

        #region << Update CustomSortOrder >>
        /// <summary>
        /// 还原Term级别的CustomSortOrder
        /// </summary>
        public void RestoreCustomSortOrder()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTerm.RestoreCustomSortOrder"))
            {
#endif
                string currentOrder = string.Empty;
                if (!string.IsNullOrEmpty(customSortOrder))
                {

                    //分解源端Order
                    string[] OrderId = customSortOrder.Split(':');

                    //替换为目的端Order
                    for (int i = 0; i < OrderId.Length; i++)
                    {
                        //含有Mapping对
                        if (this.aveTermSet.termIdMapping.ContainsKey(OrderId[i]))
                        {
                            //第一次记录
                            if (currentOrder.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
                            {
                                currentOrder = this.aveTermSet.termIdMapping[OrderId[i]];
                            }
                            else
                            {
                                currentOrder = currentOrder + ":" + this.aveTermSet.termIdMapping[OrderId[i]];
                            }
                        }
                    }
                }
                //还原CustomSortOrder
                if (!string.IsNullOrEmpty(currentOrder))
                {
                    term.CustomSortOrder = currentOrder;
                    term.TermStore.CommitAll();
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << Update CustomSortOrder >>

    }
}