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
using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveClientOM2016Request : AveClientOM2013Request, IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOM2016Request));

        public AveClientOM2016Request(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
            : base(url, userAccountInfo, obj, serverVersion)
        {
            Type = AveClientRequestType.AveClientOM2016Request;
        }

        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            string oldWebUrl = string.Empty;
            if (!string.IsNullOrEmpty(info.ParentWebRelativeUrl) && !string.IsNullOrEmpty(this.mWebUrl) && this.mWebUrl.Contains("/sites"))
            {
                oldWebUrl = this.mWebUrl;
                this.mWebUrl = string.Format("{0}{1}", this.mWebUrl.Substring(0, this.mWebUrl.IndexOf("/sites", StringComparison.OrdinalIgnoreCase)), info.ParentWebRelativeUrl);
            }
            try
            {
                using (AveClientContext context = base.CreateContext())
                {
                    Site site = context.Site;
                    using (var documentRestore = new Ave2016DocumentRestore(this, site, mObj, context, mServerVersion,report))
                    {
                        return documentRestore.RestoreDocument(info, fileStream); ;
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(oldWebUrl))
                {
                    this.mWebUrl = oldWebUrl;
                }
            }
        }

        public override Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2016FolderRestore folderRestore = new Ave2016FolderRestore(this, site, context, mObj))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }

        public override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2016ListItemRestore listItemRestore = new Ave2016ListItemRestore(this, site, context, mObj))
                {
                    return listItemRestore.RestoreListItem(data, userData, AddItemMapping);
                }
            }
        }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}
