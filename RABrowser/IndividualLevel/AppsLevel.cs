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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class AppsLevel : IndividualBase
    {
        public AppsLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }
        public List<SPTreeNodeDto> GetAppDefinitions(IAveWeb web)
        {
            List<SPTreeNodeDto> appsDefinitions = new List<SPTreeNodeDto>();
            try
            {
                IList<IAveAppInstance> appInstances = ObjectModel.CreateAppCatalog().GetAppInstances(web);
                foreach (IAveAppInstance appInstance in appInstances)
                {
                    SPTreeNodeDto item = new SPTreeNodeDto();
                    item.Name = appInstance.Title;
                    item.DisplayName = appInstance.Title;
                    item.Level = NodeLevel.App;
                    item.SPObjectId = appInstance.App.ProductId.ToString();
                    item.Url = appInstance.AppWebFullUrl != null ? appInstance.AppWebFullUrl.ToString() : string.Empty;
                    item.FarmID = FarmId;
                    item.NodeExtension = FillNodeExtension(item.NodeExtension, appInstance);
                    appsDefinitions.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", web.Url, ex.ToString());
            }
            return appsDefinitions;
        }

        public List<SPTreeNodeDto> GetAppInstances(IAveWeb parentWeb, SPTreeNodeDto appDefinitionNode)
        {
            List<SPTreeNodeDto> appsInstance = new List<SPTreeNodeDto>();
            SPTreeNodeDto item = new SPTreeNodeDto();
            try
            {
                IList<IAveAppInstance> appInstances = parentWeb.GetAppInstancesByProductId(new Guid(appDefinitionNode.SPObjectId));
                IAveAppInstance instance = appInstances[0];
                //非SharePoint Hosted App没有App Web ?
                if (instance.AppWebFullUrl != null)
                {
                    string absolutePath = instance.AppWebFullUrl.AbsolutePath;
                    item.Name = absolutePath.Substring(absolutePath.LastIndexOf('/') + 1);
                    item.DisplayName = "App Data";
                    item.Level = NodeLevel.AppData;
                    item.SPObjectId = instance.Id.ToString();
                    item.ParentId = appDefinitionNode.SPObjectId;
                    item.FarmID = FarmId;
                    item.CanChildrenBeLoaded = false;
                    appsInstance.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", parentWeb.Url, ex.ToString());
            }
            return appsInstance;
        }

        //to do : get app definitions by page

        public List<SPTreeNodeDto> GetBrowserAppDefinitions(Guid parentWebId)
        {
            List<SPTreeNodeDto> appsDefinitions = new List<SPTreeNodeDto>();
            try
            {
                IList<AveAppBrowserInfo> appInstances = Query.GetBrowserApps(parentWebId);
                foreach (AveAppBrowserInfo appInstance in appInstances)
                {
                    SPTreeNodeDto item = new SPTreeNodeDto();
                    item.Name = appInstance.Name;
                    item.DisplayName = appInstance.DisplayName;
                    item.Level = NodeLevel.App;
                    item.SPObjectId = appInstance.SPObjectId.ToString();
                    item.Url = appInstance.Url != null ? appInstance.Url.ToString() : string.Empty;
                    item.FarmID = FarmId;
                    item.NodeExtension = FillNodeExtension(item.NodeExtension, appInstance);
                    appsDefinitions.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", parentWebId, ex.ToString());
            }
            return appsDefinitions;
        }

        public List<SPTreeNodeDto> GetBrowserAppInstances(Guid parentWebId, SPTreeNodeDto appDefinitionNode)
        {
            List<SPTreeNodeDto> appsInstance = new List<SPTreeNodeDto>();
            SPTreeNodeDto item = new SPTreeNodeDto();
            try
            {
                List<AveAppBrowserInfo> appInstances = Query.GetBrowserAppsByProductId(parentWebId, new Guid(appDefinitionNode.SPObjectId));
                AveAppBrowserInfo instance = appInstances[0];
                //非SharePoint Hosted App没有App Web ?
                if (instance.Url != null)
                {
                    string absolutePath = instance.Url.AbsolutePath;
                    item.Name = absolutePath.Substring(absolutePath.LastIndexOf('/') + 1);
                    item.DisplayName = "App Data";
                    item.Level = NodeLevel.AppData;
                    item.SPObjectId = instance.SPObjectId.ToString();
                    item.ParentId = appDefinitionNode.SPObjectId;
                    item.FarmID = FarmId;
                    item.CanChildrenBeLoaded = false;
                    appsInstance.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", parentWebId, ex.ToString());
            }
            return appsInstance;
        }
    }
}
