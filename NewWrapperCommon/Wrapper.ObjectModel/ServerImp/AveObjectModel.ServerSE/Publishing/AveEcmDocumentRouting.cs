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
using Microsoft.Office.RecordsManagement.RecordsRepository;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveEcmDocumentRouting : IEcmDocumentRouting
    {

        internal bool RouteFileToFinalDestination(SPWeb web, SPList dropOffLibrary, SPUser routUser, SPFile file, out string routDestination)
        {
            //Route File的逻辑实现可以使用Reflector查看SharePoint API RouteFileButton的SaveItem方法。
            routDestination = "";
            EcmDocumentRoutingWeb routingWeb = new EcmDocumentRoutingWeb(web);
            if (dropOffLibrary == null)
            {
                dropOffLibrary = routingWeb.DropOffZone;
            }

            if (routingWeb.RoutingRuleCollection.Count > 0 && file.CheckOutType != SPFile.SPCheckOutType.None)
            {
                file.CheckIn("");
            }
            if (dropOffLibrary.EnableModeration && file.Item.ModerationInformation != null && file.Item.ModerationInformation.Status != SPModerationStatusType.Approved)
            {
                file.Approve("");
            }
            SPListItem itemToRoute = dropOffLibrary.GetItemByUniqueId(file.UniqueId);

            object[] parameters = new object[] { itemToRoute, web, web.CurrentUser, "", "", null };
            bool succ = (bool)AveAssemblyUtility.InvokeMethod(routingWeb.Router, typeof(EcmDocumentRouter), "RouteFileToFinalLocationNowAsSystem", parameters);

            if (parameters[4] != null)
            {
                routDestination = parameters[4].ToString();
            }
            return succ;
        }

        /// <summary>
        /// According to Routing Rule to update edit template name of content type to 'DropOffZoneRoutingForm'
        /// </summary>
        /// <param name="web"></param>
        internal void UpdateDropOffLibContentType(SPWeb web)
        {
            EcmDocumentRoutingWeb routingWeb = new EcmDocumentRoutingWeb(web);
            if (routingWeb.IsRoutingEnabled)
            {
                SPList dropOffLib = null;
                foreach (EcmDocumentRouterRule rule in routingWeb.RoutingRuleCollection)
                {
                    if (dropOffLib == null)
                    {
                        dropOffLib = routingWeb.DropOffZone;
                    }
                    string ctString = rule.ContentTypeString;
                    SPContentType ct = null;
                    if (!string.IsNullOrEmpty(ctString))
                    {
                        string[] strArray = ctString.Split(new char[] { '|' });
                        if (strArray.Length == 2)
                        {
                            ct = dropOffLib.ContentTypes[strArray[1]];
                            if (ct == null)
                            {
                                SPContentTypeId ctId = new SPContentTypeId(strArray[0]);
                                foreach (SPContentType contentType in dropOffLib.ContentTypes)
                                {
                                    if (contentType.Id.Parent.Equals(ctId))
                                    {
                                        ct = contentType;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (strArray.Length == 1)
                        {
                            ct = dropOffLib.ContentTypes[strArray[0]];
                        }
                    }
                    if (ct != null && ct.Id.IsChildOf(SPBuiltInContentTypeId.Document) && !ct.EditFormTemplateName.Equals("DropOffZoneRoutingForm", StringComparison.OrdinalIgnoreCase))
                    {
                        ct.Sealed = false;
                        ct.ReadOnly = false;
                        ct.EditFormTemplateName = "DropOffZoneRoutingForm";
                        ct.Update();
                    }
                }
            }
        }

        public bool RouteFileToFinalDestination(IAveWeb web, IAveList dropOffLibrary, IAveUser routUser, IAveFile file, out string routDestination)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveEcmDocumentRouting.RouteFileToFinalDestination"))
            {

                return RouteFileToFinalDestination((web as AveWeb).Web,
                    dropOffLibrary == null ? null : (dropOffLibrary as AveList).List,
                    (routUser as AveUser).User,
                    (file as AveFile).File, out routDestination);

            }

        }

        public void UpdateDropOffLibContentType(IAveWeb web)
        {
            UpdateDropOffLibContentType((web as AveWeb).Web);
        }
    }
}
