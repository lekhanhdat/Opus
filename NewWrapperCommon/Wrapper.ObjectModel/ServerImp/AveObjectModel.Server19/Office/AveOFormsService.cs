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
using Microsoft.Office.InfoPath.Server.Administration;
using AvePoint.Wrapper.Common.Office;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOFormsService : AveService, IAveOFormsService
    {
        private FormsService mFormsService;

        public AveOFormsService(FormsService formsService)
            : base(formsService)
        {
            mFormsService = formsService;
        }

        public AveOFormsService()
            : this(new FormsService())
        { }

        public IAveOFormTemplateCollection FormTemplates
        {
            get { return new AveOFormTemplateCollection(mFormsService.FormTemplates); }
        }

        public string ServiceName
        {
            get { return FormsService.ServiceName; }
        }

        public int MaxDataConnectionResponseSize
        {
            get
            {
                return mFormsService.MaxDataConnectionResponseSize;
            }
            set
            {
                mFormsService.MaxDataConnectionResponseSize = value;
            }
        }

        public int MaxDataConnectionTimeout
        {
            get
            {
                return mFormsService.MaxDataConnectionTimeout;
            }
            set
            {
                mFormsService.MaxDataConnectionTimeout = value;
            }
        }

        public int MaxPostbacksPerSession
        {
            get
            {
                return mFormsService.MaxPostbacksPerSession;
            }
            set
            {
                mFormsService.MaxPostbacksPerSession = value;
            }
        }

        public int MaxSizeOfUserFormState
        {
            get
            {
                return mFormsService.MaxSizeOfUserFormState;
            }
            set
            {
                mFormsService.MaxSizeOfUserFormState = value;
            }
        }

        public int MaxUserActionsPerPostback
        {
            get
            {
                return mFormsService.MaxUserActionsPerPostback;
            }
            set
            {
                mFormsService.MaxUserActionsPerPostback = value;
            }
        }

        public int MemoryCacheSize
        {
            get
            {
                return mFormsService.MemoryCacheSize;
            }
            set
            {
                mFormsService.MemoryCacheSize = value;
            }
        }


        public bool RequireSslForDataConnections
        {
            get
            {
                return mFormsService.RequireSslForDataConnections;
            }
            set
            {
                mFormsService.RequireSslForDataConnections = value;
            }
        }

        public bool AllowUserFormCrossDomainDataConnections
        {
            get
            {
                return mFormsService.AllowUserFormCrossDomainDataConnections;
            }
            set
            {
                mFormsService.AllowUserFormCrossDomainDataConnections = value;
            }
        }

        public bool AllowUserFormBrowserEnabling
        {
            get
            {
                return mFormsService.AllowUserFormBrowserEnabling;
            }
            set
            {
                mFormsService.AllowUserFormBrowserEnabling = value;
            }
        }

        public bool AllowUserFormBrowserRendering
        {
            get
            {
                return mFormsService.AllowUserFormBrowserRendering;
            }
            set
            {
                mFormsService.AllowUserFormBrowserRendering = value;
            }
        }

        public bool AllowUdcAuthenticationForDataConnections
        {
            get
            {
                return mFormsService.AllowUdcAuthenticationForDataConnections;
            }
            set
            {
                mFormsService.AllowUdcAuthenticationForDataConnections = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public bool AllowEmbeddedSqlForDataConnections
        {
            get
            {
                return mFormsService.AllowEmbeddedSqlForDataConnections;
            }
            set
            {
                mFormsService.AllowEmbeddedSqlForDataConnections = value;
            }
        }

        public int DefaultDataConnectionTimeout
        {
            get
            {
                return mFormsService.DefaultDataConnectionTimeout;
            }
            set
            {
                mFormsService.DefaultDataConnectionTimeout = value;
            }
        }

        public int ActiveSessionsTimeout
        {
            get
            {
                return mFormsService.ActiveSessionsTimeout;
            }
            set
            {
                mFormsService.ActiveSessionsTimeout = value;
            }
        }

        public IAveODataConnectionFileCollection DataConnectionFiles
        {
            get { return new AveODataConnectionFileCollection(mFormsService.DataConnectionFiles); }
        }

        public void AllowUserFormWebServiceProxy(Uri webApplicationUri, bool enable)
        {
            mFormsService.AllowUserFormWebServiceProxy(webApplicationUri, enable);
        }

        public void AllowWebServiceProxy(Uri webApplicationUri, bool enable)
        {
            mFormsService.AllowWebServiceProxy(webApplicationUri, enable);
        }

        public void Provision()
        {
            mFormsService.Provision();
        }

        public void Update(bool ensure)
        {
            mFormsService.Update(ensure);
        }

        public void BrowserEnableUserFormTemplate(IAveFile formTemplateFile)
        {
            //mFormsService = new FormsService();
            mFormsService.BrowserEnableUserFormTemplate((formTemplateFile as AveFile).File);
        }
    }
}
