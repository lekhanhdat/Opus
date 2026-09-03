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
namespace AvePoint.ObjectModel.ServerSE
{
    using AvePoint.Wrapper.Common;
    using OfficeDevPnP.Core.Pages;
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.SharePoint;

    class ClientSideWebPartListWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "f92bf067-bc19-489e-a556-7fe95f508720";

        protected override List<ClientSideWebpartProperty> PotentialHtmlPropertiesAndTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override List<ClientSideWebpartProperty> PotentialPropertiesAndTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Process(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost)
        {
            var requirePostAction = false;
            var listIdToken = webPart.Properties["selectedListId"];
            var viewIdToken = webPart.Properties["selectedViewId"];
            var listTitleToken = webPart.Properties["listTitle"];

            var builder = new StringBuilder();

            if (listIdToken != null)
            {
                var listId = (Guid)listIdToken;

                if (listId != Guid.Empty)
                {

                    IAveList list = null;

                    var targetListId = listId;
                    try
                    {
                        if (!mapping.GetValueFromListIdMapping(listId, out targetListId))
                        {
                            try
                            {
                                list = document.Web.Lists.GetList(listId, false);
                            }
                            catch (Exception e)
                            {
                                logger.Debug("Can not get list by original id:{0}.Error:{1}", listId, e);
                            }
                            if (list == null && listTitleToken != null)
                            {
                                var listTitle = (string)listTitleToken;
                                list = document.ParentFolder.ParentWeb.Lists.TryGetList(listTitle);
                            }
                        }
                        else
                        {
                            list = document.ParentFolder.ParentWeb.Lists.GetList(targetListId, false);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Can not get list.Error:{0}", e);
                    }

                    if (list != null)
                    {
                        if (list.ID != listId)
                        {
                            webPart.Properties["selectedListId"] = list.ID.ToString("D");
                            webPart.Properties["listTitle"] = list.Title;

                            var serverRelativeUrl = list.RootFolder.ServerRelativeUrl;
                            var webRelativeUrl = serverRelativeUrl;
                            if (!list.ParentWeb.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
                            {
                                webRelativeUrl = serverRelativeUrl.Substring(list.ParentWeb.ServerRelativeUrl.Length);
                            }

                            webPart.Properties["selectedListUrl"] = list.RootFolder.ServerRelativeUrl;
                            webPart.Properties["webRelativeListUrl"] = webRelativeUrl;
                        }

                        if (viewIdToken != null)
                        {
                            var viewId = (Guid)viewIdToken;

                            var targetId = viewId;
                            if (!mapping.GetViewGuidMappingValue(viewId, out targetId))
                            {
                                targetId = viewId;
                            }

                            if (targetId != null)
                            {
                                webPart.Properties["selectedViewId"] = targetId.ToString("D");
                            }
                            else
                            {
                                if (list.GetView(viewId) == null)
                                {
                                    builder.AppendFormat("Cannot get the correct view id:{0} in list:{1} with properties:{2}.\r\n", viewId, list.RootFolder.ServerRelativeUrl, webPart.PropertiesJson);
                                    requirePostAction = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        builder.AppendFormat("Cannot get the list information with {0}\r\n", webPart.PropertiesJson);
                        requirePostAction = true;
                    }
                }
            }

            if (builder.Length > 0)
            {
                logger.Warn("Process {0} with information:{1}", typeof(ClientSideWebPartListWorker).Name, builder.ToString());
            }

            return !requirePostAction;
        }
    }
}
