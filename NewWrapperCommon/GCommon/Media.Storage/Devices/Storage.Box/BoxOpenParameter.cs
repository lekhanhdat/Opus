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
using System.Text;

namespace AvePoint.Media.Storage.Box
{
    class BoxOpenParameter
    {
        public String AccessToken { get; set; }
        public String ClientId { get; set; }
        public String EmailAddress { get; set; }
        public String ClientSecret { get; set; }
        public String RootFolderId { get; set; }
        public String RefreshToken { get; set; }
        public String RootFolderName { get; set; }
        public String ConfigLocation { get; set; }
        public String ConfigUsername { get; set; }
        public String ConfigPassword { get; set; }
        public String ManagerUserName { get; set; }
        public String ManagerUserId { get; set; }
        public Boolean IsValidate { get; set; }
        public String Proxy { get; set; }
    }

    //class AccessToken
    //{
    //    private DateTime createTime;
    //    private Boolean isTokenDisable;
    //    private String accessToken;
    //    public AccessToken(DateTime createTime, String accessToken)
    //    {
    //        this.createTime = createTime;
    //        this.accessToken = accessToken;
    //    }

    //    internal String Token
    //    {
    //        get
    //        {
    //            return accessToken;
    //        }
    //        set
    //        {
    //            accessToken = value;
    //        }
    //    }
    //    internal Boolean IsTokenDisable
    //    {
    //        get
    //        {
    //            if ((DateTime.Now - createTime).TotalMinutes > 55)
    //            {
    //                isTokenDisable = true;
    //            }
    //            else
    //            {
    //                isTokenDisable = false;
    //            }
    //            return isTokenDisable;
    //        }
    //    }
    //}
}
