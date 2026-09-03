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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveAntivirusSettings : AveAutoSerializingObject, IAveAntivirusSettings
    {
        private readonly string mAntivirusSettings_SetVendorId_Method = "SetVendorId";
        private SPAntivirusSettings mAntivirusSettings;

        public AveAntivirusSettings(SPAntivirusSettings settings)
            : base(settings)
        {
            mAntivirusSettings = settings;
        }

        #region IAveSPAntivirusSettings Members

        public bool UploadScanEnabled
        {
            get
            {
                return mAntivirusSettings.UploadScanEnabled;
            }
            set
            {
                mAntivirusSettings.UploadScanEnabled = value;
            }
        }

        public bool DownloadScanEnabled
        {
            get
            {
                return mAntivirusSettings.DownloadScanEnabled;
            }
            set
            {
                mAntivirusSettings.DownloadScanEnabled = value;
            }
        }

        public double TimeoutTotalSeconds
        {
            get { return mAntivirusSettings.Timeout.TotalSeconds; }
        }

        public int NumberOfThreads
        {
            get
            {
                return mAntivirusSettings.NumberOfThreads;
            }
            set
            {
                mAntivirusSettings.NumberOfThreads = value;
            }
        }

        public bool CleaningEnabled
        {
            get
            {
                return mAntivirusSettings.CleaningEnabled;
            }
            set
            {
                mAntivirusSettings.CleaningEnabled = value;
            }
        }

        public TimeSpan TimeOut
        {
            get { return mAntivirusSettings.Timeout; }
            set { mAntivirusSettings.Timeout = value; }
        }

        public bool AllowDownload
        {
            get { return mAntivirusSettings.AllowDownload; }
            set { mAntivirusSettings.AllowDownload = value; }
        }

        public void IncrementVendorUpdateCount()
        {
            mAntivirusSettings.IncrementVendorUpdateCount();
        }

        public int SetVendorId()
        {
            return (int)AveAssemblyUtility.InvokeMethod(mAntivirusSettings, mAntivirusSettings_SetVendorId_Method, new Type[] { }, new object[] { });
        }

        public int SetVendorId(int value)
        {
            return (int)AveAssemblyUtility.InvokeMethod(mAntivirusSettings, mAntivirusSettings_SetVendorId_Method, new Type[] { typeof(int) }, new object[] { value });
        }

        #endregion


        public bool AllowQuarantinedFileDownload
        {
            get
            {
                return mAntivirusSettings.AllowQuarantinedFileDownload;
            }
            set
            {
                mAntivirusSettings.AllowQuarantinedFileDownload = value;
            }
        }

        public bool SkipSearchCrawl
        {
            get
            {
                return mAntivirusSettings.SkipSearchCrawl;
            }
            set
            {
                mAntivirusSettings.SkipSearchCrawl = value;
            }
        }

        public int VendorUpdateCount
        {
            get
            {
                return mAntivirusSettings.VendorUpdateCount;
            }
            set
            {
                mAntivirusSettings.VendorUpdateCount = value;
            }
        }


        public TimeSpan Timeout
        {
            get
            {
                return mAntivirusSettings.Timeout;
            }
            set
            {
                mAntivirusSettings.Timeout = value;
            }
        }
    }
}
