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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveHtmlTransformSettings : AveAutoSerializingObject,IAveHtmlTransformSettings
    {
        private SPHtmlTransformSettings mSPHtmlTransformSettings;

        public AveHtmlTransformSettings(SPHtmlTransformSettings htmlTransformSettings)
            : base(htmlTransformSettings)
        {
            mSPHtmlTransformSettings = htmlTransformSettings;
        }

        #region IAveHtmlTransformSettings Members


        public bool Enabled
        {
            get
            {
                return mSPHtmlTransformSettings.Enabled;
            }
            set
            {
                mSPHtmlTransformSettings.Enabled = value;
            }
        }

        public int MaximumCacheSize
        {
            get
            {
                return mSPHtmlTransformSettings.MaximumCacheSize;
            }
            set
            {
                mSPHtmlTransformSettings.MaximumCacheSize = value;
            }
        }

        public int MaximumFileSize
        {
            get
            {
                return mSPHtmlTransformSettings.MaximumFileSize;
            }
            set
            {
                mSPHtmlTransformSettings.MaximumFileSize = value;
            }
        }

        public string ServerLocation
        {
            get
            {
                return mSPHtmlTransformSettings.ServerLocation;
            }
            set
            {
                mSPHtmlTransformSettings.ServerLocation = value;
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                return mSPHtmlTransformSettings.Timeout;
            }
            set
            {
                mSPHtmlTransformSettings.Timeout = value;
            }
        }

        #endregion
    }
}
