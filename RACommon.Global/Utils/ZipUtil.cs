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




namespace AvePoint.RA.Common.Global.Util
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    #endregion

    /**
     *  Ionic zip default encoding is IBM- code page 437, which defined in zip specification,
     *  although the zip format support the entry name and comment in Unicode, but default the
     *  code page 437 does not support it, also, the windows shell zip utility, which known as
     *  a windows feature named "Compressed Folder", is not compatible for zip specification,
     *  that is, the windows Compressed Folder is not fully implemented by MicroSoft corporation.
     *  as this uri http://commons.apache.org/compress/zip.html describes:
     *
     *    Windows' "compressed folder" feature doesn't recognize any flag or extra field and
     *    creates archives using the platforms default encoding - and expects archives to be
     *    in that encoding when reading them.
     *
     *  wiki page: http://en.wikipedia.org/wiki/ZIP_(file_format) :
     *
     *  Versions of Microsoft Windows have included support for zip compression in Explorer
     *  since the Plus! pack was released for Windows 98.[41] Microsoft calls this feature
     *  "Compressed Folders". Not all zip features are supported by the Windows Compressed
     *  Folders capability. For example, AES Encryption, split or spanned archives, and Unicode
     *  entry encoding are not known to be readable or writable by the Compressed Folders
     *  feature in Windows XP or Windows Vista.
     *
     *  ZIP specification: http://www.pkware.com/documents/casestudies/APPNOTE.TXT
     */

    public class ZipUtil
    {
        public static void ZipFolder(
            String folderPath,
            String outputZipFile,
            Encoding encoding)
        {
            ZipFile.CreateFromDirectory(folderPath, outputZipFile, CompressionLevel.Optimal, false, encoding);
        }

    }
}