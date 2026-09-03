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

namespace AvePoint.ObjectModel.Server19.NonPublicAPI
{
    using Wrapper.Common;
    using Microsoft.SharePoint;
    using System;
    using System.IO;
    using System.Collections;
    using GCommon;
    internal static class SPFileCollectionExtension
    {
        public static SPFile AddExtension(this SPFileCollection files, string urlOrFile, Stream fileStream, bool overwrite)
        {
            if (fileStream.Length < 2047 * 1024 * 1024)
            {
                return files.Add(SPResourcePath.FromDecodedUrl(urlOrFile), fileStream, new SPFileCollectionAddParameters { Overwrite = overwrite });
            }
            else
            {
                var file = files.Add(SPResourcePath.FromDecodedUrl(urlOrFile), new byte[] { }, new SPFileCollectionAddParameters { Overwrite = overwrite });
                FileUploader.SaveBinaryInternal(file, fileStream);
                return file;
            }
        }

        public static SPFile AddExtension(this SPFileCollection files, string urlOrFile, Stream fileStream, Hashtable properties, bool overwrite)
        {
            if (fileStream.Length < 2047 * 1024 * 1024)
            {
                return files.Add(SPResourcePath.FromDecodedUrl(urlOrFile), fileStream, new SPFileCollectionAddParameters {Properties= properties, Overwrite = overwrite });
            }
            else
            {
                var file = files.Add(SPResourcePath.FromDecodedUrl(urlOrFile), new byte[] { }, new SPFileCollectionAddParameters { Properties = properties, Overwrite = overwrite });
                FileUploader.SaveBinaryInternal(file, fileStream);
                return file;
            }
        }
    }
}
