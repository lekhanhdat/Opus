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
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.App
{
    public static class RMGraphAppIdConstant
    {
        public const string GRAPH_API = "00000003-0000-0000-c000-000000000000";
    }

    public static class RMGraphAppAccessIdConstant
    {
        #region GRAPH_API

        public const string MAIL_SEND = "b633e1c5-b582-4048-a93e-9f11b44c7e96";

        public const string USER_READ_ALL = "df021288-bdef-4463-88db-98f22de89214";

        public const string DIRECTORY_READ_ALL = "7ab1d382-f21e-4acd-a863-ba3e13f7da61";

        #endregion
    }

    public static class RMGraphAppPermissionRelationshipConstant
    {
        public static Dictionary<string, List<string>> GRAPH_API => new ()
        {
            {
                RMGraphAppIdConstant.GRAPH_API,
                new List<string>
                {
                    RMGraphAppAccessIdConstant.MAIL_SEND,
                    RMGraphAppAccessIdConstant.USER_READ_ALL,
                    RMGraphAppAccessIdConstant.DIRECTORY_READ_ALL
                }
            }
        };
    }
}
