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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace AutoInstallationCommon.Utility
{
    public class CommonFeaturesWrapper
    {
        #region For Features

        private readonly string queryResultFile = Environment.GetEnvironmentVariable("Temp") +
                                                  "\\AvePointFeatures.xml";

        private readonly string queryResultFileWin7 = Environment.GetEnvironmentVariable("Temp") +
                                                      "\\AvePointFeaturesWin7.txt";

        public bool IsShowHasServerManagerMessageBox;

        /// <summary>
        ///     检查多个Feature是否安装
        /// </summary>
        /// <param name="featureIDList">需要检查的Feature Id list</param>
        /// <returns>未安装的Feature Id list</returns>
        public List<string> VerifyFeature(List<string> featureIDList)
        {
            var result = new List<string>();
            try
            {
                if (!File.Exists(queryResultFile))
                {
                    var tempFileInfo = new FileInfo(queryResultFile);
                    if (!Directory.Exists(tempFileInfo.Directory.FullName))
                        Directory.CreateDirectory(tempFileInfo.Directory.FullName);
                    StartServerManagerCMDProcess("-query " + queryResultFile);
                }

                if (File.Exists(queryResultFile))
                    foreach (var featureID in featureIDList)
                    {
                        if (ScanFeature("Feature", featureID)) continue;
                        if (ScanFeature("Role", featureID)) continue;
                        if (ScanFeature("RoleService", featureID)) continue;
                        result.Add(featureID);
                    }
                else
                    result.Add("FeatureNotExist");
            }
            finally
            {
                DeleteQueryResultFile();
            }

            return result;
        }

        /// <summary>
        ///     检查单个Feature或RoleService是否安装
        /// </summary>
        /// <param name="id">Feature Id或RoleService Id</param>
        /// <returns></returns>
        public bool VerifyFeature(string id)
        {
            try
            {
                if (StartServerManagerCMDProcess("-query " + queryResultFile))
                    if (File.Exists(queryResultFile))
                    {
                        if (ScanFeature("Feature", id)) return true;
                        if (ScanFeature("Role", id)) return true;
                        if (ScanFeature("RoleService", id)) return true;
                    }
            }
            finally
            {
                DeleteQueryResultFile();
            }

            return false;
        }

        /// <summary>
        ///     根据Id查询
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool ScanFeature(string type, string id)
        {
            var mDoc = new XmlDocument();
            mDoc.Load(queryResultFile);
            var elemList = mDoc.GetElementsByTagName(type);
            for (var i = 0; i < elemList.Count; i++)
            {
                var attributeCollection = elemList[i].Attributes;
                if (IsCollationEmpty(attributeCollection)) continue;
                if (IsTheSameID(id, attributeCollection))
                {
                    var attributes = elemList[i].Attributes;
                    if (attributes != null && attributes["Installed"] == null) continue;
                    var collection = elemList[i].Attributes;
                    return collection != null &&
                           collection["Installed"].Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        private static bool IsTheSameID(string id, XmlAttributeCollection attributeCollection)
        {
            return attributeCollection != null &&
                   attributeCollection["Id"].Value.Equals(id, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCollationEmpty(XmlAttributeCollection attributeCollection)
        {
            return attributeCollection != null && attributeCollection["Id"] == null;
        }

        /// <summary>
        ///     删除查询结果文件
        /// </summary>
        private void DeleteQueryResultFile()
        {
            if (File.Exists(queryResultFile)) File.Delete(queryResultFile);
        }

        /// <summary>
        ///     调用ServerManagerCmd.exe
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool StartServerManagerCMDProcess(string args)
        {
            var p = new Process();
            try
            {
                p.StartInfo.FileName = Environment.SystemDirectory + "\\ServerManagerCmd.exe";
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                return true;
            }
            finally
            {
                p.Dispose();
            }
        }

        private bool VerifyIfHasServerManagerProcesses()
        {
            var pros = Process.GetProcesses();
            foreach (var pro in pros)
                if (pro.ProcessName.Equals("MMC", StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        #endregion

        #region For Features (Win 7)

        /// <summary>
        ///     调用DISM.exe
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool StartDISMCMDProcess(string args)
        {
            var p = new Process();
            try
            {
                p.StartInfo.FileName = Environment.SystemDirectory + @"\dism.exe";
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
                using (var sw = new StreamWriter(queryResultFileWin7))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(output)) sw.Write(output);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                p.Dispose();
            }
        }

        /// <summary>
        ///     检查多个Feature是否安装
        /// </summary>
        /// <param name="featureNameList">需要检查的Feature Id list</param>
        /// <returns>未安装的Feature Id list</returns>
        public List<string> VerifyFeatureWin7(List<string> featureNameList)
        {
            var result = new List<string>();
            try
            {
                var callDISMresult = false;

                if (File.Exists(queryResultFileWin7)) File.Delete(queryResultFileWin7);
                if (!File.Exists(queryResultFileWin7))
                {
                    var tempFileInfo = new FileInfo(queryResultFileWin7);
                    if (!Directory.Exists(tempFileInfo.Directory.FullName))
                        Directory.CreateDirectory(tempFileInfo.Directory.FullName);
                    callDISMresult = StartDISMCMDProcess(@" /online /get-features /format:table /english");
                }

                if (callDISMresult && File.Exists(queryResultFileWin7))
                    foreach (var featureName in featureNameList)
                    {
                        if (ScanFeatureWin7(featureName)) continue;
                        result.Add(featureName);
                    }
                else
                    result.Add("FeatureNotExist");
            }
            finally
            {
                DeleteQueryResultFileWin7();
            }

            return result;
        }

        /// <summary>
        ///     根据Feature Name查询
        /// </summary>
        /// <param name="featureName"></param>
        /// <returns></returns>
        private bool ScanFeatureWin7(string featureName)
        {
            using (var sr = new StreamReader(queryResultFileWin7))
            {
                while (true)
                {
                    var templine = sr.ReadLine();
                    if (templine != null)
                    {
                        if (templine.Contains(featureName) && !templine.Contains("45"))
                        {
                            if (templine.Trim().ToLower(CultureInfo.CurrentCulture)
                                    .EndsWith("enabled ", StringComparison.OrdinalIgnoreCase) ||
                                templine.Trim().ToLower(CultureInfo.CurrentCulture)
                                    .EndsWith("enabled", StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (templine.Trim().ToLower(CultureInfo.CurrentCulture)
                                .EndsWith("disabled", StringComparison.OrdinalIgnoreCase)) return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        ///     删除查询结果文件Win7
        /// </summary>
        private void DeleteQueryResultFileWin7()
        {
            if (File.Exists(queryResultFileWin7)) File.Delete(queryResultFileWin7);
        }

        #endregion
    }
}