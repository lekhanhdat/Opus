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
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Restore.NintexForm.Server
{
    class WorkflowDiagramControl : BaseControl
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public WorkflowDiagramControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
        }
        public override void ProcessControl(bool isPost)
        {
            if(!NeedContinue())
            {
                return;
            }
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            IAveList destinationList;
            #region Handle List
            var listNode = GetPropertyNode(GetXPath("List"));
            var listTitle = listNode == null ? string.Empty : listNode.InnerText;
            if (string.IsNullOrEmpty(listTitle))
            {
                return;
            }
            string destinationListTitle;
            if (!siteMappingManager.GetValueFromListTitleMappnig(mWeb.SPWeb.ID, listTitle, out destinationListTitle))
            {
                if (!isPost)
                {
                    //可能List还没有被还原。
                    throw new AveNintexFormListNotFoundException(listTitle, contentTypeId);
                }
                destinationListTitle = listTitle;
            }
            listNode.InnerText = destinationListTitle;
            #endregion

            #region Handle Item
            var itemNode = GetPropertyNode(GetXPath("ItemId"));
            var listItemIdString = itemNode == null ? string.Empty : itemNode.InnerText;
            var listItemId = 0;
            int.TryParse(listItemIdString, out listItemId);
            if (listItemId <= 0)
            {
                return;
            }
            destinationList = mWeb.SPWeb.GetListByName(destinationListTitle, false);
            if (destinationList == null)
            {
                throw new AveNintexFormListNotFoundException(listTitle, contentTypeId);
            }
            listNode.InnerText = destinationList.Title;
            var destinationListItemId = siteMappingManager.GetMappingItemId(destinationList.ID, listItemId);
            if (destinationListItemId == -1)
            {
                if(!isPost)
                {
                    throw new AveNintexFormListItemNotFoundException(destinationListTitle, listItemId, contentTypeId);
                }
                log.Warn("This item did restored in this job. Use source Item id to publish nintex form. List: {0}, Item: {1}", listTitle, listItemId);
            }
            else
            {
                itemNode.InnerText = destinationListItemId.ToString();
            }
            #endregion
        }
        bool NeedContinue()
        {
            var loadFromContext = false;
            Boolean.TryParse(GetProperty(GetXPath("LoadFromContext")), out loadFromContext);
            if (loadFromContext)//不需要替换。
            {
                return false;
            }
            return true;
        }
    }
}
