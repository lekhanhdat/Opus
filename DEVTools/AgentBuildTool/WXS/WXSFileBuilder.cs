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
using AgentBuildTool.Common;
using AgentBuildTool.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace AgentBuildTool.WXS
{
    public class WXSFileBuilder
    {
        private HashSet<string> IncludeFiles = new HashSet<string>();
        private HashSet<string> ExcludeFileNames = new HashSet<string>();
        private List<Regex> ExcludeFileNameRegexes = new List<Regex>();
        private HashSet<string> ExcludeFolders = new HashSet<string>();
        private HashSet<string> ThirdDlls = new HashSet<string>();
        private HashSet<string> ForceSignDlls = new HashSet<string>();
        private HashSet<string> ForceObfuscationDlls = new HashSet<string>();
        private List<WXSDirNode> allDirNodes = new List<WXSDirNode>();
        private List<WXSFileNode> allFileNodes = new List<WXSFileNode>();
        private HashSet<string> finalExcludeDirNodeIDs = new HashSet<string>();
        private string productCode = Guid.NewGuid().ToString().ToUpper();
        private WXSDirNode licDir = null;
        private WXSDirNode binDir = null;

        public void Build()
        {
            try
            {
                LoadIdentitiesFromOldWXSConfig();
                InitExcludeConfigs();
                InitBaseDirectories();
                RetrievalLicFiles();
                RetrievalAgentBin();

                GenerateWXSFile();
                GenerateSignAndPackageConfigFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Build WXS file failed: {ex}");
                Console.ReadLine();
            }
            Console.WriteLine($"Build WXS file completed.");
            Console.ReadLine();
        }

        private void InitBaseDirectories()
        {
            var cloudDir = CreateDirNode(new WXSDirNode(null, "$(var.Type)", "INSTALLFOLDER"), "Cloud");
            var agentDir = CreateDirNode(cloudDir, "Agent");
            binDir = CreateDirNode(agentDir, "bin");
            licDir = CreateDirNode(agentDir, "lic");
        }

        private void InitExcludeConfigs()
        {
            var excludeConfig = JsonConvert.DeserializeObject<WXSExcludeConfig>(File.ReadAllText("Config/WXSExcludeConfig.json"));
            foreach (var filename in excludeConfig.IncludeFiles)
            {
                IncludeFiles.Add(filename.ToLower());
            }
            foreach (var foldername in excludeConfig.ExcludeFolders)
            {
                ExcludeFolders.Add(foldername.ToLower());
            }
            foreach (var regexStr in excludeConfig.ExcludeFileNameRegexes)
            {
                ExcludeFileNameRegexes.Add(new Regex(regexStr, RegexOptions.IgnoreCase));
            }
            foreach (var filename in excludeConfig.ExcludeFileNames)
            {
                ExcludeFileNames.Add(filename.ToLower());
            }
            foreach (var dllName in excludeConfig.ThirdDlls)
            {
                ThirdDlls.Add(dllName.ToLower());
            }
            foreach (var dllName in excludeConfig.ForceSignDlls)
            {
                ForceSignDlls.Add(dllName.Trim());
            }
            foreach (var dllName in excludeConfig.ForceObfuscateDlls)
            {
                ForceObfuscationDlls.Add(dllName.Trim());
            }
        }

        private void RetrievalLicFiles()
        {
            var licFolder = new DirectoryInfo(CommonConfig.AGENT_LIC_PATH);
            if (!licFolder.Exists)
            {
                throw new Exception($"Agent lic folder not exists. Please check appSetting - agent_lic_path.");
            }
            foreach (var file in licFolder.GetFiles())
            {
                CreateFileNode(licDir, file.Name);
            }
        }

        private void RetrievalAgentBin()
        {
            var binFolder = new DirectoryInfo(CommonConfig.AGENT_BIN_OUTPUT_PATH);
            if(!binFolder.Exists)
            {
                throw new Exception($"Agent bin output path not exists. Please check appSetting - agentbin_output.");
            }
            foreach (var fileInfo in binFolder.GetFiles())
            {
                if (!IsExcludeFile(fileInfo, false))
                {
                    CreateFileNode(binDir, fileInfo.Name);
                }
            }
            foreach (var subFolder in binFolder.GetDirectories())
            {
                RetrievalDir(binDir, subFolder, false);
            }
        }

        /// <summary>
        /// return if exists WXS node
        /// </summary>
        private bool RetrievalDir(WXSDirNode parentNode, DirectoryInfo curFolder, bool inExcludeDir)
        {
            var isExcludeDir = inExcludeDir || IsExcludeDirectory(curFolder);
            bool hasFile = false;
            WXSDirNode curDirNode = CreateDirNode(parentNode, curFolder.Name);
            foreach (var fileInfo in curFolder.GetFiles())
            {
                if (!IsExcludeFile(fileInfo, isExcludeDir))
                {
                    CreateFileNode(curDirNode, fileInfo.Name);
                    hasFile = true;
                }
            }
            foreach (var subFolder in curFolder.GetDirectories())
            {
                hasFile = RetrievalDir(curDirNode, subFolder, isExcludeDir) || hasFile;
                
            }

            if(!hasFile)
            {
                finalExcludeDirNodeIDs.Add(curDirNode.Id);
            }
            return hasFile;
        }

        private bool IsExcludeDirectory(DirectoryInfo dirInfo)
        {
            var curDirPath = dirInfo.FullName.ToLower();
            foreach (var excludeDirName in ExcludeFolders)
            {
                if(curDirPath.EndsWith(excludeDirName))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIncludeFile(FileInfo fileInfo)
        {
            var curFilePath = fileInfo.FullName.ToLower();
            foreach (var fileName in IncludeFiles)
            {
                if (curFilePath.EndsWith(fileName))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsExcludeFile(FileInfo fileInfo, bool inExcludeDir)
        {
            if (IsIncludeFile(fileInfo))
            {
                return false;
            }
            else if (inExcludeDir)
            {
                return true;
            }

            var curFilePath = fileInfo.FullName.ToLower();
            foreach (var regex in ExcludeFileNameRegexes)
            {
                if (regex.IsMatch(fileInfo.FullName))
                {
                    return true;
                }
            }

            foreach (var excludeFileName in ExcludeFileNames)
            {
                if (curFilePath.EndsWith(excludeFileName))
                {
                    return true;
                }
            }

            return false;
        }

        private WXSDirNode CreateDirNode(WXSDirNode parentNode, string name)
        {
            var dirNode = new WXSDirNode(parentNode, name);
            allDirNodes.Add(dirNode);
            return dirNode;
        }

        private WXSFileNode CreateFileNode(WXSDirNode parentNode, string fileName)
        {
            var fileNode = new WXSFileNode(parentNode, fileName);
            allFileNodes.Add(fileNode);
            return fileNode;
        }

        private Dictionary<string, Tuple<string, string, string>> existingFileNodeIDs = new Dictionary<string, Tuple<string, string, string>>();
        private Dictionary<string, Tuple<string, string>> existingDirNodeIDs = new Dictionary<string, Tuple<string, string>>();
        private void LoadIdentitiesFromOldWXSConfig()
        {
            var wxsXml = new XmlDocument();
            wxsXml.Load(CommonConfig.AGENT_PACKAGE_WXS_PATH);
            var fileXmlNodes = wxsXml.DocumentElement.ChildNodes;
            var parentDirNodeIDs = new Dictionary<string, string>();
            var dirNodeNames = new Dictionary<string, string>();
            dirNodeNames["INSTALLFOLDER"] = "$(var.Type)";
            foreach (XmlNode xmlNode in fileXmlNodes)
            {
                switch (xmlNode.FirstChild?.Name)
                {
                    case "Package":
                        if(!CommonConfig.AGENT_MajorVersionBuild)
                        {
                            productCode = xmlNode.Attributes["Id"].Value;
                        }
                        break;
                    case "ComponentGroup":
                        if (xmlNode.FirstChild?.Attributes["Id"]?.Value == "FileComponents")
                        {
                            foreach (XmlNode fileNode in xmlNode.FirstChild.ChildNodes)
                            {
                                if(fileNode.Name != "Component")
                                {
                                    continue;
                                }
                                var componentId = fileNode.Attributes["Id"].Value;
                                var componentGuid = fileNode.Attributes["Guid"].Value;
                                var fileId = fileNode.FirstChild.Attributes["Id"].Value;
                                var fileKey = fileNode.FirstChild.Attributes["Source"].Value;
                                existingFileNodeIDs[fileKey] = Tuple.Create(fileId, componentId, componentGuid);
                            }
                        }
                        break;
                    case "DirectoryRef":
                        if (xmlNode.FirstChild?.FirstChild?.Name == "Directory")
                        {
                            var parentDirId = xmlNode.FirstChild.Attributes["Id"].Value;
                            var dirName = xmlNode.FirstChild.FirstChild.Attributes["Name"].Value;
                            var dirId = xmlNode.FirstChild.FirstChild.Attributes["Id"].Value;
                            parentDirNodeIDs[dirId] = parentDirId;
                            dirNodeNames[dirId] = dirName;
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (var item in dirNodeNames)
            {
                var dirPath = item.Value;
                var dirId = item.Key;
                string pDirId = null;
                if(!parentDirNodeIDs.TryGetValue(dirId, out pDirId))
                {
                    continue;
                }

                string tempDirParentId = pDirId;
                while (dirNodeNames.TryGetValue(tempDirParentId, out string pName))
                {
                    dirPath = $"{pName}\\{dirPath}";
                    if (!parentDirNodeIDs.TryGetValue(tempDirParentId, out tempDirParentId))
                    {
                        break;
                    }
                }

                existingDirNodeIDs[dirPath] = Tuple.Create(dirId, pDirId);
            }
        }

        private void GenerateWXSFile()
        {
            StringBuilder dirsContent = new StringBuilder();
            foreach (var dirNode in allDirNodes)
            {
                if (finalExcludeDirNodeIDs.Contains(dirNode.Id))
                {
                    continue;
                }
                if (existingDirNodeIDs.TryGetValue(dirNode.FullName, out var existingId))
                {
                    dirNode.Id = existingId.Item1;
                    dirNode.ParentNode.Id = existingId.Item2;
                }
                dirsContent.Append(dirNode.ToWXSString());
            }

            string configurationToolFileId = null;
            StringBuilder filesContent = new StringBuilder();
            foreach (var fileNode in allFileNodes)
            {
                if(existingFileNodeIDs.TryGetValue(fileNode.FullName, out var existingId))
                {
                    fileNode.Id = existingId.Item1;
                    fileNode.ComponentId = existingId.Item2;
                    fileNode.Guid = existingId.Item3;
                }

                filesContent.Append(fileNode.ToWXSString());
                if (fileNode.FileName.Equals(CommonConfig.AGENT_CONFIGURATION_TOOLNAME, StringComparison.OrdinalIgnoreCase))
                {
                    configurationToolFileId = fileNode.Id;
                }
            }

            StringBuilder allContent = new StringBuilder();
            allContent.AppendFormat(WXSFragmentTemplates.WXS_Fragment_FileComponents, filesContent.ToString());
            allContent.Append(dirsContent.ToString());

            File.WriteAllText(
                CommonConfig.AGENT_PACKAGE_WXS_PATH, 
                string.Format(
                    WXSFragmentTemplates.PACKAGE_AGENT_WXS_TEMPLATE
                    , allContent.ToString(),
                    configurationToolFileId,
                    binDir.Id,
                    productCode
                ));
        }

        private void GenerateSignAndPackageConfigFile()
        {
            HashSet<string> allOutputDLLsWithPath = new HashSet<string>();
            HashSet<string> allOutputDLLsFileName = new HashSet<string>();
            
            foreach (var node in allFileNodes)
            {
                var dllName = node.FileName.ToLower();
                if (dllName.EndsWith(".dll") && !ThirdDlls.Contains(dllName))
                {
                    // Get relative path from bin directory for signname.txt
                    string relativePath = GetRelativePathFromBin(node);
                    allOutputDLLsWithPath.Add(relativePath);
                    
                    // Get just filename for IncludeInPackage.xml
                    allOutputDLLsFileName.Add(node.FileName);
                }
            }

            var signDlls = new HashSet<string>(allOutputDLLsWithPath);
            foreach (var dllName in ForceSignDlls)
            {
                signDlls.Add(dllName);
            }
            
            // Sort by file name first, then by relative path
            var sortedSignDlls = signDlls.OrderBy(dll => 
            {
                var fileName = Path.GetFileName(dll);
                return fileName;
            }).ThenBy(dll => dll);
            
            File.WriteAllText(
                CommonConfig.AGENT_PACKAGE_DLLS_SIGNNAME_CONFIG, 
                string.Join("\r\n", sortedSignDlls));


            var obfuscationDlls = new HashSet<string>(allOutputDLLsFileName);
            foreach (var dllName in ForceObfuscationDlls)
            {
                obfuscationDlls.Add(dllName);
            }
            var packageDLLsXml = new StringBuilder();
            packageDLLsXml.AppendLine("<files>");
            
            // Sort by file name only for IncludeInPackage.xml
            var sortedObfuscationDlls = obfuscationDlls.OrderBy(dll => dll);
            
            foreach (var dllName in sortedObfuscationDlls)
            {
                // Exclude .resources.dll files from IncludeInPackage.xml only
                if (!dllName.ToLower().EndsWith(".resources.dll"))
                {
                    packageDLLsXml.AppendLine($"  <file name=\"{dllName}\" type=\"Obfuscation\" />");
                }
            }
            packageDLLsXml.Append("</files>");
            File.WriteAllText(CommonConfig.AGENT_PACKAGE_DLLS_OBFUSCATE_CONFIG, packageDLLsXml.ToString());
        }

        private string GetRelativePathFromBin(WXSFileNode node)
        {
            // Get the full path from the file node
            string fullPath = node.FullName;
            
            // Find the bin directory path in the full path
            string binPath = binDir.FullName;
            
            // If the file is directly in bin directory, return just the filename
            if (node.ParentNode == binDir)
            {
                return node.FileName;
            }
            
            // Otherwise, build the relative path from bin directory
            var currentDir = node.ParentNode;
            var pathParts = new List<string>();
            
            // Collect directory names until we reach binDir
            while (currentDir != null && currentDir != binDir)
            {
                pathParts.Add(currentDir.Name);
                currentDir = currentDir.ParentNode;
            }
            
            // If we didn't reach binDir, something is wrong, return just filename
            if (currentDir != binDir)
            {
                return node.FileName;
            }
            
            // Reverse the path parts and join with filename
            pathParts.Reverse();
            pathParts.Add(node.FileName);
            
            return string.Join("\\", pathParts);
        }

    }
}
