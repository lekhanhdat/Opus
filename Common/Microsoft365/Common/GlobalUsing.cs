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

global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Sockets;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.Kiota.Abstractions;
global using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
global using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
global using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

global using Microsoft365.Common.Middleware;

//global using Microsoft365Backup.Logger;