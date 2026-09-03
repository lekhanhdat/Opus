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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Extension;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class SharePointOnPremDisposal : IBrowser
    {
        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremDisposal));

        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPremDisposal;

        private Guid relatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        public string Browse(string message)
        {
            try
            {
                Logger.Info($"delete SharePoint On-Premises related operations with message: {message}");
                var args = SerializerHelper.DeserializeByJsonSerializer<SharePointOnPremDisposalArgs>(message);
                var factory = Wrapper.Common.AveObjectModelFactory.CreateObjectModelFactory(null, null, AvePoint.Wrapper.Common.AveContextKind.ServerObjectModel);
                IAveSite site = null;
                if (args.SiteId == Guid.Empty)
                {
                    site = factory.CreateSite(args.SiteUrl);
                }
                else
                {
                    site = factory.CreateSite(args.SiteId);
                }

                using (site)
                {
                    var web = site.AllWebs[args.WebId];
                    using (web)
                    {
                        Logger.Info($"Successfully accessed web: {web.Url}");
                        var list = web.GetList(args.ListId);
                        var listItem = list.GetItemByUniqueId(args.ItemId);

                        if (listItem.IsRecord())
                        {
                            Logger.Info($"current item has declare,need to undeclare first,1id:{listItem.ID}");
                            IAveORecords records = factory.CreateRecords();
                            records.UndeclareItemAsRecord(listItem);;
                        }
                        listItem.Delete();
                        Logger.Info("SPRelated update item successfully. ItemId:{0}", args.ItemId);
                    }
                }
                return SerializerHelper.SerializeByJsonSerializer(new SharePointOnPremDisposalResult { IsSuccess = true,ErrorMessage = "SharePoint On-Premises operations completed." });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while delete SharePoint On-Premises related operations. Error: {e}");
                return SerializerHelper.SerializeByJsonSerializer(new SharePointOnPremDisposalResult { ErrorMessage = e.Message });
            }
        }
    }
}
