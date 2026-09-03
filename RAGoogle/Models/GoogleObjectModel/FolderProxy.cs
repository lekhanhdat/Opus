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
using RAGoogle.Services;
using static Google.Apis.Drive.v3.Data.Drive;

namespace RAGoogle.Models.GoogleObjectModel
{
    public class FolderProxy : FileProxy
    {
        public FolderProxy(Dictionary<string, object> properties) : base(properties)
        {
        }

        public FolderProxy(GoogleDriveService driveSerivce, Dictionary<string, object> properties = default) : base(driveSerivce, properties)
        {
            if (properties.IsNullOrEmpty())
            {
                //to do : init drive properties
            }
        }

        public string FolderColorRgb
        {
            get { return GetProperty<string>("FolderColorRgb"); }
        }
    }
}