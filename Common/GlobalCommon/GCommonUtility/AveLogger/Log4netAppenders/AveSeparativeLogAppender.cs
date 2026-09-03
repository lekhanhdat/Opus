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


namespace AvePoint.GCommon
{
    using System;

    class AveSeparativeLogAppender : AveRollingFileAppender
    {
        private bool m_NeedWriteFooter = true;

        public bool NeedWriteFooter
        {
            get { return m_NeedWriteFooter; }
            set { m_NeedWriteFooter = value; }
        }
        protected override void OnClose()
        {
            if (m_NeedWriteFooter)
            {
                base.OnClose();
            }
            m_NeedWriteFooter = true;
        }

        public override void ActivateOptions()
        {
            //让logname按正序增长，防止上传删除之后新建log引起与log storage中重名
            base.StaticLogFileName = true;
            base.CountDirection = 1;
            base.ActivateOptions();
            if (base.RollingStyle == RollingMode.Date || base.RollingStyle == RollingMode.Composite)
            {
                using (base.SecurityContext.Impersonate(this))
                {
                    try
                    {
                        DateTime logCreationTime = System.IO.File.GetCreationTime(this.File);
                        base.NextCheck = NextCheckDate(logCreationTime, base.RollPoint);
                    }
                    catch (Exception)
                    {
                        base.NextCheck = NextCheckDate(DateTime.Now, base.RollPoint);
                    }
                }
            }
        }

        protected override void AdjustFileBeforeAppend()
        {
            MultiTenantFileLocker locker = base.LockingModel as MultiTenantFileLocker;
            if (locker == null)
            { 
                return;
            }
            base.AdjustFileBeforeAppend();
        }

        public override string File
        {
            get
            {
                MultiTenantFileLocker locker = base.LockingModel as MultiTenantFileLocker;
                return locker.FileName;
            }
            set
            {
                base.File = value;
            }
        }

        public string BaseFileName
        {
            get
            {
                MultiTenantFileLocker locker = base.LockingModel as MultiTenantFileLocker;
                return locker.BaseFileName;
            }
        }

        public void DeleteLog(string fileName)
        {
            base.DeleteFile(fileName);
        }
    }
}
