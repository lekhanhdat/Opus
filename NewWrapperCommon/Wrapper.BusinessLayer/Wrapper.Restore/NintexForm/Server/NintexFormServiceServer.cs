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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    class NintexFormServiceServer : NintexFormServiceBase
    {
        internal NintexFormServiceServer(IAveList aveList, IAveSPWeb aveSPWeb, bool isPost)
            : base(aveList, aveSPWeb, isPost)
        {
            contentProcessor = new NintexFormContentProcessorServer(aveSPWeb, aveList);
        }

        public override void RestoreForm(AveNintexFormInfo nintexFormInfo, string contentTypeId)
        {
            var finalFormXml = contentProcessor.ReplaceFormContent(nintexFormInfo.FormXml, contentTypeId, isPost);
            PublishNintexForm(finalFormXml, contentTypeId);
            KeepNintexFormFileVersionCreateTime(contentTypeId, nintexFormInfo.VersionCreateTime);
        }

        protected override void PublishNintexForm(string newNintexFormXml, string contentTypeId)
        {
            using (var nfContext = new NfClientContext(mAveSPWeb.SPWeb.Url, null, mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind))
            {
                nfContext.PublishForm(mAveList.ID.ToString("B"), contentTypeId, newNintexFormXml);
            }
        }
        private IAveListItem GetNintexFormFileItem(IAveList nintexFormsList, string contentTypeId, string parentlistId)
        {
            var itemIds = nintexFormsList.GetItemsByColumnValue("FormContentTypeId", contentTypeId);
            if (itemIds.Count == 1)
            {
                return nintexFormsList.GetItemById(itemIds[0]);
            }
            foreach (var itemId in itemIds)
            {
                var item = nintexFormsList.GetItemById(itemId);
                var formListId = item["FormListId"];
                if (formListId != null && string.Equals(formListId.ToString(), parentlistId, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;

        }
        private void KeepNintexFormFileVersionCreateTime(string contentTypeId, DateTime versionCreateTime)
        {
            if (versionCreateTime == default(DateTime))
            {
                return;
            }

            var nintexFormLibrayId = this.mAveSPWeb.SPWeb.Properties.ContainsKey("nintexformslibraryid")
                       && AveTypeHelper.IsGuid(this.mAveSPWeb.SPWeb.Properties["nintexformslibraryid"]) ?
                       new Guid(this.mAveSPWeb.SPWeb.Properties["nintexformslibraryid"]) : Guid.Empty;
            if (nintexFormLibrayId == Guid.Empty)
            {
                return;
            }
            IAveList nintexFormsList = this.mAveSPWeb.SPWeb.Lists[nintexFormLibrayId];
            if (nintexFormsList != null)
            {
                var item = GetNintexFormFileItem(nintexFormsList, contentTypeId, mAveList.ID.ToString("B"));
                if (item != null)
                {
                    nintexFormsList.EnableVersioning = false;
                    nintexFormsList.Update();
                    
                    //Keep Modify time
                    item["Modified"] = this.mAveSPWeb.SPWeb.RegionalSettings.TimeZone.UTCToLocalTime(versionCreateTime);
                    item.UpdateOverwriteVersion();


                    nintexFormsList.EnableVersioning = true;
                    nintexFormsList.EnableMinorVersions = true;
                    nintexFormsList.Update();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listId">need format {list id}</param>
        /// <param name="contentTypeId"></param>
        public override void DeleteForm(string listId, string contentTypeId)
        {
            using (var nfContext = new NfClientContext(mAveSPWeb.SPWeb.Url, null, mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind))
            {
                nfContext.DeleteForm(listId, contentTypeId);
            }
        }
    }
}
