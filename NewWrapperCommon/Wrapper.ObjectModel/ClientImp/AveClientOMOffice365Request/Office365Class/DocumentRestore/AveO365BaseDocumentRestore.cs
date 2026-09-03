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
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public abstract class AveO365BaseDocumentRestore : Ave2019BaseDocumentRestore
    {
        protected ITokenProvider tokenProvider;
        protected ITokenProvider IDCLRtokenProvider;
        protected AveO365BaseDocumentRestore(AveClientContext context, AveClientOM2019Request request, FederationToken tokenProvider, AveDocumentInfo docInfo, Stream fileStream)
            :base(context, request, null, docInfo, fileStream)
        {
            this.tokenProvider = tokenProvider.MainTokenProvider;
            this.IDCLRtokenProvider = tokenProvider.GetProviderByType(TokenType.IDCLR);
        }


        protected override void SetModernWebPrtFields(ClientFile file, AveDocumentInfo info, Dictionary<string, object> fields)
        {
            if (file != null)
            {
                var dataProcessor = new SharePointDocumentDataProcessor(this.Context, this.AveWeb, info.ListId, file.UniqueId, info.MappingManager.SiteMappingManager, info.SourceSiteInfo,info.GetUserFromMapping);
                if (!dataProcessor.ProcessUserData(fields))
                {
                    dataProcessor.RecordPostActions();
                }
            }
        }

        protected override Dictionary<string, string> RestoreWebParts(ClientFile webPartPage)
        {
            if (LimitedWebPartManager == null)
            {
                return null;
            }
            ListItem webPartPageitem = IsSystemFile ? null : webPartPage.ListItemAllFields;
            using (var webpartRestore = new AveOffice365WebpartRestore(Context, AveWeb, ParentWeb,
                                                                                    ParentList, webPartPage, LimitedWebPartManager,
                                                                                    webPartPageitem, DocInfo.WebPartCache, mReport, Authentication,tokenProvider))
            {
                webpartRestore.RestoreWebPartsOnly(webpartRestore.GetNeedRestoreWebParts(DocInfo.WebParts, true));
                return webpartRestore.WebPartIdMapping;
            }
        }

        protected override void UpdateByWebService(ClientFile file, string webAppName, Dictionary<string, object> needKeepData)
        {
            if (this.IDCLRtokenProvider != null)
            {
                AvePoint.ObjectModel.WebService.AveWebServiceRequest.UpdateListItems(webAppName, AveWeb.ServerRelativeUrl, ParentList.Title, file.ListItemAllFields.Id, file.ListItemAllFields.FieldValues["FileRef"].ToString(), Authentication, needKeepData, this.IDCLRtokenProvider);
            }
        }

    }
}
