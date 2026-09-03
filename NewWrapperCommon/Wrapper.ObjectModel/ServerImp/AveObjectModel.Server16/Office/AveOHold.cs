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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.Office.RecordsManagement.Holds;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOHold : IAveOHold
    {
        public AveOHold()
        { }

        #region IAveOHold Members

        public void SetHold(IAveListItem item, IAveListItem hold, string comments)
        {
            Hold.SetHold((item as AveListItem).ListItem, (hold as AveListItem).ListItem, comments);
        }

        public IAveList GetHoldsList(IAveWeb web)
        {
            SPList list = Hold.GetHoldsList((web as AveWeb).Web);
            if (list != null)
            {
                return (web.Lists as AveListCollection).CreateListByType(list);
            }
            return null;
        }

        public void SetSiteLockProperty(IAveSite site)
        {
            Hold.SetLockHoldItemsProperty((site as AveSite).Site, true);
        }

        public void ProvisionWeb(IAveWeb web)
        {
            Hold.ProvisionWeb((web as AveWeb).Web);
        }

        public void ProvisionList(IAveList list)
        {
            Hold.ProvisionList((list as AveList).List);
        }

        public List<IAveListItem> GetHolds(IAveListItem item)
        {
            List<SPListItem> spholds = Hold.GetHolds((item as AveListItem).ListItem);
            List<IAveListItem> holds = null;
            if (spholds != null)
            {
                holds = new List<IAveListItem>();
                foreach (SPListItem hold in spholds)
                {
                    if (hold != null)
                    {
                        AveSite site = new AveSite(hold.Web.Site);
                        AveWeb web = new AveWeb(site, hold.Web);
                        AveListCollection lists = new AveListCollection(web, hold.Web.Lists);
                        AveList list = lists.CreateListByType(hold.ParentList);
                        holds.Add(new AveListItem(list, hold));
                    }
                    else
                    {
                        holds.Add(null);
                    }
                }
            }
            return holds;
        }

        public bool IsItemOnHold(IAveListItem item)
        {
            return Hold.IsItemOnHold((item as AveListItem).ListItem);
        }

        public bool SetHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            return Hold.SetHold((items as AveListItemCollection).ListItemCollection, (hold as AveListItem).ListItem, comments);
        }

        public bool RemoveHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            return Hold.RemoveHold((items as AveListItemCollection).ListItemCollection, (hold as AveListItem).ListItem, comments);
        }

        public void RemoveHold(IAveListItem item, IAveListItem hold, string comments)
        {
            Hold.RemoveHold((item as AveListItem).ListItem, (hold as AveListItem).ListItem, comments);
        }

        public void RegisterCustomHoldProcessor(string strAssembly, string strClass, IAveWebApplication webApp)
        {
            Hold.RegisterCustomHoldProcessor(strAssembly, strClass, (webApp as AveWebApplication).WebApplication);
        }

        public void UnRegisterCustomHoldProcessor(IAveWebApplication webApp)
        {
            Hold.UnRegisterCustomHoldProcessor((webApp as AveWebApplication).WebApplication);
        }

        public void RemoveHold(int holdID, IAveWeb web)
        {
            Hold.RemoveHold(holdID, (web as AveWeb).Web);
        }

        public bool IsHoldEnabled(IAveList list)
        {
            return Hold.IsHoldEnabled((list as AveList).List);
        }

        public IAveListItem GetHold(IAveWeb web, int holdID)
        {
            SPListItem item = (SPListItem)AveAssemblyUtility.InvokeStaticMethod(typeof(Hold), "GetHold", new Type[] { typeof(SPWeb), typeof(int) }, new object[] { (web as AveWeb).Web, holdID });
            if (item != null)
            {
                return new AveListItem((GetHoldsList(web) as AveList), item);
            }
            return null;
        }

        #endregion
    }
}
