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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class ItemLevel : IndividualBase
    {
        public ItemLevel(AveObjectModelFactory objectModel, String siteUrl)
            : base(objectModel, string.Empty, siteUrl)
        {

        }

        public List<SPTreeNodeDto> GetItemVersions(IAveListItem parentItem, ref string pageInfo, uint perPage, int siteLockStatus)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            List<SPTreeNodeDto> itemVersions = new List<SPTreeNodeDto>();
            foreach (IAveListItemVersion itemVersion in parentItem.Versions)
            {
                itemVersions.Add(ConvertToDto(itemVersion, parentItem, siteLockStatus));
            }
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower ItemVersions Elasped Time: {0}, ItemVersionCount: {1}, ParentItem: {2}, PageInfo: {3}, PerPage: {4}", sw.Elapsed.ToString(), itemVersions.Count, parentItem.Url, pageInfo, perPage);
#endif
            return itemVersions;
        }

        protected SPTreeNodeDto ConvertToDto(IAveListItemVersion itemVersion, IAveListItem parentItem, int siteLockStatus)
        {
            SPTreeNodeDto itemNode = new SPTreeNodeDto();
            itemNode.Level = NodeLevel.ItemVersion;
            itemNode.FullPath = itemVersion.Url;
            itemNode.Name = parentItem.Name;
            if (string.IsNullOrEmpty(itemNode.Name))
            {
                itemNode.Name = parentItem.ID.ToString();
            }
            itemNode.Name = itemNode.Name + " " + itemVersion.VersionLabel;
            itemNode.DisplayName = parentItem.DisplayName + " " + itemVersion.VersionLabel;
            itemNode.ParentId = parentItem.UniqueId.ToString();
            itemNode.SPObjectId = parentItem.UniqueId.ToString();
            itemNode.FarmID = FarmId;
            itemNode.SiteLockStatus = siteLockStatus;
            itemNode.NodeExtension = FillNodeExtension(itemNode.NodeExtension, itemVersion);
            return itemNode;
        }

    }
}
