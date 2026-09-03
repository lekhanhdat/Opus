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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Deployment;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    public class AveDeploymentSettings : IAveDeploymentSettings
    {
        private SPDeploymentSettings mDeploymentSettings;

        public AveDeploymentSettings()
        { }

        public AveDeploymentSettings(SPDeploymentSettings deploymentSettings)
        {
            mDeploymentSettings = deploymentSettings;
        }

        internal SPDeploymentSettings DeploymentSettings
        {
            get
            {
                return mDeploymentSettings;
            }
        }

        public AveIncludeSecurity IncludeSecurity
        {
            get
            {
                return (AveIncludeSecurity)(mDeploymentSettings.IncludeSecurity);
            }
            set
            {
                mDeploymentSettings.IncludeSecurity = (SPIncludeSecurity)value;
            }
        }

        public bool FileCompression
        {
            get
            {
                return mDeploymentSettings.FileCompression;
            }
            set
            {
                mDeploymentSettings.FileCompression = value;
            }
        }

        public string BaseFileName
        {
            get
            {
                return mDeploymentSettings.BaseFileName;
            }
            set
            {
                mDeploymentSettings.BaseFileName = value;
            }
        }

        public string SiteUrl
        {
            get
            {
                return mDeploymentSettings.SiteUrl;
            }
            set
            {
                mDeploymentSettings.SiteUrl = value;
            }
        }

        public string FileLocation
        {
            get
            {
                return mDeploymentSettings.FileLocation;
            }
            set
            {
                mDeploymentSettings.FileLocation = value;
            }
        }

        public string LogFilePath
        {
            get
            {
                return mDeploymentSettings.LogFilePath;
            }
            set
            {
                mDeploymentSettings.LogFilePath = value;
            }
        }
    }
}
