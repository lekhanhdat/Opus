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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    [Flags]
    public enum MethodType
    {
        CSOM=2,
        Rest=4,
        HttpRequest=8,
        WebService=16
    }
    public enum ReadWrite
    {
        Read,
        Write
    }
    public enum MethodLevel
    {
        Tenant,
        Site,
        Web,
        App,
        List,
        Folder,
        Item,
        Navigation,
        Field,
        ContentType,
        Workflow,
        UserCustomAction,
    }
    /// <summary>
    /// methods need to replaced by CSOM API
    /// </summary>
    class ClientOMRequestAttribute:Attribute
    {
        public ReadWrite ReadWrite { get; set; }
        public MethodLevel Level { get; set; }
        public MethodType Type { get; set; }
        public ClientOMRequestAttribute(ReadWrite readWrite, MethodLevel level,MethodType api)
        {
            ReadWrite = readWrite;
            Level = level;
        }
    }
}
