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
using Microsoft.SharePoint.Client;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveListMemento
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveListMemento));
        private ClientContext mContext;
        private List mList;
        private RestoreListInfo mListInfo;//instead of mList
        private bool mStatus = false;
        private AveListUpdateOption mFlag;
        private bool mEnableMinorVersions = false;
        private bool mEnableVersioning = false;
        private bool mEnableModeration = false;
        private DraftVisibilityType mDraftVersionVisibility = DraftVisibilityType.Approver;
        private bool mForceCheckOut = false;
        private File mPage;

        public bool EnableMinorVersions
        {
            get { return mEnableMinorVersions; }
        }
        public bool EnableVersioning
        {
            get { return mEnableVersioning; }
        }
        public bool EnableModeration
        {
            get { return mEnableModeration; }
        }
        
        public AveListMemento(ClientContext context, List list)
        {
            mContext = context;
            mList = list;
        }

        public AveListMemento(ClientContext context, List list,File page)
        {
            mContext = context;
            mList = list;
            mPage = page;
        }

        [Obsolete("Use constructor without RestoreListInfo parameter")]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="list">Used to update list or get items</param>
        /// <param name="listInfo">Used to get list basic properties</param>
        public AveListMemento(ClientContext context, List list, RestoreListInfo listInfo)
        {
            mContext = context;
            mList = list;
            mListInfo = listInfo;
        }

        public void DisableVersionSettings()
        {
            mEnableMinorVersions = mList.EnableMinorVersions;
            mEnableVersioning = mList.EnableVersioning;
            mEnableModeration = mList.EnableModeration;
            mDraftVersionVisibility = mList.DraftVersionVisibility;
            mForceCheckOut = mList.ForceCheckout;
            if (mList.EnableVersioning && mList.BaseTemplate != (int)ListTemplateType.Survey)
            {
                if (!mList.EnableMinorVersions || (mPage != null && mPage.IsPropertyAvailable("UIVersion") && mPage.UIVersion % 512 == 0))
                {
                    //如果只开启大Version或者开启小version但是当前Version是大version的document，关闭version即可，
                    //但是小version document关闭version后操作document会导致document张为大version 
                    mList.EnableVersioning = false;
                    mStatus = true;
                    mFlag = mFlag | AveListUpdateOption.UpdateVersionSettings;
                }
            }
            if (mList.EnableModeration && !mList.HasExternalDataSource && mList.BaseTemplate != (int)ListTemplateType.Survey)
            {
                mList.EnableModeration = false;
                mStatus = true;
                mFlag = mFlag | AveListUpdateOption.UpdateModeration;
            }
            if (!mList.HasExternalDataSource && mList.ForceCheckout)
            {
                mList.ForceCheckout = false;
                mStatus = true;
                mFlag = mFlag | AveListUpdateOption.UpdateForceCheckout;
            }
            if (mList.DraftVersionVisibility != DraftVisibilityType.Reader)
            {
                mList.DraftVersionVisibility = DraftVisibilityType.Reader;
                mStatus = true;
                mFlag = mFlag | AveListUpdateOption.UpdateDraftVersionVisibility;
            }
            if (mStatus)
            {
                mList.Update();
            }
        }

        public void RevertVersionSettings()
        {
            try
            {
                if (mList != null)
                {
                    if ((mFlag & AveListUpdateOption.UpdateVersionSettings) != 0)
                    {
                        mList.EnableVersioning = mEnableVersioning;
                    }
                    if ((mFlag & AveListUpdateOption.UpdateModeration) != 0)
                    {
                        mList.EnableModeration = mEnableModeration;
                    }
                    if ((mFlag & AveListUpdateOption.UpdateVersionSettings) != 0)
                    {
                        mList.EnableMinorVersions = mEnableMinorVersions;
                    }
                    if ((mFlag & AveListUpdateOption.UpdateForceCheckout) != 0)
                    {
                        mList.ForceCheckout = mForceCheckOut;
                    }
                    if ((mFlag & AveListUpdateOption.UpdateDraftVersionVisibility) != 0)
                    {
                        mList.DraftVersionVisibility = mDraftVersionVisibility;
                    }
                    mList.Update();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("An error occurred while updating list at last.Message:{0}.", ex.ToString());
                //存在一些hidden list，不能update setting。
            }
        }
    }
    internal enum AveListUpdateOption
    {
        None = 0,
        UpdateVersionSettings = 1,
        UpdateModeration = 2,
        UpdateForceCheckout = 4,
        UpdateDraftVersionVisibility = 8
    }
}


