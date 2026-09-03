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
    /// TermStore Wrapper Restore
    /// </summary>
    public class AveTermStore : IMMSRestore, IDisposable
    {
        #region << Property >>
        private static AveLogger sLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory objectModelFactory;
        private IAveTaxonomySession taxonomySession;
        private IAveTermStore termStore;
        private IAveSite aveSite;
        public int DefaultLCID = 1033;
        private string targetTermStoreName = string.Empty;
        private Guid serviceApplicationId;
        private AveTermStoreInfo mTermStoreInfo;
        private AveRestoreOption mRestoreOption;
        private AveTaxonomyUserMappingUtility mUserMappingUtility = null;

        public IAveTermStore TermStore
        {
            get { return this.termStore; }
            set { this.termStore = value; }
        }

        public AveObjectModelFactory ObjectModelFactory
        {
            get { return this.objectModelFactory; }
            set { this.objectModelFactory = value; }
        }

        public AveTaxonomyUserMappingUtility UserMappingUtility
        {
            get { return mUserMappingUtility; }
        }

        public Guid AppilicationId
        {
            get
            {
                if (this.serviceApplicationId == Guid.Empty)
                {
                    foreach (IAveService service in this.objectModelFactory.CreateFarm().Local.Services)
                    {
                        foreach (IAveServiceApplication application in service.Applications)
                        {
                            if (application.IsConnected(this.termStore.SharedServiceProxy))
                            {
                                this.serviceApplicationId = application.ID;
                                break;
                            }
                        }
                    }
                }

                return this.serviceApplicationId;
            }
            set { this.serviceApplicationId = value; }
        }

        #endregion << Property >>

        #region << Constructor >>
        public AveTermStore(AveObjectModelFactory omFactory, string targetTermStoreName, AveTermStoreInfo termStoreInfo, AveRestoreOption restoreOption, IAveSite site = null)
        {
            this.objectModelFactory = omFactory;
            this.mTermStoreInfo = termStoreInfo;
            this.mRestoreOption = restoreOption;
            this.targetTermStoreName = targetTermStoreName;
            if (site == null)
            {
                this.aveSite = this.objectModelFactory.CreateAdministrationWebApplication().Local.Sites[0];
            }
            else
            {
                this.aveSite = site;
            }
            taxonomySession = this.objectModelFactory.CreateTaxonomySession(this.aveSite);
            mUserMappingUtility = new AveTaxonomyUserMappingUtility(this.aveSite, this.objectModelFactory);
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
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermStore.Exists"))
            {
#endif
                try
                {
                    IAveTermStore termStore = null;
                    if (string.IsNullOrEmpty(this.targetTermStoreName))
                    {
                        termStore = this.taxonomySession.TermStores[mTermStoreInfo.Name];
                    }
                    else
                    {
                        termStore = this.taxonomySession.TermStores[this.targetTermStoreName];
                    }
                    this.termStore = termStore;
                    DefaultLCID = termStore.DefaultLanguage;
                    return true;
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(this.targetTermStoreName))
                    {
                        sLogger.Info("Cannot get termStore in Destination. termStoreName:{0}. Reason:{1}.", mTermStoreInfo.Name, ex.ToString());
                    }
                    else
                    {
                        sLogger.Info("Cannot get termStore in Destination. termStoreName:{0}. Reason:{1}.", this.targetTermStoreName, ex.ToString());
                    }
                    return false;
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << Exists >>

        #region << Create >>
        /// <summary>
        /// Store级别暂时不支持Create
        /// </summary>
        public void Create()
        {
        }
        #endregion << Create >>

        #region << Update >>
        /// <summary>
        /// Store级别暂时不支持Update
        /// </summary>
        public void Update()
        {
        }
        #endregion << Update >>

        #region << DeleteChildren >>
        /// <summary>
        /// 清空子结点支持Replace
        /// </summary>
        public void DeleteChildren()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveTermStore.DeleteChildren"))
            {
#endif
                if (this.termStore != null)
                {
                    foreach (IAveTaxonomyGroup group in this.termStore.Groups)
                    {
                        if (!group.IsSystemGroup)
                        {
                            foreach (IAveTermSet termSet in group.TermSets)
                            {
                                termSet.Delete();

                            }
                            group.Delete();
                        }
                        else
                        {
                            //对于KeyWord可以Delete term,但是不能Delete Set和Group
                            IAveTermSet keyword = this.termStore.KeywordsTermSet;
                            foreach (IAveTerm term in keyword.Terms)
                            {
                                term.Delete();
                            }
                        }
                    }
                    this.termStore.CommitAll();
                }
                else
                {
                    if (string.IsNullOrEmpty(this.targetTermStoreName))
                    {
                        throw new AveException("TermStore object is null in Destination. termStoreName:{0}.", mTermStoreInfo.Name);
                    }
                    else
                    {
                        throw new AveException("TermStore object is null in Destination. termStoreName:{0}.", this.targetTermStoreName);
                    }
                }
#if PerformanceLog
            }
#endif
        }
        #endregion << DeleteChildren >>

        #region << Dispose >>
        public void Dispose()
        {
            if (this.aveSite != null)
            {
                this.aveSite.Dispose();
            }
        }
        #endregion << Dispose >>

    }

    #region << Metadata Service Restore Interface >>
    /// <summary>
    /// MMS Restore的接口
    /// </summary>
    public interface IMMSRestore
    {
        /// <summary>
        /// 判定是否存在
        /// </summary>
        /// <returns>True:存在 False:不存在</returns>
        bool Exists();

        /// <summary>
        /// 对于不存在的新建
        /// </summary>
        void Create();

        /// <summary>
        /// 更新属性
        /// </summary>
        void Update();

        /// <summary>
        /// 清空子结点
        /// </summary>
        void DeleteChildren();
    }
    #endregion << Metadata Service Restore Interface >>
}