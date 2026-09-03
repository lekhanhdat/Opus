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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using Microsoft.Ceres.Diagnostics.Administration;

namespace AvePoint.ObjectModel.Server16
{
    class AveDiagnosticsService : AveDiagnosticsServiceBase, IAveDiagnosticsService
    {
        private SPDiagnosticsService mDiagnosticeService;
        private AveDiagnosticsService mLocal;
        private DiagnosticsService m_DiagnosticeService;
        public AveDiagnosticsService(SPDiagnosticsService diagnosticsService)
            : base(diagnosticsService)
        {
            mDiagnosticeService = diagnosticsService;
        }
        public AveDiagnosticsService(DiagnosticsService diagnosticsService)
            : base(diagnosticsService)
        {
            m_DiagnosticeService = diagnosticsService;
        }
        public AveDiagnosticsService()
            : this(new SPDiagnosticsService())
        {
            m_DiagnosticeService = new DiagnosticsService();
        }

        public AveDiagnosticsService(string name, IAveFarm parent)
            : this(new SPDiagnosticsService(name, (parent as AveFarm).Farm))
        {
            m_DiagnosticeService = new DiagnosticsService(name, (parent as AveFarm).Farm);
        }

        public IAveDiagnosticsService Local
        {
            get
            {
                if (mLocal == null)
                {
                    SPDiagnosticsService diagnosticsService = SPDiagnosticsService.Local;
                    if (diagnosticsService != null)
                    {
                        mLocal = new AveDiagnosticsService(diagnosticsService);
                    }
                }
                return mLocal;
            }
        }

        public bool CEIPEnabled
        {
            get
            {
                return mDiagnosticeService.CEIPEnabled;
            }
            set
            {
                mDiagnosticeService.CEIPEnabled = value;
            }
        }

        public bool DownloadErrorReportingUpdates
        {
            get
            {
                return mDiagnosticeService.DownloadErrorReportingUpdates;
            }
            set
            {
                mDiagnosticeService.DownloadErrorReportingUpdates = value;
            }
        }

        public bool ErrorReportingAutomaticUpload
        {
            get
            {
                return mDiagnosticeService.ErrorReportingAutomaticUpload;
            }
            set
            {
                mDiagnosticeService.ErrorReportingAutomaticUpload = value;
            }
        }

        public bool ErrorReportingEnabled
        {
            get
            {
                return mDiagnosticeService.ErrorReportingEnabled;
            }
            set
            {
                mDiagnosticeService.ErrorReportingEnabled = value;
            }
        }

        public bool ScriptErrorReportingEnabled
        {
            get
            {
                return mDiagnosticeService.ScriptErrorReportingEnabled;
            }
            set
            {
                mDiagnosticeService.ScriptErrorReportingEnabled = value;
            }
        }
    }
}
