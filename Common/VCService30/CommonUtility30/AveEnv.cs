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





namespace AvePoint.Common
{
    #region using directives
    using GCommon;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    #endregion


    public class AveEnv
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static AveEnv()
        {
            Init();
        }

        #region -- public Static Properties --        
        public static string AgentName { get; set; }
        public static string AgentAddress { get; set; }
        public static string AgentRootFolder { get; set; }
        public static string AgentBinFolder { get; set; }
        public static string AgentJobFolder { get; set; }
        public static string AgentDataFolder { get; set; }
        public static string AgentDataPath { get; set; }
        public static string AgentTempFolder { get; set; }
        public static string AgentFarmId { get; set; }

        public static bool IsPublishing { get { return false; } }
        public static bool IsSharePoint2007 { get { return false; } }
        public static bool IsSharePoint2010 { get { return false; } }
        public static bool IsMoss { get { return false; } }
        public static bool IsWss { get { return false; } }

        #endregion

        #region -- Static Methods --

        private static void InitEnv()
        {
            try
            {
                //4. finally, reading current process location                
                if (string.IsNullOrEmpty(AgentRootFolder))
                {
                    AgentRootFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                    logger.Info("set agent root folder to: {0}", AgentRootFolder);                    
                }

                AgentDataPath = CombinePath(AgentRootFolder, "data");
                AgentTempFolder = CombinePath(AgentRootFolder, "Temp");
                AgentJobFolder = CombinePath(AgentRootFolder, "AgentData/jobs");
            }
            catch (Exception ex)
            {
                logger.Error("Init AveEnv failed:{0}", ex.ToString());
            }
        }

        private static string CombinePath(string parentFolder, string currentFolderName)
        {
            var path = Path.Combine(parentFolder, currentFolderName);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Create Directory:{0} failed:{1}", path, ex.ToString());
            }
            return path;
        }

        public static void Init()
        {
#if DEBUG
            while (File.Exists("C:\\debugAveEnv"))
            {
                Thread.Sleep(2000);
            }
#endif
            InitEnv();  //init all dir path
        }

        #endregion

    }

}