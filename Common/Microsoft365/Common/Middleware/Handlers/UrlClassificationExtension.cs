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
#nullable enable
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

//using Util.MSAzure;

namespace Microsoft365.Common.Middleware.Handlers;

public static class UrlClassificationExtension
{
    private static readonly ConcurrentDictionary<String,String> KnownHosts=new ConcurrentDictionary<String,String>(StringComparer.OrdinalIgnoreCase);
    //private static readonly HashSet<string> KnownHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private const String SharePointUrlRegex = @"https:\/\/.*sharepoint\.(com||us||cn||de).*";
    private const String SharePointUrlClassification = "SharePoint";
    private const String EmptyClassification = "Empty";
    static UrlClassificationExtension()
    {
        //Enum.GetValues<AzureEnvironment>().ForEach(env =>
        //{
        //    var endPoints = Endpoints.GetEndpoints(env);
        //    foreach (var property in typeof(Endpoints).GetProperties())
        //    {
        //        var value = Convert.ToString(property.GetValue(endPoints));

        //        if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Host.IsNotNullOrEmpty())
        //        {
        //            KnownHosts.TryAdd(uri.Host, property.Name);
        //        }
        //    }
        //});
    }

    public static String GetClassification(this HttpRequestMessage message)
    {
        return GetClassification(message?.RequestUri);
    }

    /// <summary>
    /// null or Empty means don't know what it is
    /// 00:00:00.0775472 time cost for 100k times call
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static String GetClassification(this Uri? uri)
    {
        if (uri == null)
        {
            return EmptyClassification;
        }

        if (KnownHosts.TryGetValue(uri.Host,out String classification))
        {
            return classification;
        }

        if (Regex.IsMatch(uri.ToString(), SharePointUrlRegex, RegexOptions.IgnoreCase))
        {
            return SharePointUrlClassification;
        }

        return String.Empty;
    }
}
