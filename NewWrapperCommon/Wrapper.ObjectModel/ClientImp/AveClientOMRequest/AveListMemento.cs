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

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveListMemento
    {
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveListMemento));
        private List mList;
        private bool mStatus = false;
        private bool mEnableMinorVersions = false;
        private bool mEnableVersioning = false;
        private bool mEnableModeration = false;
        private DraftVisibilityType mDraftVersionVisibility = DraftVisibilityType.Approver;
        private bool mForceCheckOut = false;
        private bool? mIsListSettingChanged;

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
        public AveListMemento(List list)
        {
            mList = list;
        }
        public bool IsListSettingChanged
        {
            get { return mIsListSettingChanged.HasValue ? mIsListSettingChanged.Value : false; }
        }

        public void EnableVersionSetting(int moderationStatus, bool isMinorVersion)
        {
            mEnableVersioning = mList.EnableVersioning;
            mEnableMinorVersions = mList.EnableMinorVersions;
            mEnableModeration = mList.EnableModeration;
            if (!mList.EnableVersioning || !mList.EnableMinorVersions)
            {
                mList.EnableVersioning = true;
                mList.EnableMinorVersions = true;
                mStatus = true;
            }
            if (!mList.EnableModeration)
            {
                mList.EnableModeration = true;
                mStatus = true;
            }
            if (moderationStatus == 0 && isMinorVersion && mList.EnableModeration)
            {
                mList.EnableModeration = false;
                mStatus = true;
            }
            if (mStatus)
            {
                mList.Update();
            }
        }

        public void SetListSetting(bool? EnableVersioning, bool? EnableMinorVersions, bool? EnableModeration, bool? forceCheckOut)
        {
            if (mList == null)
            {
                throw new ArgumentNullException("List should not be null, when update list settings");
            }
            bool changed = false;
            if (EnableVersioning != null && mList.EnableVersioning != EnableVersioning.Value)
            {
                mList.EnableVersioning = EnableVersioning.Value;
                changed = true;
            }
            if (EnableMinorVersions != null && mList.EnableMinorVersions != EnableMinorVersions.Value)
            {
                mList.EnableMinorVersions = EnableMinorVersions.Value;
                changed = true;
            }
            if (EnableModeration != null && mList.EnableModeration != EnableModeration.Value)
            {
                mList.EnableModeration = EnableModeration.Value;
                changed = true;
            }
            if (forceCheckOut != null && mList.ForceCheckout != forceCheckOut.Value)
            {
                mList.ForceCheckout = forceCheckOut.Value;
                changed = true;
            }
            if (changed)
            {
                mList.Update();
                if (!mIsListSettingChanged.HasValue)
                {
                    mIsListSettingChanged = true;
                }
                mList.Context.Load(mList);
            }
        }

        public void DisableVersionSettings()
        {
            mEnableMinorVersions = mList.EnableMinorVersions;
            mEnableVersioning = mList.EnableVersioning;
            mEnableModeration = mList.EnableModeration;
            mDraftVersionVisibility = mList.DraftVersionVisibility;
            mForceCheckOut = mList.ForceCheckout;

            if (mList.EnableVersioning && !mList.EnableMinorVersions && mList.BaseTemplate != (int)ListTemplateType.Survey)
            {
                mList.EnableVersioning = false;
                mStatus = true;
            }
            if (mList.EnableModeration && !mList.HasExternalDataSource && mList.BaseTemplate != (int)ListTemplateType.Survey)
            {
                mList.EnableModeration = false;
                mStatus = true;
            }
            if (!mList.HasExternalDataSource && mList.ForceCheckout)
            {
                mList.ForceCheckout = false;
                mStatus = true;
            }
            if (mList.DraftVersionVisibility != DraftVisibilityType.Reader)
            {
                mList.DraftVersionVisibility = DraftVisibilityType.Reader;
                mStatus = true;
            }
            if (mStatus)
            {
                mList.Update();
            }
        }

        public void DisableEnableModeration()
        {
            mEnableModeration = mList.EnableModeration;
            if (mList.EnableModeration)
            {
                mList.EnableModeration = false;
                mStatus = true;
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
                if (mStatus && mList != null)
                {
                    if (mList.EnableVersioning != mEnableVersioning)
                    {
                        mList.EnableVersioning = mEnableVersioning;
                    }
                    if (mList.EnableModeration != mEnableModeration)
                    {
                        mList.EnableModeration = mEnableModeration;
                    }
                    if (mList.EnableMinorVersions != mEnableMinorVersions)
                    {
                        mList.EnableMinorVersions = mEnableMinorVersions;
                    }
                    if (mList.ForceCheckout != mForceCheckOut)
                    {
                        mList.ForceCheckout = mForceCheckOut;
                    }
                    if (mList.DraftVersionVisibility != mDraftVersionVisibility)
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

        public Dictionary<string, object> ExportCurrentListSetting()
        {
            var settings = new Dictionary<string, object>();
            settings["EnableVersioning"] = mList.EnableVersioning;
            settings["EnableMinorVersions"] = mList.EnableMinorVersions;
            settings["EnableModeration"] = mList.EnableModeration;
            settings["ForceCheckout"] = mList.ForceCheckout;
            settings["DraftVersionVisibility"] = mList.DraftVersionVisibility;
            return settings;
        }
    }
}
