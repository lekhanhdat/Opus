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
// *
// *  PROPRIETARY and CONFIDENTIAL
// *
// *  This file is licensed from, and is a trade secret of:
// *
// *                   AvePoint, Inc.
// *                   525 Washington Blvd, Suite 1400
// *                   Jersey City, NJ 07310
// *                   United States of America
// *                   Telephone: +1-201-793-1111
// *                   WWW: www.avepoint.com
// *
// *  Refer to your License Agreement for restrictions on use,
// *  duplication, or disclosure.
// *
// *  RESTRICTED RIGHTS LEGEND
// *
// *  Use, duplication, or disclosure by the Government is
// *  subject to restrictions as set forth in subdivision
// *  (c)(1)(ii) of the Rights in Technical Data and Computer
// *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
// *  FAR 52.227-19 (C) (June 1987).
// *
// *  Copyright © 2023 AvePoint® Inc. All Rights Reserved. 
// *
// *  Unpublished - All rights reserved under the copyright laws of the United States.
// */

using AvePoint.GCommon;
using System.Reflection;
///********************************************************************
public class ExecutorContext
{
    private static AveLogger logger = AveLogger.GetInstance(typeof(ExecutorContext));
    public static string BinDirectory
    {
        get
        {
            try
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception ex)
            {
                logger.Warn("get bin dir failed, we will use " + AppDomain.CurrentDomain.BaseDirectory + ex.Message);
                string assmblyPath = Assembly.GetExecutingAssembly().Location;
                string dllName = Assembly.GetExecutingAssembly().ManifestModule.Name;
                assmblyPath = assmblyPath.TrimEnd(dllName.ToCharArray());
                return assmblyPath;
            }
        }
    }
}

//    /// <summary>
//    /// 这个enum是为了区分不同功能添加的，某些方法有时候需要根据功能进行一些特殊处理。
//    /// </summary>
public enum ModuleType
{
    MediaService = 0,
    Connector = 1,
}