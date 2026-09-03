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
using Microsoft.SharePoint.Client.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365DocumentRestore : Ave2019DocumentRestore, IDisposable
    {
        private FederationToken tokenProviders; 
        public AveO365DocumentRestore(AveClientOMOffice365Request request, Site site, FederationToken tokenProviders, AveClientContext conText, string serverVersion, IReport report)
            : base(request, site, null, conText, serverVersion, report)
        {
            this.tokenProviders = tokenProviders;
        }

        protected override void InitItemRestoreContext()
        {
            mItemRestore = mParentList != null && mRowId > 0 ? new AveO365ListItemRestore(this.mRequest as AveClientOMOffice365Request, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, this.tokenProviders.GetProviderByType(TokenType.IDCLR)) : null;
        }

        public override BaseDocumentRestore CreateDocumentObject(AveDocumentInfo info, Stream fileStream)
        {
            BaseDocumentRestore itemRestore;
            if (info.IsView)
            {
                itemRestore = new AveO365ViewRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else if (info.OriginalRowId <= 0)
            {
                itemRestore = new AveO365SystemFileRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else if (info.ParentLibraryIsMasterPageGallery)
            {
                itemRestore = new AveO365MasterPageDocumentRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else if (IsPageLibrary(info))
            {
                itemRestore = new AveO365PageFileRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else if (info.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                itemRestore = new AveO365XmlFileRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else if ((info.AveItem.Folder.ParentList != null && info.AveItem.Folder.ParentList.IsOneDriveLibrary)
                || WrapperConfiguration.KeepVersionSettingDuringRestore)
            {
                //也可以用此方法还原普通的Document，不开关Version
                itemRestore = new AveO365OneDriveDocumentRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            else
            {
                itemRestore = new AveO365OrdinaryFileRestore(mContext as AveClientContext, mRequest as AveClientOMOffice365Request, this.tokenProviders, info, fileStream);
            }
            itemRestore.SetReport(mReport);
            return itemRestore;
        }
    }
}
