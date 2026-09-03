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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser.Query;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class IndividualBase : IDisposable
    {

        protected static AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory mObjectModel = null;
        private string mFarmId = string.Empty;
        protected string siteUrl = string.Empty;
        public AveObjectModelFactory ObjectModel
        {
            get
            {
                return mObjectModel;
            }
        }

        public IndividualBase(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl, bool forceNativeModel = false)
        {
            mObjectModel = objectModel;
            this.siteUrl = siteUrl;
            if (forceNativeModel)
            {
                Query = new BrowserNativeQuery(objectModel, sqlConnString, siteUrl);
                Logger.Debug("The browser is native model.");
            }
            else
            {
                Query = new BrowserAPIQuery(objectModel);
                Logger.Debug("The browser is not native model.");
            }
            mFarmId = AveEnv.AgentFarmId;
        }
        public string FarmId
        {
            get
            {
                return mFarmId;
            }
        }

        public IBrowserQuery Query { get; set; }

        /// <summary>
        /// 为DM Extension结点赋值
        /// </summary>
        public virtual NodeExtensionDto FillNodeExtension(NodeExtensionDto extensionNode, object nodeDto)
        {
            return extensionNode;
        }

        public IAveSite GetSite(string url)
        {
            return ObjectModel.CreateSite(url);
        }

        public IAveSite GetSiteById(Guid id)
        {
            return ObjectModel.CreateSite(id);
        }

        public void Dispose()
        {
            Query.Dispose();
        }

        public bool IsAuthenticatedUsers(string userName)
        {
            return userName != null && userName.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsBuiltInAccount(string userName)
        {
            return userName != null && userName.StartsWith("NT AUTHORITY\\", StringComparison.OrdinalIgnoreCase);
        }

        #region << For Workflow >>
        internal bool GetInfoFromInternalName(string internalName, out Guid noCodeWorkflowLibId, out int cfgFileItemId, out int cfgFileVersion)
        {
            noCodeWorkflowLibId = Guid.Empty;
            cfgFileItemId = -1;
            cfgFileVersion = -1;
            try
            {
                int startIndex = internalName.LastIndexOf("<cfg.", StringComparison.OrdinalIgnoreCase);
                if (startIndex > 0)
                {
                    internalName = internalName.Substring(startIndex);
                    if (internalName.ToLower(CultureInfo.CurrentCulture).StartsWith("<cfg.", StringComparison.OrdinalIgnoreCase)
                        && internalName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                    {
                        internalName = internalName.Substring(1, internalName.Length - 2);
                        string[] splitedCfgName = internalName.Split('.');
                        noCodeWorkflowLibId = new Guid(splitedCfgName[1].Replace('_', '-'));
                        cfgFileItemId = int.Parse(splitedCfgName[2]);
                        cfgFileVersion = int.Parse(splitedCfgName[3]);
                        return true;
                    }
                    else
                        Logger.Warn("Invalid workflow definition internal name v1.Name:{0}", internalName);
                    return false;
                }
                else
                {
                    Logger.Warn("Invalid workflow definition internal name v2.Name:{0}", internalName);
                    return false;
                }


            }
            catch (Exception e)
            {
                Logger.Warn("An error occurred while GetInfoFromInternalName.Name:{0},Error:{1}", internalName, e);
                return false;
            }
        }

        /// <summary>
        /// Compare InternalName大小
        /// </summary>
        /// <param name="xInternalName"></param>
        /// <param name="yInternalName"></param>
        /// <returns>
        /// 0: 等于 或者 一个为空
        /// 1: xInternalName 大于 yInternalName
        /// -1:xInternalName 小于 yInternalName
        /// </returns>
        public int Compare(string xInternalName, string yInternalName)
        {
            //internal name format   <Cfg.360f6279_595b_486f_a971_48a6f3189720.4.1024.>
            if (string.IsNullOrEmpty(xInternalName) || string.IsNullOrEmpty(yInternalName))
            {
                return 0;
            }
            Guid xLibId = Guid.Empty;
            int xItemId = -1;
            int xVersionId = -1;
            GetInfoFromInternalName(xInternalName, out xLibId, out xItemId, out xVersionId);
            Guid yLibId = Guid.Empty;
            int yItemId = -1;
            int yVersionId = -1;
            GetInfoFromInternalName(yInternalName, out yLibId, out yItemId, out yVersionId);
            if (xLibId != yLibId)
            {
                return xLibId.CompareTo(yLibId);
            }
            else
            {
                if (xItemId > yItemId)
                {
                    return 1;
                }
                else if (xItemId < yItemId)
                {
                    return -1;
                }
                else
                {
                    if (xVersionId > yVersionId)
                    {
                        return 1;
                    }
                    else if (xVersionId < yVersionId)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        #endregion << For Workflow >>
    }
    internal class SPTreeNodeDtoComparer : IComparer<SPTreeNodeDto>
    {
        public int Compare(SPTreeNodeDto x, SPTreeNodeDto y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
        }
    }
}
