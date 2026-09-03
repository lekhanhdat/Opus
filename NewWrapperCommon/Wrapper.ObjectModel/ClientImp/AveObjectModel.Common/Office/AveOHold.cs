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
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOHold : IAveOHold
    {
        #region IAveOHold Members
        public AveOHold() { }

        private static AveLogger log = AveLogger.GetInstance(typeof(AveOHold));
        
        public void SetHold(IAveListItem item, IAveListItem hold, string comments)
        {
            //throw new NotImplementedException();
        }

        public void ProvisionWeb(IAveWeb web)
        {
            try
            {
                if (web.Properties.ContainsKey("IsHoldEnabled") &&
                    string.Compare(web.Properties["IsHoldEnabled"], "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                web.Properties.Add("IsHoldEnabled", "true");
                web.Properties.Update();
                web.Update();
            }
            catch (Exception ex) 
            {
                log.Warn("Provision web failed in Hold. WebUrl:{0}.Message:{1}", web.Url, ex.ToString());
            }
        }
        public void ProvisionList(IAveList list)
        {
            try
            {
                IAveField field = list.ParentWeb.AvailableFields.GetById(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E"));
                if (!list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
                {
                    list.Fields.AddFieldAsXml(field.SchemaXml);
                }
                if (!list.ParentWeb.Properties.ContainsKey("IsHoldEnabled"))
                {
                    ProvisionWeb(list.ParentWeb);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Provision list failed in Hold. List Title:{0}.Message:{1}", list.Title, ex.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "holdlistid is a web property")]
        public IAveList GetHoldsList(IAveWeb web)
        {
            try
            {
                if (web.Properties.ContainsKey("holdlistid"))
                {
                    return web.Lists[new Guid(web.Properties["holdlistid"])];
                }
                else if (web.Properties.ContainsKey("HoldListId")) 
                {
                    return web.Lists[new Guid(web.Properties["HoldListId"])];
                }
                return web.Lists["Holds"];
            }
            catch (Exception ex)
            {
                log.Warn("Get hold list failed. WebUrl:{0}.Message:{1}", web.Url, ex.ToString());
                return null;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_dlc_holds_lockhelditemsetting is a web property")]
        public void SetSiteLockProperty(IAveSite site)
        {
            try
            {
                if (site.RootWeb.Properties.ContainsKey("_dlc_holds_lockhelditemsetting") &&
                    string.Compare(site.RootWeb.Properties["_dlc_holds_lockhelditemsetting"], "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return;
                }
                site.RootWeb.Properties["_dlc_holds_lockhelditemsetting"] = "true";
                site.RootWeb.Properties.Update();
                site.RootWeb.Update();
            }
            catch (Exception ex)
            {
                log.Warn("Provision site failed in Hold.SiteUrl:{0}.Message:{1}", site.Url, ex.ToString());
            }
        }

        public List<IAveListItem> GetHolds(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool IsItemOnHold(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool SetHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool RemoveHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public void RemoveHold(IAveListItem item, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public void RegisterCustomHoldProcessor(string strAssembly, string strClass, IAveWebApplication webApp)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterCustomHoldProcessor(IAveWebApplication webApp)
        {
            throw new NotImplementedException();
        }

        public void RemoveHold(int holdID, IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public bool IsHoldEnabled(IAveList list)
        {
            throw new NotImplementedException();
        }

        public IAveListItem GetHold(IAveWeb web, int holdID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
