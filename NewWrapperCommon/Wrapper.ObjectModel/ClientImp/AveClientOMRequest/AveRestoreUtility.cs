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


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace AvePoint.ObjectModel.ClientOM
//{
//    class AveRestoreUtility
//    {
//    }
//}
public enum AveRestoreOption
{
    Default = 0x0001,
    OverWrite = 0x0002,
    Append = 0x0004,
    Replace = 0x000A,
    OverWriteByModifiedTime = 0x0012,
    AppendANewVersion = 0x0020,
    UpgradeOnly = 0x000D, //for app
    Restore = 0x000E,
    RestoreProperty = 0x0100,
    RestoreSecurity = 0x0200,
    ReplicatorDefault = 0x0302,
}

public class AveItemUIVersion
{
    public int MajorVersion;
    public int MinorVersion;

    public int UIVersion
    {
        get
        {
            return MajorVersion * 512 + MinorVersion;
        }
    }

    public string VersionLabel
    {
        get
        {
            return MajorVersion + "." + MinorVersion;
        }
    }

    public AveItemUIVersion(int uiversion)
    {
        MajorVersion = uiversion / 512;
        MinorVersion = uiversion % 512;
    }

    public AveItemUIVersion(string versionLabel)
    {
        MajorVersion = int.Parse(versionLabel.Split('.')[0]);
        MinorVersion = int.Parse(versionLabel.Split('.')[1]);
    }

    public override int GetHashCode()
    {
        return MajorVersion * 512 + MinorVersion;
    }
}

public enum AveConfictType
{
    None = 0,
    RecycleBin = 1,
    Document = 2,
    Both = 3
}