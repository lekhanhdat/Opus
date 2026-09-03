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

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class AppsLevel : IndividualBase
    {
        public AppsLevel(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
            : base(objectModel, sqlConnString, siteUrl)
        {

        }

        public List<SPTreeNodeDto> GetAppDefinitions(Guid siteID, Guid parentWebId, string siteUrl)
        {
            List<SPTreeNodeDto> appsDefinitions = new List<SPTreeNodeDto>();
            IAveSite site;
            if (ObjectModel != null && string.Equals(ObjectModel.GetType().Name, "AveClientObjectModelFactory"))
            {
                site = this.GetSite(siteUrl);
            }
            else
            {
                site = this.GetSiteById(siteID);
            }
            IAveWeb web = site.OpenWeb(parentWebId);
            try
            {
                IList<IAveAppInstance> appInstances = ObjectModel.CreateAppCatalog().GetAppInstances(web);
                foreach (IAveAppInstance appInstance in appInstances)
                {
                    appsDefinitions.Add(ConvertToDefinitionDto(appInstance));
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", web.Url, ex.ToString());
            }
            finally
            {
                site.Dispose();
                web.Dispose();
            }
            return appsDefinitions;
        }

        private SPTreeNodeDto ConvertToDefinitionDto(IAveAppInstance appInstance)
        {
            SPTreeNodeDto item = new SPTreeNodeDto();
            item.Name = appInstance.Title;
            item.DisplayName = appInstance.Title;
            item.Level = NodeLevel.App;
            item.SPObjectId = appInstance.App.ProductId.ToString();
            item.Url = appInstance.AppWebFullUrl != null ? appInstance.AppWebFullUrl.ToString() : string.Empty;
            item.FarmID = FarmId;
            item.NodeExtension = FillNodeExtension(item.NodeExtension, appInstance);
            return item;
        }

        public List<SPTreeNodeDto> GetAppInstances(SPTreeNodeDto appDefinitionNode, Guid siteId, Guid parentWebId, string siteUrl)
        {
            List<SPTreeNodeDto> appsInstance = new List<SPTreeNodeDto>();
            IAveSite site;

            if (ObjectModel != null && string.Equals(ObjectModel.GetType().Name, "AveClientObjectModelFactory"))
            {
                site = this.GetSite(siteUrl);
            }
            else
            {
                site = this.GetSiteById(siteId);
            }
            IAveWeb web = site.OpenWeb(parentWebId);
            try
            {
                IList<IAveAppInstance> appInstances = web.GetAppInstancesByProductId(new Guid(appDefinitionNode.SPObjectId));
                IAveAppInstance instance = appInstances[0];
                //非SharePoint Hosted App没有App Web ?
                if (instance.AppWebFullUrl != null)
                {
                    appsInstance.Add(ConvertToInstanceDto(appDefinitionNode, instance));
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("can not get apps from web {0}, error message: {1}", web.Url, ex.ToString());
            }
            finally
            {
                site.Dispose();
                web.Dispose();
            }
            return appsInstance;
        }

        private SPTreeNodeDto ConvertToInstanceDto(SPTreeNodeDto appDefinitionNode, IAveAppInstance instance)
        {
            SPTreeNodeDto item = new SPTreeNodeDto();
            string absolutePath = instance.AppWebFullUrl.AbsolutePath;
            item.Name = absolutePath.Substring(absolutePath.LastIndexOf('/') + 1);
            item.DisplayName = "App Data";
            item.Level = NodeLevel.AppData;
            item.SPObjectId = instance.Id.ToString();
            item.ParentId = appDefinitionNode.SPObjectId;
            item.FarmID = FarmId;
            item.CanChildrenBeLoaded = false;
            item.NodeExtension = FillNodeExtension(item.NodeExtension, instance);
            return item;
        }
    }
}
