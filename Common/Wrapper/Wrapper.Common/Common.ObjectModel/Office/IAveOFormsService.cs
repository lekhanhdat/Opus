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
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOFormsService : IAveService
    {
        IAveOFormTemplateCollection FormTemplates { get; }
        string ServiceName { get; }
        int MaxDataConnectionResponseSize { get; set; }
        int MaxDataConnectionTimeout { get; set; }
        int MaxPostbacksPerSession { get; set; }
        int MaxSizeOfUserFormState { get; set; }
        int MaxUserActionsPerPostback { get; set; }
        int MemoryCacheSize { get; set; }
        bool RequireSslForDataConnections { get; set; }
        bool AllowUserFormCrossDomainDataConnections { get; set; }
        bool AllowUserFormBrowserEnabling { get; set; }
        bool AllowUserFormBrowserRendering { get; set; }
        bool AllowUdcAuthenticationForDataConnections { get; set; }
        bool AllowEmbeddedSqlForDataConnections { get; set; }
        int DefaultDataConnectionTimeout { get; set; }
        int ActiveSessionsTimeout { get; set; }
        IAveODataConnectionFileCollection DataConnectionFiles { get; }

        void AllowUserFormWebServiceProxy(Uri webApplicationUri, bool enable);
        void AllowWebServiceProxy(Uri webApplicationUri, bool enable);
        void Provision();
        void Update(bool ensure);
    }
}
