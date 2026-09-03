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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.ClientOM
{
    public interface IFolderVersionIncreaser
    {
        void UpdateToSpecificVersion(ListItem spListItem, int originalVersion, bool deleteBaseVersion);
    }
    public class AveFolderVersionIncreaser 
    {
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFolderVersionIncreaser));
        protected ClientContext mContext;
        protected List mParentList;
        protected IAveWeb mAveWebCache;
        protected AveClientOM2013Request mRequest;
        protected object mObj;
        protected int mVersion;
    }

    public class AveDocLibFolderVersionIncreaser : AveFolderVersionIncreaser, IFolderVersionIncreaser
    {
        private AveListMemento memento;
        private int mModerationStatus;
        public AveDocLibFolderVersionIncreaser(ClientContext context, AveClientOM2013Request request, object obj, IAveWeb aveWeb, List parentList,int moderationStatus)
        {
            mContext = context;
            mParentList = parentList;
            mAveWebCache = aveWeb;
            mRequest = request;
            mObj = obj;
            mModerationStatus = moderationStatus;
        }

        private void PrepareListSetting(int moderationStatus, bool is512MinorVersion)
        {
            memento = new AveListMemento(mParentList);
            memento.EnableVersionSetting(moderationStatus, is512MinorVersion);
        }

        public void IncreaseMajorVersion(ListItem item, int targetVesion)
        {
            int currentVersion = mVersion / 512;
            while (currentVersion < targetVesion) 
            { 
                item.Update();
                item["_ModerationStatus"] = 0;
                item.Update();
                currentVersion += 1;
                mVersion = currentVersion * 512;
            }
        }

        public void IncreaseMinorVersion(ListItem spListItem, int targetVersion)
        {
            int currentMinorVersion = mVersion % 512;
            while (currentMinorVersion < targetVersion) 
            {
                spListItem.Update();
                currentMinorVersion += 1;
            }
        }

        public void RevertListSetting()
        {
            memento.RevertVersionSettings();
        }

        public void UpdateToSpecificVersion(ListItem spListItem, int originalVersion, bool deleteBaseVersion)
        {
            if (spListItem == null) 
            {
                return;
            }
            mVersion = (int)spListItem["_UIVersion"];
            //originalVersion / 512 == 1： 特殊case，on-premise 不开启moderation创建的document set 小version(此种小verison必然是1.x)需要关闭Moderation update version
            PrepareListSetting(mModerationStatus, originalVersion / 512 == 1);
            IncreaseMajorVersion(spListItem, originalVersion / 512);
            IncreaseMinorVersion(spListItem, originalVersion % 512);
            RevertListSetting();
            mContext.ExecuteQuery();
        }
    }

    public class AveListFolderVersionIncreaser : AveFolderVersionIncreaser, IFolderVersionIncreaser
    {
        public AveListFolderVersionIncreaser(ClientContext context, AveClientOM2013Request request, object obj, IAveWeb aveWeb, List parentList)
        {
            mContext = context;
            mParentList = parentList;
            mAveWebCache = aveWeb;
            mRequest = request;
            mObj = obj;
        }

        public void UpdateToSpecificVersion(ListItem spListItem, int originalVersion, bool deleteBaseVersion)
        {
            if (spListItem == null)
            {
                return;
            }
            mVersion = (int)spListItem["_UIVersion"];
            //UnLockItem(spListItem);            
            List<int> versionLabels = new List<int>();

            if (deleteBaseVersion)
            {
                versionLabels.Add(mVersion);
            }
            int preVersion = -1;
            while (originalVersion > mVersion)
            {
                if (originalVersion % 512 == 0)
                {
                    if (mParentList.EnableMinorVersions)
                    {
                        mParentList.EnableMinorVersions = false;
                        mParentList.Update();
                    }
                }
                spListItem.Update();
                if (mVersion < 512)
                {
                    mVersion = 512;
                }
                else
                {
                    mVersion += 512;
                }
                if (preVersion == mVersion)
                {
                    return;
                }
                preVersion = mVersion;
                versionLabels.Add(mVersion);
            }

            if (versionLabels.Count > 0)
            {
                mContext.ExecuteQuery();
                //delete middle versions
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mParentList.Id.ToString();
                string fileName = spListItem["FileRef"].ToString();
                string op = "Delete";

                for (int i = 0; i < versionLabels.Count; i++)
                {
                    try
                    {
                        mRequest.OperateOnVersion(mAveWebCache.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, spListItem.Id, versionLabels[i], listId, fileName, op);
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveClientOMRequestResource.UpdateToSpecificVersionError, versionLabels[i], fileName, e.ToString());
                    }
                }
            }
        }
    }
}
