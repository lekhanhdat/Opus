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
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AvePoint.ObjectModel.ClientOM
{
    public class AveOffice365WebpartRestore : Ave2013WebPartRestore
    {
        protected ITokenProvider tokenProvider;

        public AveOffice365WebpartRestore(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, bool clearAll, ClientContext context, AveWebPartCache mapping, IAveWeb web, IReport report, object obj, ITokenProvider tokenProvider)
            : base(webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, clearAll, context, mapping, web,report, obj)
        {
            this.tokenProvider = tokenProvider;
        }

        public AveOffice365WebpartRestore(ClientContext context, IAveWeb cachedWeb, Web web, List list, File page, LimitedWebPartManager limitedWebPartManager, ListItem item, AveWebPartCache mapping, IReport report, object obj, ITokenProvider tokenProvider)
            : base(context, cachedWeb, web, list, page, limitedWebPartManager, item, mapping, report, obj)
        {
            this.tokenProvider = tokenProvider;
        }

        protected override Guid GetWebPartIdByWebservice(AveWebPartBaseInfo webpartInfo, string webUrl, string pageUrl)
        {
            return AveWebServiceRequest.AddWebPartWithWebService(webUrl, pageUrl, mObj, tokenProvider, webpartInfo);
        }

        protected override void PostUpdateWebPart(AveWebPartBaseInfo webpartInfo, string newWebPartId)
        {
            var updater = AveOffice365WebPartPostUpdater.CreateInstance(new Guid(newWebPartId), webpartInfo, mCachedWeb, mFileServerRelativeUrl, tokenProvider);
            updater.PostUpdate();
        }
    }
}