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
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWebApplicationBuilder
    {
        string ApplicationPoolId { get; set; }
        IAveManagedAccount ManagedAccount { get; set; }
        AveIdentityType IdentityType { get; set; }
        IAveSearchServiceInstance SearchServiceInstance { get; set; }
        bool AllowAnonymousAccess { get; set; }
        bool CreateNewDatabase { get; set; }
        string DatabaseName { get; set; }
        string DatabasePassword { get; set; }
        string DatabaseServer { get; set; }
        string DatabaseUsername { get; set; }
        Uri DefaultZoneUri { get; set; }
        string HostHeader { get; set; }
        Guid ID { get; set; }
        int Port { get; set; }
        DirectoryInfo RootDirectory { get; set; }
        string ServerComment { get; set; }
        bool UseNTLMExclusively { get; set; }
        bool UseSecureSocketsLayer { get; set; }
        IAveWebService WebService { get; set; }

        IAveWebApplication Create();
    }
}
