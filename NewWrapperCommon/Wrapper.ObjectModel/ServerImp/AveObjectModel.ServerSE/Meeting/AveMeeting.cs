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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Meetings;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveMeeting : IAveMeeting
    {
        private SPMeeting meeting;

        public AveMeeting()
        {
        }

        public string LinkWithEvent(IAveWeb eventWeb, string strEventListId, int eventItemId, string strEventWorkspaceLinkField, string strEventWorkspaceLinkUrlField)
        {
            var aveWeb = eventWeb as AveWeb;
            SPWeb parentWeb = null;
            try
            {
                parentWeb = aveWeb.Web.Site.OpenWeb(eventWeb.ParentWebId);
                string url = aveWeb.Web.Url;
                if (strEventListId != null)
                {
                    meeting = SPMeeting.GetMeetingInformation(parentWeb);
                    int num = (int)AveAssemblyUtility.InvokeMethod(meeting, "AddFromEvent", new Type[] { typeof(string), typeof(string), typeof(int) }, url, strEventListId, eventItemId);
                    if (num > 0)
                    {
                        url = url + "?InstanceID=" + num.ToString();
                    }
                    if ((strEventWorkspaceLinkField != null) && (strEventWorkspaceLinkUrlField != null))
                    {
                        string title = aveWeb.Web.Title;
                        this.UpdateEventCPL(parentWeb, url, title, strEventListId, eventItemId, strEventWorkspaceLinkField, strEventWorkspaceLinkUrlField);
                    }
                }
                return url;

            }
            finally
            {
                if (parentWeb != null)
                {
                    parentWeb.Dispose();
                }
            }
        }

        private void UpdateEventCPL(SPWeb web, string mwsUrl, string urlDescription, string listId, int itemId, string fieldSource, string cpLinkSource)
        {
            Guid guid = new Guid(listId);
            SPList list = web.Lists[guid];
            SPQuery query = new SPQuery
            {
                IncludeMandatoryColumns = true,
                ViewXml = "<View ModerationType=\"Moderator\"><Query><Where><Eq><FieldRef Name=\"ID\"></FieldRef><Value Type=\"Integer\"> |0</Value></Eq></Where></Query><ViewFields><FieldRef Name=\"ID\" /><FieldRef Name=\"GUID\" /><FieldRef Name=\"WorkspaceLink\" /><FieldRef Name=\"Workspace\" /></ViewFields></View>".Replace("|0", itemId.ToString())
            };
            SPListItem item = list.GetItems(query)[0];
            string str = mwsUrl.Replace(",", ",,") + ", " + urlDescription;

            //item.SetValue(fieldSource, true, false);
            //item.SetValue(cpLinkSource, str, false);
            AveAssemblyUtility.InvokeMethod(item, "SetValue", new Type[] { typeof(string), typeof(object), typeof(bool) }, new object[] { fieldSource, true, false });
            AveAssemblyUtility.InvokeMethod(item, "SetValue", new Type[] { typeof(string), typeof(object), typeof(bool) }, new object[] { cpLinkSource, str, false });
            item.SystemUpdate(false);

        }
    }
}
