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
    internal class MjaorAndMinorIncreseVersion : OneDriveUpdaterbase
    {
        public MjaorAndMinorIncreseVersion(File file, bool keepCheckOut)
            : base(file, keepCheckOut)
        {
        }

        // 1. Increase Major Version
        // 2. Increase Minor Version
        // 3. Delete Version
        public override void UpdateFileVersion(int originalVersion, int currentFileVersion)
        {
            base.UpdateFileVersion(originalVersion, currentFileVersion);
            if (originalVersion == currentFileVersion)
            {
                UpdateWithoutChangeVersion();
            }
            else
            {
                //需要提升的MajorVersion个数
                var majorVersionIncre = originalVersion / 512 - currentFileVersion / 512;

                var needIncreaseMajor = majorVersionIncre != 0;

                //需要提升的MinorVersion个数
                var minorVersionIncre = needIncreaseMajor ? originalVersion % 512 : (originalVersion - currentFileVersion);

                var needIncreaseMinor = minorVersionIncre != 0;
                if (needIncreaseMajor)
                {
                    IncreaseMajorVersion(majorVersionIncre, needIncreaseMinor);
                }
                if (needIncreaseMinor)
                {
                    IncreaseMinorVersion(minorVersionIncre);
                }
            }
            DeleteMiddleVersion();
        }

        protected override void IncreaseMinorVersion(int minorVersionIncre)
        {
            for (int i = 0; i < minorVersionIncre; i++)
            {
                File.CheckOut();
                TryInvokeDcoumentUpdateEventOnlyOneTime();
                if (!(i == minorVersionIncre - 1 && keepCheckOut))
                {
                    File.CheckIn(CheckInComment, CheckinType.MinorCheckIn);
                    ResetModifiedByAfterCheckIn();
                }

                IncreateMinorVersionNumber();
                RecordNeedDeletedVersion();
            }
        }

        protected override void UpdateWithoutChangeVersion()
        {
            if (originalDocumentVersion % 512 != 0)
            {
                base.TryInvokeDocumentUpdateForEqualVersionEventOnlyOneTime();
            }
            else
            {
                throw new AveVersionConflictException(this.originalDocumentVersion, this.currentDocumentVersion);
            }
        }
    }
}