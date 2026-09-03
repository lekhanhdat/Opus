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
using System.Text;
using System.Collections;
using AvePoint.Media.Storage.Cloud.Azure;

namespace AvePoint.Media.Storage.Cloud.Common
{
    abstract class AbstractCloudSystemWrapper : IChangeArrayListResultsListener
    {
        public IXSystem system { set; get; }
        public AbstractRESTOprationExecutor client{set;get;}
        public SubDirsAndFilesBean sd { set; get; }
        public SubDirsAndFilesBean sdtemp { set; get; }

        public AbstractCloudSystemWrapper(IXSystem system , AbstractRESTOprationExecutor client)
        {
            this.system = system;
            this.client = client;
            this.sd = new SubDirsAndFilesBean();
            this.sdtemp = new SubDirsAndFilesBean(); 

        }

        public abstract void GetListSubDirectoriesAndFilesCount(StorageInfo storageInfo);

        public abstract ArrayList GetNextDirsResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo);

        public abstract ArrayList GetNextFilesResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo);

        public virtual void ListResultsToArrayList(List<XDirectoryInfo> dirsList, List<XFileInfo> filesList, ArrayList dirs, ArrayList files)
        {
            if (dirsList != null)
                for (int i = 0; i < dirsList.Count; i++)
                    dirs.Add((object)dirsList[i]);

            if (filesList != null)
                for (int i = 0; i < filesList.Count; i++)
                    files.Add((object)filesList[i]);
        }

        public abstract int GetDirsResultsCount();

        public abstract int GetFilesResultsCount();
    }
}
