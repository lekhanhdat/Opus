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
using Microsoft.SharePoint.Client;

namespace AvePoint.ObjectModel.ClientOM
{
    internal abstract class OneDriveUpdaterbase
    {
        protected File File { get; private set; }
        protected bool updateEventInvoked;

        protected bool keepCheckOut;
        protected int currentDocumentVersion;
        protected int originalDocumentVersion;

        protected List<int> needDeletedVersionIds = new List<int>();

        public string CheckInComment { get; private set; }

        /// <summary>
        /// 事件中注册的方法内容会在CheckOut之后，CheckIn之前执行的，主要为了避免长Version
        /// </summary>
        internal event Action DcoumentUpdateEvent;

        /// <summary>
        /// 改事件只会在originalVersion == currentFileVersion的情况下被调用
        /// </summary>
        internal event Action DocumentUpdateForEqualVersionEvent;

        /// <summary>
        /// 由于我们是通过checkout--checkin 的方式keep version的，但是在checkin 时version的modified by会变为当前的user
        /// 因此，需要在checkin 后Reset modified by
        /// </summary>
        internal event Action ResetModifiedByAfterCheckInEvent;


        protected OneDriveUpdaterbase(File file)
        {
            this.File = file;
            currentDocumentVersion = file.UIVersion;
        }

        protected OneDriveUpdaterbase(File file, bool keepCheckOut)
            : this(file)
        {
            this.keepCheckOut = keepCheckOut;
        }

        public void SetCheckInComment(string checkInComment)
        {
            this.CheckInComment = checkInComment;
        }

        public virtual void UpdateFileVersion(int originalVersion, int currentFileVersion)
        {
            this.originalDocumentVersion = originalVersion;
        }

        protected virtual void IncreaseMajorVersion(int majorVersionIncre, bool needIncreaseMinor)
        {
            for (int i = 0; i < majorVersionIncre; i++)
            {
                File.CheckOut();
                TryInvokeDcoumentUpdateEventOnlyOneTime();
                if (!(keepCheckOut && i + 1 == majorVersionIncre && originalDocumentVersion % 512 == 0))
                {
                    File.CheckIn(this.CheckInComment, CheckinType.MajorCheckIn);
                }
                //同时开启大小version时 checkin 后调用ResetModifiedByEvent会导致涨version 暂时没有找到解决办法，
                //因此需要对于此种情况先不修改modified by
                IncreateMajorVersionNumber();
                RecordNeedDeletedVersion();
            }
        }

        public void AddNewFileNeedDeleteVersion(int originalDocumentVersion, int currentDocumentVersion)
        {
            if (currentDocumentVersion != originalDocumentVersion)
            {
                needDeletedVersionIds.Add(currentDocumentVersion);
            }
        }

        protected void ResetModifiedByAfterCheckIn()
        {
            ResetModifiedByAfterCheckInEvent.Invoke();
        }

        protected void RecordNeedDeletedVersion()
        {
            if (currentDocumentVersion != originalDocumentVersion)
            {
                needDeletedVersionIds.Add(currentDocumentVersion);
            }
        }

        protected void IncreateMajorVersionNumber()
        {
            currentDocumentVersion = (currentDocumentVersion / 512 + 1) * 512;
        }

        protected void IncreateMinorVersionNumber()
        {
            currentDocumentVersion++;
        }

        protected virtual void IncreaseMinorVersion(int minorVersionIncre)
        {

        }

        protected virtual void UpdateWithoutChangeVersion()
        {

        }

        /// <summary>
        /// 无论提升多少次Version，此方法只能执行一次
        /// </summary>
        protected virtual void TryInvokeDcoumentUpdateEventOnlyOneTime()
        {
            if (!updateEventInvoked)
            {
                DcoumentUpdateEvent.Invoke();
                updateEventInvoked = true;
            }
        }

        /// <summary>
        /// 无论提升多少次Version，此方法只能执行一次
        /// </summary>
        protected virtual void TryInvokeDocumentUpdateForEqualVersionEventOnlyOneTime()
        {
            if (!updateEventInvoked)
            {
                DocumentUpdateForEqualVersionEvent.Invoke();
                updateEventInvoked = true;
            }
        }

        public void DeleteMiddleVersion()
        {
            for (int index = 0; index < needDeletedVersionIds.Count; index++)
            {
                var needDeletedVersionId = needDeletedVersionIds[index];
                File.Versions.DeleteByID(needDeletedVersionId);
            }
        }

        internal static OneDriveUpdaterbase CreateIncreaseVersion(File file, bool keepCheckOut, bool enableMinorVersion, bool enableMajorVersion)
        {
            if (enableMinorVersion)
            {
                return new MjaorAndMinorIncreseVersion(file, keepCheckOut);
            }
            if (enableMajorVersion)
            {
                return new MajorOnlyIncreseVersion(file, keepCheckOut);
            }
            return new OneDriveDefault(file, keepCheckOut);
        }
    }
}