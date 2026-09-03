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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion
    class TSMConfiguration
    {
        TSMClient nodeClient;
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TSMConfiguration(TSMClient mNodeClient)
        {
            this.nodeClient = mNodeClient;
        }

        public TSMConfiguration()
        {
        }

        #region used for media setUp

        public string CheckCommOpt(TSMNodeInfo nodeInfo)
        {
            string text = null;
            CreateUnexistsFodle(nodeInfo);
            FileInfo configFile = new FileInfo(nodeInfo.CommConfigFile);
            if (configFile.Exists)
            {
                text = File.ReadAllText(configFile.FullName, Encoding.UTF8);
            }
            else
            {
                using (FileStream fs = new FileStream(configFile.FullName, FileMode.Create))
                {
                    string content = GetCommOptContent();
                    byte[] dsmByte = Encoding.UTF8.GetBytes(content);
                    fs.Write(dsmByte, 0, dsmByte.Length);
                };
                text = File.ReadAllText(configFile.FullName, Encoding.UTF8);
            }
            return text;
        }

        public static string GetCommOptContent()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("PASSWORDACCESS".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append("GENERATE".ToLower(CultureInfo.InvariantCulture));
            buffer.Append("\r\n");

            buffer.Append("TCPBUFFSIZE".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append(65);
            buffer.Append("\r\n");

            buffer.Append("TCPNODELAY".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append("yes");
            buffer.Append("\r\n");

            //buffer.Append("ENCRYPTIONTYPE");
            //buffer.Append(" ");
            //buffer.Append("DES56");
            //buffer.Append("\r\n");

            buffer.Append("\r\n");

            buffer.Append("*DocAve.FileSpace=DocAve");
            buffer.Append("\r\n");
            buffer.Append("*DocAve.FileSpace.Capacity=1");
            buffer.Append("\r\n");
            buffer.Append("*DocAve.FileSpace.Occupancy=0");
            buffer.Append("\r\n");
            buffer.Append("*TraceFile".ToLower(CultureInfo.InvariantCulture) + TSMConst.tsmResourceRoot + "/logs/AvePoint-DocAveMedia-Trace.log");

            buffer.Append("\r\n");
            return buffer.ToString();
        }

        private void CreateUnexistsFodle(TSMNodeInfo nodeInfo)
        {
            if (!Directory.Exists(nodeInfo.CommDsmiDir))
            {
                Directory.CreateDirectory(nodeInfo.CommDsmiDir);
            }
            if (!Directory.Exists(nodeInfo.CommDsmiLogDir))
            {
                Directory.CreateDirectory(nodeInfo.CommDsmiLogDir);
            }
            if (!Directory.Exists(nodeInfo.CommConfigFileDir))
            {
                Directory.CreateDirectory(nodeInfo.CommConfigFileDir);
            }
        }

        public string CheckNodeOpt(TSMNodeInfo nodeInfo)
        {
            if (!Directory.Exists(nodeInfo.ConfigFileDir))
            {
                Directory.CreateDirectory(nodeInfo.ConfigFileDir);
            }
            FileInfo configFile = new FileInfo(nodeInfo.ConfigFile);
            var content = GenerateNodeOptContent(nodeInfo);
            if (configFile.Exists)
            {
                var existContent = File.ReadAllText(configFile.FullName, Encoding.UTF8);
                if (!existContent.Equals(content, StringComparison.OrdinalIgnoreCase))
                {
                    using (var fs = new FileStream(configFile.FullName, FileMode.Truncate))
                    {
                        byte[] dsmByte = Encoding.UTF8.GetBytes(content);
                        fs.Write(dsmByte, 0, dsmByte.Length);
                    }
                }
            }
            else
            {
                using (var fs = new FileStream(configFile.FullName, FileMode.Create))
                {
                    byte[] dsmByte = Encoding.UTF8.GetBytes(content);
                    fs.Write(dsmByte, 0, dsmByte.Length);
                }
            }
            return content;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Lanfree")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Commmethod")]
        public string GenerateNodeOptContent(TSMNodeInfo nodeInfo)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("COMMMETHOD".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append(nodeInfo.CommunicationMethod);
            buffer.Append("\r\n");

            buffer.Append("TCPPORT".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append(nodeInfo.Port);
            buffer.Append("\r\n");

            buffer.Append("TCPSERVERADDRESS".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append(nodeInfo.TcpServerAddress);
            buffer.Append("\r\n");

            buffer.Append("NODENAME".ToLower(CultureInfo.InvariantCulture));
            buffer.Append(" ");
            buffer.Append(nodeInfo.Nodename);
            buffer.Append("\r\n");

            if (nodeInfo.EnableNodeProxy)
            {
                buffer.Append("asnodename".ToLower(CultureInfo.InvariantCulture));
                buffer.Append(" ");
                buffer.Append(nodeInfo.Asnodename);
                buffer.Append("\r\n");
            }

            if (nodeInfo.EnableLanfree)
            {
                buffer.Append("ENABLELANFREE".ToLower(CultureInfo.InvariantCulture));
                buffer.Append(" ");
                buffer.Append(nodeInfo.EnableLanfree);
                buffer.Append("\r\n");

                buffer.Append("LANFREETCPPORT".ToLower(CultureInfo.InvariantCulture));
                buffer.Append(" ");
                buffer.Append(nodeInfo.Lanfreetcpport);
                buffer.Append("\r\n");

                buffer.Append("LANFREETCPSERVERADDRESS".ToLower(CultureInfo.InvariantCulture));
                buffer.Append(" ");
                buffer.Append(nodeInfo.LanfreeTcpServerAddress);
                buffer.Append("\r\n");

                buffer.Append("LanfreeCommmethod".ToLower(CultureInfo.InvariantCulture));
                buffer.Append(" ");
                buffer.Append(nodeInfo.LanfreeCommmethod);
                buffer.Append("\r\n");
            }

            string mc = nodeInfo.IncludeMC;
            if (string.IsNullOrEmpty(mc) || mc.Trim().ToLower(CultureInfo.InvariantCulture).Equals("default"))
            {
                // default managementClass
                buffer.Append("\r\n");
            }
            else
            {
                buffer.Append("Include * " + mc.ToUpper(CultureInfo.InvariantCulture) + "\r\n");
            }
            return buffer.ToString();
        }

        public void CleanUpNoUseData(TSMNodeInfo mNodeInfo)
        {
            DirectoryInfo info = new DirectoryInfo(mNodeInfo.CommConfigFileDir);
            DirectoryInfo[] subFolders = info.GetDirectories();
            foreach (DirectoryInfo subFolder in subFolders)
            {
                subFolder.Delete(true);
            }
        }

        public void CleanUpValidateData(TSMNodeInfo mNodeInfo)
        {
            DirectoryInfo info = new DirectoryInfo(mNodeInfo.ConfigFileDir);
            if (info.Exists)
            {
                info.Delete(true);
            }
        }

        #endregion
    }
}
