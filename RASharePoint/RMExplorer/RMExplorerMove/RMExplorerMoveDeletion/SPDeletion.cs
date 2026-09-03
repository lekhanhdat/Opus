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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPDeletion
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SPDeletion));
        public static void DeleteSPFile(IAveFile file)
        {
            if (file != null && file.Exists)
            {
                logger.Info("File exists in SharePoint, file id: {0}.", file.UniqueId);
                try
                {
                    //Local&365:当前user 自动check out/Check Out的文件，可以调用delete方法直接删除，不会抛错。
                    file.Delete();
                    logger.Debug("Delete file successful. File id: {0}.", file.UniqueId);
                }
                catch (Exception e)
                {
                    logger.Info(string.Format("Cannot delete current file, id: {0}, message: {1}.", file.Name, e.ToString()));
                    throw new Exception("RM_JM_Archive_UnableDeleteSourceFile_ErrorMessage");
                }
            }
            else
            {
                //进入这个判断有两种可能，1.当前文件是check out file. 2.当前文件在目的端不存在.
                logger.Info("File is not exists, it may be an auto-check out file, file id: {0}.", file?.UniqueId);
                //DeleteDesAutoCheckOutFile(desFile);
            }
        }
    }
}
