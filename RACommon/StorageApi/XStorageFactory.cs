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

namespace Microsoft365Backup.CommonUtil.StorageApi;

using System;
using System.Collections.Generic;
using System.IO;

using Storage;
using Storage.Cloud.Azure;
using Storage.FS;
using Storage.Util;

public static class XStorageFactory
{
    private const int AzureSingleBlockThreshold = 300;
    private const int AmazonSingleBlockThreshold = 300;
    public static IXSystem InstanceSystem(string connectionString)
    {
        return XFactory.InstanceSystem(connectionString).Configure();
    }

    private static IXSystem Configure(this IXSystem system)
    {
        system.SmallFileLength = system.StorageType switch
        {
            XStorageType.Azure => AzureSingleBlockThreshold,
            XStorageType.Amazon => AmazonSingleBlockThreshold,
            _ => system.SmallFileLength
        };
        return system;

    }

    public static XLibrary InstanceLibrary(List<string> connectionStrings)
    {
        var library = XFactory.InstanceLibrary(connectionStrings);
        foreach (var subSystem in library.SubSystems)
        {
            subSystem.Configure();
        }
        return library;
    }

    /// <summary>
    /// IFSSystem
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IXSystem CreateFileSystem(String path)
    {
        var xri= CreateLocalSystemConnectionBuilder(path).ToString();
        var system = InstanceSystem(xri);
        system.Open();
        return system;
    }

    private static ConnectionBuilder CreateLocalSystemConnectionBuilder(String path)
    {
        ConnectionBuilder builder = new() { StorageName = StorageName.FS };
        builder.Params.Add(XRIParameterKeys.LocationKey, path);
        builder.Params.Add(XRIParameterKeys.USERNAME_KEY, String.Empty);
        builder.Params.Add(XRIParameterKeys.PASSWORD_KEY, String.Empty);
        builder.Params.Add(XRIParameterKeys.CREATE_IF_NOT_EXISTS, "true");
        return builder;
    }

}