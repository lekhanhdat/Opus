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
global using Newtonsoft.Json;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using AvePoint.RA.Common.Cache;
global using AvePoint.RA.Common.Aos;
global using AvePoint.GCommon.Utility;
global using Cloud.Sdk.Data.AosModern;
global using AvePoint.RA.Contract.Aos;
global using AvePoint.RA.CommonUtil;
global using AvePoint.RA.Contract.Services;
global using SfMetadataApi;
global using System.Net.Http.Headers;
global using Util;
global using SforceService;
global using System.Text.RegularExpressions;
global using System.Globalization;
global using System.Collections.Concurrent;
global using AvePoint.RA.Contract.Discovery.Model.Configuration;
