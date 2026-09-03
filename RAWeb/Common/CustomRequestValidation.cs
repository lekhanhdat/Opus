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
using System.Web;
using System.Web.Util;

namespace AvePoint.RA.Web.Common
{
    public class CustomRequestValidation : RequestValidator
    {
        public CustomRequestValidation() { }

        protected override bool IsValidRequestString(HttpContext context, string value,RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            //Set a default value for the out parameter.  
            validationFailureIndex = -1;
            if (requestValidationSource == RequestValidationSource.Form && collectionKey == "user" 
                && context.Request.Url.AbsolutePath.EndsWith("/Account/logon", StringComparison.OrdinalIgnoreCase))
            {
                // The form user value is allowed.  
                validationFailureIndex = -1;
                return true;
            }
            // All other HTTP input checks fall back to   
            // the base ASP.NET implementation.  
            else
            {
                return base.IsValidRequestString(context, value,
                    requestValidationSource, collectionKey,
                    out validationFailureIndex);
            }
        }
    }
}