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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
namespace AvePoint.ObjectModel.Common
{
    class AveSolutionPackage : AveClientObject, IAveSolutionPackage
    {
        XmlDocument _ManifestDoc;
        XmlNamespaceManager _NamespaceManager;
        public AveSolutionPackage(string path, string name)
        {
            base.DataCache.AddProperty("Name",name);
            using (FileStream cabFile = new FileStream(path, FileMode.Open))
            {
                using (CabinetExtractor extrator = new CabinetExtractor())
                {
                    Stream manifestContent = extrator.Extract(cabFile, "manifest.xml");
                    _ManifestDoc = new XmlDocument();
                    _ManifestDoc.Load(manifestContent);
                    _NamespaceManager = InitXMLNamespace(_ManifestDoc);
                    InitSolution(cabFile, extrator);
                }
            }
        }

        public AveSolutionPackage(Stream cabFile, string name)
        {
            base.DataCache.AddProperty("Name", name);
            using (CabinetExtractor extrator = new CabinetExtractor())
            {
                Stream manifestContent = extrator.Extract(cabFile, "manifest.xml");
                _ManifestDoc = new XmlDocument();
                _ManifestDoc.Load(manifestContent);
                _NamespaceManager = InitXMLNamespace(_ManifestDoc);
                InitSolution(cabFile, extrator);
            }
        }

        private XmlNamespaceManager InitXMLNamespace(XmlDocument xmlDoc)
        {
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNamespaceManager.AddNamespace("WebPartSolution", "http://schemas.microsoft.com/WebPart/v2/Manifest");
            xmlNamespaceManager.AddNamespace("Solution", "http://schemas.microsoft.com/sharepoint/");
            return xmlNamespaceManager;
        }

        private void InitSolution(Stream cabFile, CabinetExtractor extrator)
        {
            XmlElement root = _ManifestDoc.DocumentElement;
            if (!root.HasAttribute("SolutionId") || string.IsNullOrEmpty(root.Attributes["SolutionId"].Value))
            {
                throw new Exception("Solution file is not right");
            }
            XmlAttribute xmlAttribute = root.Attributes["DeploymentServerType"];
            if (xmlAttribute != null && string.Compare(xmlAttribute.Value, "ApplicationServer", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.DataCache.AddProperty("DeploymentServerType",AveServerRole.Application);
            }
            this.DataCache.AddProperty("SolutionId",new Guid(root.Attributes["SolutionId"].Value));
            AddCasPolicySettings(root);
            AddAssemblies(root);
            InitSolutionDependencies(root);
            InitSolutionFeatures(root, cabFile, extrator);
            InitWebTemplateName(root, cabFile, extrator);
        }

        private void AddCasPolicySettings(XmlNode root)
        {
            XmlNodeList xmlNodeList = root.SelectNodes("Solution:CodeAccessSecurity/Solution:PolicyItem", this._NamespaceManager);
            if (xmlNodeList != null && xmlNodeList.Count >= 1)
            {
                return;
            }
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                XmlNode xmlNode2 = xmlNode.SelectSingleNode("Solution:PermissionSet", this._NamespaceManager);
                if (xmlNode2 == null)
                {
                    continue;
                }
                XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("Solution:Assemblies/Solution:Assembly", this._NamespaceManager);
                if (xmlNodeList2.Count == 0)
                {
                    continue;
                }
                base.DataCache.AddProperty("ContainsCasPolicy",true);
                base.DataCache.AddProperty("ContainsWebApplicationResource",true);
            }
        }

        private void AddAssemblies(XmlNode root)
        {
            string xpath = IsWspSolutionCab(root) ? "Solution:Assemblies/Solution:Assembly" : "WebPartSolution:Assemblies/WebPartSolution:Assembly";
            string text = IsWspSolutionCab(root) ? "Location" : "FileName";
            XmlNodeList xmlNodeList = root.SelectNodes(xpath, this._NamespaceManager);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                string text2 = xmlNode.Attributes[text] == null ? null : xmlNode.Attributes[text].Value;
                if (string.IsNullOrEmpty(text2))
                {
                    throw new Exception("The solution is not right.");
                }
                XmlAttribute xmlAttribute = xmlNode.Attributes["DeploymentTarget"];
                if (xmlAttribute != null && string.Compare(xmlAttribute.Value, "GlobalAssemblyCache", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    base.DataCache.AddProperty("ContainsGlobalAssembly",true);
                    return;
                }
                if (!IsWspSolutionCab(root))
                {
                    base.DataCache.AddProperty("ContainsGlobalAssembly",true);
                }
            }
        }

        private bool IsWspSolutionCab(XmlNode root)
        {
            if (string.Compare(root.Name, "Solution", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        private void InitSolutionDependencies(XmlNode root)
        {
            var dependenciesNode = root.SelectNodes("Solution:ActivationDependencies/Solution:ActivationDependency", _NamespaceManager);
            if (dependenciesNode == null || dependenciesNode.Count == 0)
            {
                return;
            }
            List<AveSolutionDependency> dependencySolutions = new List<AveSolutionDependency>();
            foreach (XmlNode node in dependenciesNode)
            {
                string text = node.Attributes["SolutionId"] != null ? node.Attributes["SolutionId"].Value : string.Empty;
                string title = node.Attributes["SolutionTitle"] != null ? node.Attributes["SolutionTitle"].Value : string.Empty;
                string name = node.Attributes["SolutionName"] != null ? node.Attributes["SolutionName"].Value : string.Empty;
                string url = node.Attributes["SolutionUrl"] != null ? node.Attributes["SolutionUrl"].Value : string.Empty;
                AveSolutionDependency item = string.IsNullOrEmpty(text) ? new AveSolutionDependency(Guid.Empty, title, name, url) :
                                                                          new AveSolutionDependency(new Guid(text), title, name, url);

                dependencySolutions.Add(item);
            }
            this.DataCache.AddProperty("SolutionDependencies",dependencySolutions);
        }

        private void InitSolutionFeatures(XmlNode root, Stream cabFile, CabinetExtractor extrator)
        {
            var features = new List<AveSolutionFeature>();
            XmlNodeList xmlNodeList = root.SelectNodes("Solution:FeatureManifests/Solution:FeatureManifest", this._NamespaceManager);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                string text = xmlNode.Attributes["Location"] == null ? string.Empty : xmlNode.Attributes["Location"].Value;
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Solution File Error.");
                }
                AveSolutionFeature feature = new AveSolutionFeature(text);
                using (Stream featureXmlStream = extrator.Extract(cabFile, text))
                {
                    XmlDocument featureXml = new XmlDocument();
                    featureXml.Load(featureXmlStream);
                    XmlAttribute idAttribute = featureXml.DocumentElement.Attributes["Id"];
                    XmlAttribute scopeAttribute = featureXml.DocumentElement.Attributes["Scope"];
                    if (idAttribute == null || scopeAttribute == null)
                    {
                        throw new Exception("An error occurred in Solution feature file.");
                    }
                    feature.FeatureId = new Guid(idAttribute.Value);
                    feature.Scope = AveFeature.StringToScope(scopeAttribute.Value);
                    feature.Title = featureXml.DocumentElement.Attributes["Title"] != null ? featureXml.DocumentElement.Attributes["Title"].Value : string.Empty;
                }
                features.Add(feature);
            }
            this.DataCache.AddProperty("Features",features);
        }

        //获取Solution中的TemplateName 以及Template所在的Group 信息
        public void InitWebTemplateName(XmlNode root, Stream cabFile, CabinetExtractor extrator)
        {
            List<Dictionary<string, string>> solutionNamesDir = new List<Dictionary<string, string>>(); 
            if (IsWspSolutionCab(root))
            {
                XmlNodeList xmlNodeList = root.SelectNodes("Solution:FeatureManifests/Solution:FeatureManifest", this._NamespaceManager);
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    string featureXmlPath = xmlNode.Attributes["Location"] == null ? string.Empty : xmlNode.Attributes["Location"].Value;
                    if (string.IsNullOrEmpty(featureXmlPath))
                    {
                        throw new Exception("Solution File Error.");
                    }
                    string parentFolderPath = featureXmlPath.Substring(0, featureXmlPath.IndexOf("\\"));
                    using (Stream featureXmlStream = extrator.Extract(cabFile, featureXmlPath))
                    {
                        if (featureXmlStream != null)
                        {
                            XmlDocument featureXml = new XmlDocument();
                            featureXml.Load(featureXmlStream);
                            string id = featureXml.DocumentElement.GetAttribute("Id");
                            if (!string.IsNullOrEmpty(id))
                            {
                                id = id.StartsWith("{") ? id.ToUpper() : "{" + id.ToUpper() + "}";
                                AssembleSolutionInfo(featureXml.DocumentElement, cabFile, extrator, parentFolderPath, id, solutionNamesDir);
                            }
                        }
                    }
                }
            }
            this.DataCache.AddProperty("WebTemplatesInfo",solutionNamesDir);
        }


        public void AssembleSolutionInfo(XmlNode root, Stream cabFile, CabinetExtractor extrator, string parentFolderPath, string id, List<Dictionary<string, string>> solutionInfos)
        {
            XmlNodeList nodeList = root.SelectNodes("Solution:ElementManifests/Solution:ElementManifest", this._NamespaceManager);
            foreach (XmlNode xmlNode in nodeList)
            {
                string tempElementXmlPath = xmlNode.Attributes["Location"] == null ? string.Empty : xmlNode.Attributes["Location"].Value;
                if (string.IsNullOrEmpty(tempElementXmlPath))
                {
                    throw new Exception("Solution File Error.");
                }
                string elementXmlPath = parentFolderPath + "\\" + tempElementXmlPath;
                string elementXmlName = elementXmlPath.Contains("\\") ? elementXmlPath.Substring(elementXmlPath.LastIndexOf("\\") + 1) : elementXmlPath;
                if (elementXmlName.Equals("Elements.xml", StringComparison.CurrentCulture))
                {
                    using (Stream elementXmlStream = extrator.Extract(cabFile, elementXmlPath))
                    {
                        if (elementXmlStream != null)
                        {
                            XmlDocument elementXml = new XmlDocument();
                            elementXml.Load(elementXmlStream);
                            XmlNodeList elementNodeList = elementXml.DocumentElement.SelectNodes("Solution:WebTemplate", this._NamespaceManager);
                            foreach (XmlNode elementNode in elementNodeList)
                            {
                                XmlElement xmlEle = (elementNode as XmlElement);
                                if (xmlEle != null)
                                {
                                    Dictionary<string, string> webSolutionInfoDir = new Dictionary<string, string>();
                                    string solutionName = xmlEle.HasAttribute("Name") ? xmlEle.GetAttribute("Name") : string.Empty;
                                    string solutionWTName = id + "#" + solutionName;
                                    string solutionDiscription = xmlEle.HasAttribute("Description") ? xmlEle.GetAttribute("Description") : string.Empty;
                                    string solutionLCID = xmlEle.HasAttribute("Locale") ? xmlEle.GetAttribute("Locale") : string.Empty;
                                    string solutionDisplayCategory = xmlEle.HasAttribute("DisplayCategory") ? xmlEle.GetAttribute("DisplayCategory") : "Custom";
                                    webSolutionInfoDir.Add("Name",solutionName);
                                    webSolutionInfoDir.Add("WebTemplateName", solutionWTName);
                                    webSolutionInfoDir.Add("Description",solutionDiscription);
                                    webSolutionInfoDir.Add("LCID",solutionLCID);
                                    webSolutionInfoDir.Add("DisplayCategory",solutionDisplayCategory);
                                    solutionInfos.Add(webSolutionInfoDir);
                                }
                            }
                        }
                    }
                }
            }
        }


        public List<Dictionary<string, string>> WebTemplatesInfo
        {
            get
            {
                List<Dictionary<string, string>> webTemplatesInfo = base.DataCache.GetProperty <List<Dictionary<string, string>>>("WebTemplatesInfo");
                if (webTemplatesInfo.Count > 0)
                {
                    return webTemplatesInfo;
                }
                throw new Exception("No web templates found.");
            }
        }

        public string Name
        {
            get { return base.DataCache.GetProperty<string>("Name"); }
        }

        public bool ContainsCasPolicy
        {
            get { return base.DataCache.GetProperty<bool>("ContainsCasPolicy"); }
        }

        public bool ContainsGlobalAssembly
        {
            get { return base.DataCache.GetProperty<bool>("ContainsGlobalAssembly"); }
        }

        public bool ContainsWebApplicationResource
        {
            get { return base.DataCache.GetProperty<bool>("ContainsWebApplicationResource"); }
        }

        public AveServerRole DeploymentServerType
        {
            get
            {
                if (this.DataCache.IsPropertyNotLoaded("DeploymentServerType"))
                {
                    return AveServerRole.WebFrontEnd;
                }
                return this.DataCache.GetProperty<AveServerRole>("DeploymentServerType");
            }
        }

        public Guid SolutionId
        {
            get { return base.DataCache.GetProperty<Guid>("SolutionId"); }
        }


        public List<AveSolutionFeature> Features
        {
            get { return this.DataCache.GetProperty<List<AveSolutionFeature>>("Features"); }
        }

        public List<AveSolutionDependency> SolutionDependencies
        {
            get { return this.DataCache.GetProperty<List<AveSolutionDependency>>("SolutionDependencies"); }
        }
    }
}
