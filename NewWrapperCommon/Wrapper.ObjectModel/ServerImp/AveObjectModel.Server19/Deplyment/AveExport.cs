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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Deployment;

namespace AvePoint.ObjectModel.Server19
{
    #region IAveExport Members

    #endregion

    class AveExport : IAveExport, IDisposable
    {
        private SPExport mExport;

        public AveExport(IAveExportSettings settings)
        {
            mExport = new SPExport((settings as AveExportSettings).ExportSettings);
        }

        public void Run()
        {
            mExport.Run();
            mExport.Dispose();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mExport != null)
            {
                mExport.Dispose();
                mExport = null;
            }
        }

        #endregion

        public event AveDeploymentEvent<IAveDeploymentEventArgs> Canceled;

        public event AveDeploymentEvent<IAveDeploymentEventArgs> Completed;

        public event AveDeploymentEvent<IAveDeploymentEventArgs> ProgressUpdated;

        public event AveDeploymentEvent<IAveDeploymentEventArgs> Started;
    }
}
