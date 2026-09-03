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

namespace AvePoint.Media.ClassicStorage.Box
{
    public class BoxOpenParameter
    {
        public string ClientId { get; set; }
        public String ClientSecret { get; set; }
        public String RefreshToken { get; set; }
        public String AccessToken { get; set; }
        public int BlockLength { get; set; }
        public int EachBlockLength { get; set; }
        public string RootFolderId { get; set; }
        public string RootFolderName { get; set; }
        public String EmailAddress { get; set; }
        public bool IsValidate { get; set; }

        public BoxOpenParameter()
        {
            this.BlockLength = 1024 * 1024 * 1024;
            this.EachBlockLength = 1024 * 1024 * 1024;
        }
    }
}
