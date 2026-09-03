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
using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
using AvePoint.Media.ClassicStorage.Cloud.Azure.GetCount;

namespace AvePoint.Media.ClassicStorage.Cloud.Common.Warpper.SystemWrapper
{
    public abstract class AbstractCloudSystemWrapper : IChangeArrayListResultsListener
    {
        public IXSystemCommon system { set; get; }
        public AbstractRESTOprationExecutor client{set;get;}
        public SubDirsAndFilesBean sd { set; get; }
        public SubDirsAndFilesBean sdtemp { set; get; }

        public AbstractCloudSystemWrapper(IXSystemCommon system , AbstractRESTOprationExecutor client)
        {
            this.system = system;
            this.client = client;
            this.sd = new SubDirsAndFilesBean();
            this.sdtemp = new SubDirsAndFilesBean(); 

        }

        public virtual void GetListSubDirectoriesAndFilesCount(StorageInfo storageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual System.Collections.ArrayList GetNextDirsResults(Client.ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual System.Collections.ArrayList GetNextFilesResults(Client.ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void ListResultsToArrayList(List<XDirectoryInfo> dirsList, List<XFileInfo> filesList, ArrayList dirs, ArrayList files)
        {
            if (dirsList != null)
                for (int i = 0; i < dirsList.Count; i++)
                    dirs.Add((object)dirsList[i]);

            if (filesList != null)
                for (int i = 0; i < filesList.Count; i++)
                    files.Add((object)filesList[i]);
        }

        public virtual int GetDirsResultsCount()
        {
            throw new NotImplementedException();
        }

        public virtual int GetFilesResultsCount()
        {
            throw new NotImplementedException();
        }
    }
}
