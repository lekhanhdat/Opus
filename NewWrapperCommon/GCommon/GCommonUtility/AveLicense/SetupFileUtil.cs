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
using AvePoint.GCommon.Contract.AveLicense.Detail;
using System.Xml;
using System.IO;
using Microsoft.Win32;

namespace AvePoint.GCommon.Utility.AveLicense
{
    public class SetupFileUtil
    {
        private string filePath
        {
            get
            {
                string path = string.Empty;
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\AvePoint\DocAve6"))
                    {
                        if (key != null)
                        {
                            var value = key.GetValue("InstallPath");
                            path = value == null ? string.Empty : value.ToString();
                        }
                    }
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new Exception("can not get install path in registry.");
                    }
                    path = path + @"\Control\Config.ini";
                }
                catch (Exception e)
                {
                    throw new Exception("failed to get install path in registry.", e);
                }
                return path;
            }
        }
        private string rootNode { get { return "Configuration"; } }
        private string ceipNode { get { return "Backup"; } }
        private string soNode { get { return "SOnTrial"; } }
        private string docaveNode { get { return "DOnTrial"; } }

        /// <summary>
        /// 读取Config.ini文件的信息
        /// </summary>
        /// <returns></returns>
        public SetupConfig ReadInstallConfig()
        {
            SetupConfig config = new SetupConfig();
            if (!File.Exists(filePath))
            {
                return config;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                foreach (XmlLinkedNode node in doc.ChildNodes)
                {
                    if (string.Equals(node.Name, rootNode))
                    {
                        foreach (XmlElement sub in node.ChildNodes)
                        {
                            if (string.Equals(sub.Name, ceipNode))
                            {
                                bool isRegisterDocAve;
                                bool.TryParse(sub.InnerText, out isRegisterDocAve);
                                config.IsRegisterDocAve = isRegisterDocAve;
                            }
                            if (string.Equals(sub.Name, soNode))
                            {
                                bool isSoOnTrial;
                                bool.TryParse(sub.InnerText, out isSoOnTrial);
                                config.IsSoOnTrial = isSoOnTrial;
                            }
                            if (string.Equals(sub.Name, docaveNode))
                            {
                                bool isDocAveOnTrial;
                                bool.TryParse(sub.InnerText, out isDocAveOnTrial);
                                config.IsDocAveOnTrial = isDocAveOnTrial;
                            }
                        }
                    }
                }
                return config;
            }
        }

        /// <summary>
        /// 将安装过程中的用户设置保存在Config.ini中
        /// </summary>
        /// <param name="dto"></param>
        public void WriteInstallConfig(SetupConfig dto)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
                XmlElement root = doc.CreateElement(rootNode);
                XmlElement node1 = doc.CreateElement(ceipNode);
                node1.InnerText = dto.IsRegisterDocAve.ToString();
                XmlElement node2 = doc.CreateElement(soNode);
                node2.InnerText = dto.IsSoOnTrial.ToString();
                XmlElement node3 = doc.CreateElement(docaveNode);
                node3.InnerText = dto.IsDocAveOnTrial.ToString();
                root.AppendChild(node1);
                root.AppendChild(node2);
                root.AppendChild(node3);
                doc.AppendChild(root);
                doc.Save(filePath);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("failed to create install config file at {0}", filePath), e);
            }
        }
    }
}
