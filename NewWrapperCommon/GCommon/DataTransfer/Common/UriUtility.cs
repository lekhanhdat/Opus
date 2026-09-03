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

namespace AvePoint.GCommon.Transfer.Common
{
    internal class UriUtility
    {
        //public static Uri CreateUri(string schema, string agentAddr, int port, string relatedBaseUri, string serviceName)
        //{
        //    return CreateUri(schema, agentAddr, port, relatedBaseUri, serviceName, string.Empty);
        //}

        //public static Uri CreateUri(string schema, string agentAddr, int port, string relatedBaseUri, string serviceName, string jobId)
        //{
        //    UriBuilder ub = new UriBuilder();

        //    ub.Scheme = schema;
        //    ub.Host = agentAddr;
        //    ub.Port = port;

        //    if (string.IsNullOrEmpty(serviceName))
        //    {
        //        ub.Path = relatedBaseUri;
        //    }
        //    else
        //    {
        //        ub.Path = relatedBaseUri + "/" + serviceName;
        //    }

        //    if (!string.IsNullOrEmpty(jobId))
        //    {
        //        ub.Path = ub.Path + "/" + jobId;
        //    }

        //    return ub.Uri;
        //}

        /// <summary>
        /// 一般先是RelatedBaseUri，然后是ServiceName，最后是JobId
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="agentAddr"></param>
        /// <param name="port"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Uri CreateUri(string schema, string agentAddr, int port, params string[] paths)
        {
            StringBuilder builder = new StringBuilder();
            
            foreach (var path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(path);
                    }
                    else
                    {
                        builder.AppendFormat("/{0}", path);
                    }
                }
            }

            return new UriBuilder(schema, agentAddr, port, builder.ToString()).Uri;
        }
    }
}
