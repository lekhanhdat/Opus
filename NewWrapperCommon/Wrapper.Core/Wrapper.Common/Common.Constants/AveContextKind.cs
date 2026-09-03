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
namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public enum AveContextKind
    {
        Auto = 0,

        ClientObjectModel = 1, 
        [Obsolete("use Server10ObjectModel instead.")]
        ServerObjectModel = 2,
        Server07ObjectModel = 3,
        WebServiceObjectModel = 4,
        Server13ObjectModel = 5,
        Server10ObjectModel = 6,
        Server16ObjectModel = 7,
        Server19ObjectModel = 8,
        ServerSEObjectModel = 9,
    }

    public static class AveContextKindExtension
    {
        public static bool IsServerMode(this AveContextKind kind)
        {
            return kind != AveContextKind.ClientObjectModel;
        }
        /// <summary>
        /// 由于13 的值比10要小 因此这么判断
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsServerMode10Upper(this AveContextKind kind)
        {
            return kind >= AveContextKind.Server13ObjectModel ||
                kind == AveContextKind.ServerObjectModel;
        }

        public static bool IsServerMode13Upper(this AveContextKind kind)
        {
            return kind >= AveContextKind.Server13ObjectModel
                && kind != AveContextKind.Server10ObjectModel;
        }

        public static bool IsServerMode16Upper(this AveContextKind kind)
        {
            return kind >= AveContextKind.Server16ObjectModel;
        }
    }

    public enum AveDiscoveryKind
    {
        Database = 1,
        API = 2,
        ServerAPI = 3,
    }

    public enum AveSOIntegrationKind
    {
        Auto = 0
    }
}