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
using Microsoft.SharePoint.Client;

namespace AvePoint.ObjectModel.ClientOM
{
    internal class MajorOnlyIncreseVersion : OneDriveUpdaterbase
    {
        public MajorOnlyIncreseVersion(File file, bool keepCheckOut)
            : base(file, keepCheckOut)
        {

        }

        // 1. Increase Major Version        
        // 3. Delete Version
        public override void UpdateFileVersion(int originalVersion, int currentFileVersion)
        {
            base.UpdateFileVersion(originalVersion, currentFileVersion);
            if (originalVersion % 512 != 0)
            {
                throw new AveVersionConflictException(originalVersion, currentFileVersion);
            }
            if (originalVersion == currentFileVersion)
            {//只开CurrentVersion的时候，只有部分属性能够更新暂时不支持
                UpdateWithoutChangeVersion();
            }

            //需要提升的MajorVersion个数
            var majorVersionIncre = originalVersion / 512 - currentFileVersion / 512;

            var needIncreaseMajor = majorVersionIncre != 0;

            if (needIncreaseMajor)
            {
                IncreaseMajorVersion(majorVersionIncre, false);
            }
            DeleteMiddleVersion();
        }

        /// <summary>
        /// 只开Major Version的时候，无法通过Checkout来保证不长Version,此处需要注意，事件中不能用长Version的操作
        /// </summary>
        protected override void UpdateWithoutChangeVersion()
        {
            base.TryInvokeDocumentUpdateForEqualVersionEventOnlyOneTime();
        }

        protected override void IncreaseMajorVersion(int majorVersionIncre, bool needIncreaseMinor)
        {
            for (int i = 0; i < majorVersionIncre; i++)
            {
                File.CheckOut();
                TryInvokeDcoumentUpdateEventOnlyOneTime();
                if (!(keepCheckOut && i + 1 == majorVersionIncre && originalDocumentVersion % 512 == 0))
                {
                    File.CheckIn(this.CheckInComment, CheckinType.MajorCheckIn);
                    ResetModifiedByAfterCheckIn();
                }
                IncreateMajorVersionNumber();
                RecordNeedDeletedVersion();
            }
        }
    }
}