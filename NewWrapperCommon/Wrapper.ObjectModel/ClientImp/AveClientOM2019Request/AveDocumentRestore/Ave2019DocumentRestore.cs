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
using AveClientRequest.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using ClientFile = Microsoft.SharePoint.Client.File;
using System.IO;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2019DocumentRestore : Ave2013DocumentRestore, IDisposable
    {
        protected AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 为unittest添加构造函数
        /// </summary>
        public Ave2019DocumentRestore() { }

        public Ave2019DocumentRestore(AveClientOM2019Request request, Site site, object obj, AveClientContext conText, string serverVersion, IReport report)
            : base(request, site, obj, conText, serverVersion,report)
        {
        }

        protected override Microsoft.SharePoint.Client.File GetFileByAPI()
        {
            var path = ResourcePath.FromDecodedUrl(mFileRelativeUrl);
            return mParentWeb.GetFileByServerRelativePath(path);
        }
        protected override ClientFile AddFileByAPI(FileCollection files, FileCreationInformation createInfo)
        {
            FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
            fileAddParameters.Overwrite = createInfo.Overwrite;
            var filePath = ResourcePath.FromDecodedUrl(createInfo.Url);
            return files.AddUsingPath(filePath, fileAddParameters, createInfo.ContentStream);
        }
        protected override Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            FolderCollectionAddParameters folderAddParameters = new FolderCollectionAddParameters();
            folderAddParameters.Overwrite = true;
            return folders.AddUsingPath(path, folderAddParameters);
        }

        protected override Folder GetFolderByAPI(string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return mParentWeb.GetFolderByServerRelativePath(path);
        }

        protected override ClientFile GetFileByAPI(string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return mParentWeb.GetFileByServerRelativePath(path);
        }

        protected override void MoveToByAPI(ClientFile file, string url, MoveOperations option)
        {
            file.MoveToUsingPath(ResourcePath.FromDecodedUrl(url), option);
        }

        protected override ListItem AddItemByAPI(List list, ListItemCreationInformation creationInformation)
        {
            var param = new ListItemCreationInformationUsingPath
            {
                FolderPath = ResourcePath.FromDecodedUrl(creationInformation.FolderUrl),
                LeafName = ResourcePath.FromDecodedUrl(creationInformation.LeafName),
                UnderlyingObjectType = creationInformation.UnderlyingObjectType,
            };
            return list.AddItemUsingPath(param);
        }


        public override BaseDocumentRestore CreateDocumentObject(AveDocumentInfo info, Stream fileStream)
        {
            BaseDocumentRestore itemRestore;
            if (info.IsView)
            {
                itemRestore = new Ave2019ViewRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else if (info.OriginalRowId <= 0)
            {
                itemRestore = new Ave2019SystemFileRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else if (info.ParentLibraryIsMasterPageGallery)
            {
                itemRestore = new Ave2019MasterPageDocumentRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else if (IsPageLibrary(info))
            {
                itemRestore = new Ave2019PageFileRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else if (info.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                itemRestore = new Ave2019XmlFileRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else if ((info.AveItem.Folder.ParentList != null && info.AveItem.Folder.ParentList.IsOneDriveLibrary)
                || WrapperConfiguration.KeepVersionSettingDuringRestore)
            {
                //也可以用此方法还原普通的Document，不开关Version
                itemRestore = new Ave2019OneDriveDocumentRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            else
            {
                itemRestore = new Ave2019OrdinaryFileRestore(mContext as AveClientContext, mRequest as AveClientOM2019Request, mObj, info, fileStream);
            }
            itemRestore.SetReport(mReport);
            return itemRestore;
        }

        public void Dispose()
        {
            base.Dispose();
            //TODO
        }
    }
}