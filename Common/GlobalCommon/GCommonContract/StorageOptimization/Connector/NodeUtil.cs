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
using System.Text;
using System.Text.RegularExpressions;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector
{
    public class NodeUtil
    {
        public static Boolean CheckParentWasChecked(SPTreeNodeDto dto)
        {
            if (dto?.Parent == null)
            {
                return false;
            }
            if (dto.Parent.CheckNumber == 1)
            {
                return true;
            }
            return CheckParentWasChecked(dto.Parent);
        }
        public static Boolean CheckParentWasChecked(GoogleDriveTreeNodeDto dto)
        {
            if (dto?.Parent == null)
            {
                return false;
            }
            if (dto.Parent.CheckNumber == 1)
            {
                return true;
            }
            return CheckParentWasChecked(dto.Parent);
        }
        public static bool IsSiteCollectionInManagedPaths(SPTreeNodeDto scNode, SPTreeNodeDto webAppNode, List<string> managedPaths)
        {
            if (managedPaths == null || managedPaths.Count == 0)
            {
                return false;
            }
            if (scNode.Level != NodeLevel.SiteCollection)
            {
                throw new NotSupportedException();
            }
            string scUrl = scNode.FullPath;
            string webAppUrl = webAppNode.FullPath;
            string scRelative;
            string managedPath = GetManagedPath(scUrl, webAppUrl, webAppNode.NodeExtension.ManagedPathList, out scRelative);
            return managedPaths.Contains(managedPath);
        }

        public static PhysicalDeviceDto GetParentDevice(List<MapStoragePathDto> paths, SPTreeNodeDto node, SPTreeNodeDto inheritedNode)
        {
            if (node == null
                || node.Level == NodeLevel.Farm
                || node.Level == NodeLevel.WebApplication)
            {
                return null;
            }

            PhysicalDeviceDto parentPD;
            PhysicalDeviceDto physicalDevice;
            if (inheritedNode.Level != NodeLevel.WebApplication)
            {
                TryGetUnWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
            }
            else
            {
                TryGetWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
            }
            return parentPD;
        }

        public static bool TryGetPhysicalDevice(List<MapStoragePathDto> paths, SPTreeNodeDto node, SPTreeNodeDto inheritedNode, out PhysicalDeviceDto physicalDevice)
        {
            physicalDevice = null;
            if (node == null
                || node.Level == NodeLevel.Farm
                || node.Level == NodeLevel.WebApplication)
            {
                return false;
            }

            PhysicalDeviceDto parentPD;
            if (inheritedNode.Level != NodeLevel.WebApplication)
            {
                return TryGetUnWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
            }
            return TryGetWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
        }

        public static bool TryGetTwoPhysicalDevice(List<MapStoragePathDto> paths, SPTreeNodeDto node, SPTreeNodeDto inheritedNode, out PhysicalDeviceDto physicalDevice, out PhysicalDeviceDto parentPD)
        {
            physicalDevice = null;
            parentPD = null;
            if (node == null
                || node.Level == NodeLevel.Farm
                || node.Level == NodeLevel.WebApplication)
            {
                return false;
            }

            if (inheritedNode.Level != NodeLevel.WebApplication)
            {
                return TryGetUnWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
            }
            return TryGetWebAppPD(paths, node, inheritedNode, out physicalDevice, out parentPD);
        }

        private static bool TryGetUnWebAppPD(List<MapStoragePathDto> paths, SPTreeNodeDto node, SPTreeNodeDto inheritedNode, out PhysicalDeviceDto pd, out PhysicalDeviceDto parentPD)
        {
            pd = null;
            parentPD = null;
            if (paths == null || paths.Count == 0)
            {
                return false;
            }
            string nodeUrl = node.Url;
            string parentUrl = inheritedNode.Url;
            string relativePath = nodeUrl.Substring(parentUrl.Length);
            parentPD = paths[0].PhysicalDevice;
            if (string.IsNullOrEmpty(parentPD.ConnectionString))
            {
                return false;
            }
            //防止managedPath的特殊字符被转义时出错:
            parentPD.Path = GetPath(parentPD.ConnectionString);
            //parentPD = tem;
            pd = new PhysicalDeviceDto();
            pd.Path = CombinePath(parentPD.Path, relativePath);
            pd.ConnectionString = ConnectorStringAssembly(paths[0].PhysicalDevice.ConnectionString, parentPD.Path, pd.Path);
            return true;
        }

        public static string GetPath(string connectionString)
        {
            XRI xri = XRI.ValueOf(connectionString);
            string replaceString = string.Empty;
            if (xri.VIM.Equals(XRI.DocAve_NetApp, StringComparison.OrdinalIgnoreCase))
            {
                if (xri[XRI.DocAve_StartFolder] != null)
                {
                    replaceString = xri[XRI.DocAve_StartFolder];
                }
            }
            else
            {
                replaceString = xri[XRI.DovAve_Location];
            }
            return replaceString;
        }

        public static string CombinePath(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }
            char separator = '\\';

            path1 = path1.TrimEnd('\\', '/');
            path2 = path2.Trim('\\', '/');

            string newPath = (path1 + separator + path2).Replace('/', '\\');
            return newPath;
            //return newPath.Trim('\\');
        }

        public static string ConnectorStringAssembly(string connectorString, string path, string newPath)
        {
            XRI xri = XRI.ValueOf(connectorString);
            if (xri.VIM.Equals(XRI.DocAve_NetApp, StringComparison.OrdinalIgnoreCase))
            {
                xri[XRI.DocAve_StartFolder] = newPath;
            }
            else
            {
                xri[XRI.DovAve_Location] = newPath;
            }
            return xri.ToString();
        }

        public static string ConnectionStringReplace(string connectionString, string relativePath)
        {
            string oneSubstract = connectionString.Substring(connectionString.IndexOf("location", StringComparison.OrdinalIgnoreCase) + 9);
            int twoPosition = oneSubstract.IndexOf("&", StringComparison.OrdinalIgnoreCase);
            string replaceString = oneSubstract.Substring(0, twoPosition);
            string path = replaceString.TrimEnd('\\', '/') + '\\' + relativePath.Replace('/', '\\').TrimStart('\\', '/');
            string result = connectionString.Replace(replaceString, path);
            return result;
        }

        private static bool TryGetWebAppPD(List<MapStoragePathDto> paths, SPTreeNodeDto node, SPTreeNodeDto inheritedNode, out PhysicalDeviceDto pd, out PhysicalDeviceDto parentPD)
        {
            pd = null;
            parentPD = null;
            string nodeUrl = node.Url;
            string scUrl = string.Empty;
            string webAppUrl = string.Empty;
            while (node.Level != NodeLevel.Farm)
            {
                if (node.Level == NodeLevel.SiteCollection)
                {
                    scUrl = node.FullPath;
                }
                else if (node.Level == NodeLevel.WebApplication)
                {
                    webAppUrl = node.FullPath;
                }
                node = node.Parent;
            }
            PhysicalDeviceDto tem = null;
            string scRelative;
            string managedPath = GetManagedPath(scUrl,
                webAppUrl,
                inheritedNode.NodeExtension.ManagedPathList,
                out scRelative);

            if (string.IsNullOrEmpty(scRelative))
            {
                return false;
            }

            foreach (MapStoragePathDto mapPath in paths)
            {
                if (string.Compare(mapPath.ManagedPath.Trim('\\', '/'),
                    managedPath,
                    StringComparison.Ordinal) == 0)
                {

                    tem = mapPath.PhysicalDevice;
                    break;
                }
            }

            if (tem == null)
            {
                return false;
            }
            //防止managedPath的特殊字符被转义时出错:
            tem.Path = GetPath(tem.ConnectionString);

            parentPD = tem;
            string extend = nodeUrl.Substring(nodeUrl.LastIndexOf(managedPath) + managedPath.Length);
            pd = new PhysicalDeviceDto();

            string path = CombinePath(tem.Path, extend);
            pd.ConnectionString = ConnectorStringAssembly(tem.ConnectionString, tem.Path, path);
            pd.Path = path;
            return true;
        }

        public static string GetManagedPath(string scUrl, string webAppUrl, List<ManagedPathDto> managedPaths, out string scRelative)
        {
            List<ManagedPathDto> copied = new List<ManagedPathDto>(managedPaths);
            copied.Sort(Comparison);
            char[] trim = new char[] { '\\', '/' };
            scRelative = null;
            if (scUrl == null || webAppUrl == null)
            {
                throw new ArgumentNullException();
            }
            int index = scUrl.IndexOf("://", StringComparison.OrdinalIgnoreCase);
            index = index + 3;//当时HTTP是应该是7，如是HTTPS时应该是8
            int count = scUrl.IndexOf('/', index);//为了截取http://.../中第三个“/”以后的字符串内容
            if (count == -1)
            {
                scRelative = string.Empty;
                return string.Empty;
            }
            string subStr = scUrl.Substring(count).Trim(trim);

            if (string.Compare(subStr, string.Empty, StringComparison.Ordinal) == 0)
            {
                return string.Empty;
            }
            foreach (ManagedPathDto path in copied)
            {
                if (Match(path, subStr))
                {
                    string managedPath = path.Name.Trim(trim);
                    scRelative = subStr.Substring(managedPath.Length).TrimStart(trim);
                    return managedPath;
                }
            }
            //throw new InvalidOperationException(string.Format("No managed path matched for url {0}", scUrl));
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subStr">site collection 的Url 相对于WebApp的url多出的部分</param>
        /// <returns></returns>
        private static bool Match(ManagedPathDto path, string subStr)
        {
            if (path.Type == ManagedPathType.Explicit
                || path.Type == ManagedPathType.ExplicitInclusion)
            {
                return string.Compare(path.Name, subStr, StringComparison.Ordinal) == 0;
            }
            int index = subStr.IndexOf(path.Name, StringComparison.Ordinal);
            if (index != 0)
            {
                return false;
            }
            char seperator = subStr[path.Name.Length];
            return seperator == '/' || seperator == '\\';
        }

        private static int Comparison(ManagedPathDto path1, ManagedPathDto path2)
        {
            switch (path1.Type)
            {
                case ManagedPathType.Explicit:
                case ManagedPathType.ExplicitInclusion:
                    switch (path2.Type)
                    {
                        case ManagedPathType.Explicit:
                        case ManagedPathType.ExplicitInclusion:
                            return path2.Name.Length - path1.Name.Length;
                        case ManagedPathType.Wildcard:
                        case ManagedPathType.WildcardInclusion:
                            return -1;
                        default:
                            return 0;
                    }
                case ManagedPathType.Wildcard:
                case ManagedPathType.WildcardInclusion:
                    switch (path2.Type)
                    {
                        case ManagedPathType.Explicit:
                        case ManagedPathType.ExplicitInclusion:
                            return 1;
                        case ManagedPathType.Wildcard:
                        case ManagedPathType.WildcardInclusion:
                            return path2.Name.Length - path1.Name.Length;
                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }

        public static string GetFullPath(SPTreeNodeDto node)
        {
            if (node.Level == NodeLevel.WebApplication || node.Level == NodeLevel.SiteCollection)
            {
                return node.FullPath;
            }
            return node.Url;
        }

        public static bool IsRootSiteCollection(SPTreeNodeDto node)
        {
            return (node.Level == NodeLevel.SiteCollection &&
                    node.Parent != null &&

                    node.FullPath.TrimEnd('/').Equals(node.Parent.FullPath.TrimEnd('/'))

                   );
        }

        public static string GetNodeId(SPTreeNodeDto node)
        {
            StringBuilder sb = new StringBuilder().Append(node.FarmID);
            switch (node.Level)
            {
                case NodeLevel.WebApplication:
                case NodeLevel.SiteCollection:
                case NodeLevel.Site:
                    sb.Append(node.FullPath);
                    break;
                case NodeLevel.List:
                    //sb.Append(node.Url);
                    //break;
                    return node.SPObjectId;
                default:
                    sb.Append(node.FullPath);
                    break;
            }
            sb.Append("[").Append(node.Level).Append("]");
            return sb.ToString();
        }

        public static string StringTruncation(SPTreeNodeDto node, SPTreeNodeDto inheritNode, List<MapStoragePathDto> mapStoragePathDtos)
        {
            PhysicalDeviceDto pd;
            PhysicalDeviceDto physicalDevice;
            TryGetTwoPhysicalDevice(mapStoragePathDtos, node, inheritNode, out physicalDevice, out pd);
            string minuend = physicalDevice.Path;
            string subtractor = pd.Path;
            minuend = minuend.TrimEnd('\\', '/');
            subtractor = subtractor.TrimEnd('\\', '/');
            int length = subtractor.Length;
            string result = minuend.Substring(length);
            return result;
        }

        protected class XRI
        {
            #region --constant--

            //docave-xam://<vim name>!<connection string>[?<name>=<value>[&<name>=<value>] ... ]
            //patten - docave-xam://()!()

            /** SNIA XAM XRI prefix */
            public static readonly string SNIA_PREFIX = "SNIA-XAM://".ToLower();

            public static readonly string DocAve_PREFIX = "DOCAVE-XAM://".ToLower();

            public static readonly string DocAve_NetApp = "netapp_cifs_vim";

            public static readonly string DocAve_StartFolder = "startfolder";

            public static readonly string DovAve_Location = "location";

            public static readonly string PREFIX = "[(" + SNIA_PREFIX + ")|(" + DocAve_PREFIX + ")]";

            protected static readonly string PARAM_PATTERN = "[\\&]{0,1}([^=^&]+)\\=([^=^&]*)";

            // Match any set of characters, EXCEPT for the separator "!"
            protected static readonly string VIM_PATTERN = "(?:([^/\\?]+)\\?)";

            // Match any set of characters, EXCEPT for the separator "!" and "?"
            //protected const string SYSTEM_PATTERN = "([^?^!]*){1}";

            protected static readonly string XRI_PARAM_PATTERN = "([^=^&]+=[^=^&]*(?:\\&[^=^&]+\\=[^=^&]*)*)?";

            //protected const string XRI_PATTERN = PREFIX + VIM_PATTERN + SYSTEM_PATTERN + XRI_PARAM_PATTERN;
            protected static readonly string XRI_PATTERN = PREFIX + VIM_PATTERN + XRI_PARAM_PATTERN;

            protected static readonly Regex s_xri = new Regex(XRI_PATTERN);

            protected static readonly Regex s_params = new Regex(PARAM_PATTERN);

            #endregion

            #region --XRI components--
            private string protrocol = DocAve_PREFIX;
            private string vim;
            private Dictionary<string, string> parameters = new Dictionary<string, string>();
            public string Protocal
            {
                get { return this.protrocol; }
                set { this.protrocol = value; }
            }
            public string VIM
            {
                get { return this.vim; }
                set { this.vim = value; }
            }
            public Dictionary<string, string> Params
            {
                get { return this.parameters; }
            }
            #endregion

            #region -- Convert methods--
            /**
         * Creates an XRI instance from an XRI string. If the string does not contain
         * a valid XRI an {@link InvalidXRIException} is thrown.
         * 
         * @param xriString The XRI string
         * @return An XRI instance
         * @throws InvalidXRIException If the string does not contain a valid XRI
         */
            public static XRI ValueOf(string xriString)
            {
                Match match = s_xri.Match(xriString, 0, xriString.Length);
                if (!match.Success)
                {
                    throw new Exception(xriString);
                }
                XRI xri = new XRI();
                xri.Protocal = xriString.StartsWith(XRI.DocAve_PREFIX, StringComparison.OrdinalIgnoreCase) ? XRI.DocAve_PREFIX : XRI.SNIA_PREFIX;
                xri.VIM = match.Groups[1].Value;
                string parameters = match.Groups[2].Value;
                if (parameters != null)
                {
                    if (s_params.IsMatch(parameters))
                    {
                        match = s_params.Match(parameters, 0, parameters.Length);
                        while (match.Success)
                        {
                            string key = match.Groups[1].Value.ToLower();
                            string value = match.Groups[2].Value.Trim(new char[] { ' ' });
                            xri.Params.Add(key, ValueDecode(value));
                            match = match.NextMatch();
                        }
                    }
                }
                return xri;
            }

            public static string ValueEncode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
            }

            public static string ValueDecode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%").Replace("%5e", "^");
            }


            /**
             * Converts the XRI into a properly formatted string.
             * 
             * @return The XRI as a string.
             */
            public override string ToString()
            {
                StringBuilder buf = new StringBuilder(80);
                buf.Append(protrocol);
                buf.Append(vim);
                bool first = true;

                foreach (KeyValuePair<string, string> keyVal in parameters)
                {
                    if (string.IsNullOrEmpty(keyVal.Value))
                    {
                        continue;
                    }
                    if (first)
                    {
                        buf.Append('?');
                        first = false;
                    }
                    else
                    {
                        buf.Append('&');
                    }
                    string name = keyVal.Key;

                    buf.Append(name);
                    buf.Append('=');
                    string value = ValueEncode(keyVal.Value);
                    buf.Append(value);
                }
                return buf.ToString();
            }
            #endregion

            #region for binding data to gui
            public string this[string key]
            {
                get
                {
                    if (!parameters.ContainsKey(key))
                    {
                        return null;
                    }
                    return parameters[key];
                }
                set
                {
                    parameters[key] = value;
                }
            }
            #endregion
        }
    }
}
