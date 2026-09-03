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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPBackup
{
    public interface ISPFileExport : ISPItemExport
    {
        /// <summary>
        /// Export Content，默认不备份GhostedPage的Stream，但是O365需要，所以提供两个选项，供外围使用。
        /// </summary>
        /// <param name="stream"></param>
        void ExportContent(IAveBackupStream stream);

        /// <summary>
        /// Export Content
        /// 
        /// forceBackup是给O365使用的，如果源端是local，但是文件是Ghosted Page，那么就需要备份对应的流
        /// </summary>
        /// <param name="output"></param>
        /// <param name="forceBackup"></param>
        void ExportContent(IAveBackupStream stream, bool forceBackup);
    }
}
