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
using System.Reflection;
using System.Xml;
using AutoInstallation.Contract.UpgradeConfig;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class UpgradeConfigHandler
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void UpgradeConfigFile(XmlDocument newestConfig, string oldFilePath,
            List<ConfigItem> upgradePolicy)
        {
            try
            {
                var old = new XmlDocument();
                old.Load(oldFilePath);
                foreach (var item in upgradePolicy)
                {
                }
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_UPGRADECONIFGFILEERROR, ex.ToString());
            }
        }

        /// <summary>
        ///     无结构通用升级
        ///     不适合没有name的多个结点
        ///     1.多个同名结点而没有name属性，加在找到的第一个节点
        ///     <A>
        ///         <B name=""></B>
        ///     </A>
        ///     <A>
        ///         <B name=""></B>
        ///     </A>
        ///     2.
        /// </summary>
        /// <param name="newestConfig"></param>
        /// <param name="oldFilePath"></param>
        public static void UpgradeConfigFile(XmlDocument newestConfig, string oldFilePath)
        {
            try
            {
                var old = new XmlDocument();
                old.Load(oldFilePath);
                //忽略root节点不一样的情况
                //if (old.DocumentElement.Name != newestConfig.DocumentElement.Name)
                //{
                //    XmlNode temp = old.ImportNode(newestConfig.DocumentElement.FirstChild, true);
                //    old.DocumentElement.AppendChild(temp);
                //}
                var isChange = UpgradeChild(newestConfig.DocumentElement, old.DocumentElement, old);
                //if (isChange)
                //{
                old.Save(oldFilePath);
                //}
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_UPGRADECONIFGFILEERROR, ex.ToString());
            }
        }

        /// <summary>
        ///     无结构通用升级
        ///     不适合没有name的多个结点
        /// </summary>
        /// <param name="newFilePath"></param>
        /// <param name="oldConfig"></param>
        public static void UpgradeConfigFile(string newFilePath, XmlDocument oldConfig)
        {
            try
            {
                var newXml = new XmlDocument();
                newXml.Load(newFilePath);
                //忽略root节点不一样的情况
                //if (old.DocumentElement.Name != newestConfig.DocumentElement.Name)
                //{
                //    XmlNode temp = old.ImportNode(newestConfig.DocumentElement.FirstChild, true);
                //    old.DocumentElement.AppendChild(temp);
                //}
                var isChange = UpgradeChild(newXml.DocumentElement, oldConfig.DocumentElement, oldConfig);
                //if (isChange)
                //{
                oldConfig.Save(newFilePath);
                //}
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_UPGRADECONIFGFILEERROR, ex.ToString());
            }
        }

        public static void UpgradeConfigFile(string newFilePath, XmlDocument oldConfig, Infomation policy)
        {
            try
            {
                var newXml = new XmlDocument();
                newXml.Load(newFilePath);
                var isChange = UpgradeChild(newXml, oldConfig, policy);
                //if (isChange)
                //{
                oldConfig.Save(newFilePath);
                //}
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_UPGRADECONIFGFILEERROR, ex.ToString());
            }
        }

        public static void UpgradeConfigFile(XmlDocument newest, string oldFilePath, Infomation policy)
        {
            try
            {
                var old = new XmlDocument();
                old.Load(oldFilePath);

                var isChange = UpgradeChild(newest, old, policy);
                //if (isChange)
                //{
                old.Save(oldFilePath);
                //}
            }
            catch (Exception ex)
            {
                logger.Warn(LOGRESX.COMMONUTILITYLOG_UPGRADECONIFGFILEERROR, ex.ToString());
            }
        }

        private static bool UpgradeChild(XmlDocument newest, XmlDocument old, Infomation policy)
        {
            XmlNamespaceManager newXmlnsm = null;
            XmlNamespaceManager oldXmlnsm = null;
            if (policy.Namespaces.Count > 0)
            {
                newXmlnsm = new XmlNamespaceManager(newest.NameTable);
                foreach (var v in policy.Namespaces) newXmlnsm.AddNamespace(v.Key, v.Value);
                oldXmlnsm = new XmlNamespaceManager(old.NameTable);
                foreach (var v in policy.Namespaces) oldXmlnsm.AddNamespace(v.Key, v.Value);
            }

            return MergeDocument(newest, old, policy.UpgradePolicys, newXmlnsm, oldXmlnsm);
        }

        private static bool MergeDocument(XmlDocument newest, XmlDocument old, List<ConfigItem> policys,
            XmlNamespaceManager newestXmlns, XmlNamespaceManager oldXmlns)
        {
            var isChange = false;
            var temp = policys.FindAll(item => item.Policy != UpgradePolicy.ForceReplace);
            var replaces = policys.FindAll(item => item.Policy == UpgradePolicy.ForceReplace);
            foreach (var item in temp.OrderBy(item => item.FullPath))
                switch (item.Policy)
                {
                    case UpgradePolicy.Delete:
                    {
                        isChange = DeleteNode(old, item, oldXmlns);
                        break;
                    }
                    case UpgradePolicy.ReplaceAll:
                    {
                        isChange = ReplaceAll(newest, old, item, newestXmlns, oldXmlns);
                        break;
                    }
                    case UpgradePolicy.Normal:
                    {
                        isChange = MergeSingleNode(newest, old, item, newestXmlns, oldXmlns);
                        break;
                    }
                    case UpgradePolicy.MergeAll:
                    {
                        MergeNode(newest, old, item, newestXmlns, oldXmlns);
                        break;
                    }
                }
            foreach (var ii in replaces.OrderBy(item => item.FullPath))
                isChange = MergeSingleNodeFromGui(newest, old, ii, newestXmlns, oldXmlns);
            return isChange;
        }

        private static bool MergeNode(XmlDocument newest, XmlDocument old, ConfigItem item,
            XmlNamespaceManager newestXmlns, XmlNamespaceManager oldXmlns)
        {
            var retValue = false;
            XmlNode newestNode;
            XmlNode oldNode;
            if (item.Key != null)
            {
                newestNode =
                    newest.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                        newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                    oldXmlns);
            }
            else
            {
                newestNode = newest.SelectSingleNode(item.FullPath, newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath, oldXmlns);
            }

            if (newestNode != null)
            {
                if (oldNode == null)
                {
                    retValue = true;
                    var temp = old.ImportNode(newestNode, true);
                    var index = item.FullPath.LastIndexOf('/');
                    if (index > 0)
                    {
                        var parent = SelectAndCreatePath(item.FullPath.Substring(0, index), old, oldXmlns);
                        if (parent != null) parent.AppendChild(temp);
                    }
                    else
                    {
                        logger.Warn("The parent node can not found in the old ConfigFile. xpath:{0},key:{1},value{2}.",
                            item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                            item.Key == null ? string.Empty : item.Key.Value);
                    }
                }
                else
                {
                    retValue = UpgradeChild(newestNode, oldNode, old);
                }
            }
            else
            {
                logger.Warn("The node can not found in the newest ConfigFile. xpath:{0},key:{1},value{2}.",
                    item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                    item.Key == null ? string.Empty : item.Key.Value);
            }

            return retValue;
        }

        private static bool MergeSingleNode(XmlDocument newest, XmlDocument old, ConfigItem item,
            XmlNamespaceManager newestXmlns, XmlNamespaceManager oldXmlns)
        {
            var retValue = false;
            XmlNode newestNode;
            XmlNode oldNode;
            if (item.Key != null)
            {
                newestNode =
                    newest.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                        newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                    oldXmlns);
            }
            else
            {
                newestNode = newest.SelectSingleNode(item.FullPath, newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath, oldXmlns);
            }

            if (newestNode != null)
            {
                if (oldNode == null)
                {
                    retValue = true;
                    var temp = old.ImportNode(newestNode, true);
                    var index = item.FullPath.LastIndexOf('/');
                    if (index > 0)
                    {
                        var parent = SelectAndCreatePath(item.FullPath.Substring(0, index), old, oldXmlns);
                        if (parent != null) parent.AppendChild(temp);
                    }
                    else
                    {
                        logger.Warn("The parent node can not found in the old ConfigFile. xpath:{0},key:{1},value{2}.",
                            item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                            item.Key == null ? string.Empty : item.Key.Value);
                    }
                }
                else
                {
                    retValue = MergeAttributes(newestNode, oldNode, old, item);
                }
            }
            else
            {
                logger.Warn("The node can not found in the newest ConfigFile. xpath:{0},key:{1},value{2}.",
                    item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                    item.Key == null ? string.Empty : item.Key.Value);
            }

            return retValue;
        }

        private static bool MergeSingleNodeFromGui(XmlDocument newest, XmlDocument old, ConfigItem item,
            XmlNamespaceManager newestXmlns, XmlNamespaceManager oldXmlns)
        {
            var retValue = false;
            XmlNode newestNode;
            XmlNode oldNode;
            if (item.Key != null)
            {
                newestNode =
                    newest.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                        newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                    oldXmlns);
            }
            else
            {
                newestNode = newest.SelectSingleNode(item.FullPath, newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath, oldXmlns);
            }

            if (newestNode != null)
            {
                if (oldNode == null)
                {
                    retValue = true;
                    var temp = old.ImportNode(newestNode, true);
                    var index = item.FullPath.LastIndexOf('/');
                    if (index > 0)
                    {
                        var parent = SelectAndCreatePath(item.FullPath.Substring(0, index), old, oldXmlns);
                        if (parent != null)
                        {
                            parent.AppendChild(temp);
                            oldNode = temp;
                        }
                    }
                    else
                    {
                        logger.Warn("The parent node can not found in the old ConfigFile. xpath:{0},key:{1},value{2}.",
                            item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                            item.Key == null ? string.Empty : item.Key.Value);
                    }
                }

                retValue = MergeAttributes(newestNode, oldNode, old, item);
            }
            else
            {
                logger.Warn("The node can not found in the newest ConfigFile. xpath:{0},key:{1},value{2}.",
                    item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                    item.Key == null ? string.Empty : item.Key.Value);
            }

            return retValue;
        }

        private static bool MergeAttributes(XmlNode newest, XmlNode old, XmlDocument doc, ConfigItem item)
        {
            var retValue = false;
            if (old != null)
            {
                foreach (XmlAttribute v in newest.Attributes)
                {
                    var temp = old.Attributes.Cast<XmlAttribute>().FirstOrDefault(it => it.Name == v.Name);
                    if (temp == null)
                    {
                        retValue = true;
                        var att = doc.CreateAttribute(v.Name);
                        att.Value = v.Value;
                        old.Attributes.Append(att);
                    }
                }

                foreach (var v in item.Attributes)
                    switch (v.Policy)
                    {
                        case UpgradePolicy.ForceReplace:
                        {
                            retValue = true;
                            var temp = old.Attributes.Cast<XmlAttribute>().FirstOrDefault(it => it.Name == v.Name);
                            if (temp == null)
                            {
                                var att = doc.CreateAttribute(v.Name);
                                att.Value = v.Value;
                                old.Attributes.Append(att);
                            }
                            else
                            {
                                temp.Value = v.Value;
                            }

                            break;
                        }
                        case UpgradePolicy.Delete:
                        {
                            var temp = old.Attributes.Cast<XmlAttribute>().FirstOrDefault(it => it.Name == v.Name);
                            if (temp != null)
                            {
                                retValue = true;
                                old.Attributes.Remove(temp);
                            }

                            break;
                        }
                    }
            }

            return retValue;
        }

        private static bool DeleteNode(XmlDocument old, ConfigItem item, XmlNamespaceManager oldXmlns)
        {
            var retValue = false;
            ;
            XmlNode deleteNode;
            if (item.Key != null)
                deleteNode = old.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                    oldXmlns);
            else
                deleteNode = old.SelectSingleNode(item.FullPath, oldXmlns);
            if (deleteNode != null)
            {
                deleteNode.ParentNode.RemoveChild(deleteNode);
                retValue = true;
            }

            return retValue;
        }

        private static bool ReplaceAll(XmlDocument newest, XmlDocument old, ConfigItem item,
            XmlNamespaceManager newestXmlns, XmlNamespaceManager oldXmlns)
        {
            var retValue = false;
            XmlNode newestNode;
            XmlNode oldNode;
            if (item.Key != null)
            {
                newestNode =
                    newest.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                        newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath + BuildKeySeleteString(item.Key.Name, item.Key.Value),
                    oldXmlns);
            }
            else
            {
                newestNode = newest.SelectSingleNode(item.FullPath, newestXmlns);
                oldNode = old.SelectSingleNode(item.FullPath, oldXmlns);
            }

            if (newestNode != null)
            {
                retValue = true;
                if (oldNode != null)
                {
                    var temp = old.ImportNode(newestNode, true);
                    var parent = oldNode.ParentNode;
                    parent.RemoveChild(oldNode);
                    parent.AppendChild(temp);
                }
                else
                {
                    var temp = old.ImportNode(newestNode, true);
                    var index = item.FullPath.LastIndexOf('/');
                    if (index > 0)
                    {
                        var parent = SelectAndCreatePath(item.FullPath.Substring(0, index), old, oldXmlns);
                        if (parent != null) parent.AppendChild(temp);
                    }
                    else
                    {
                        logger.Warn("The parent node can not found in the old ConfigFile. xpath:{0},key:{1},value{2}.",
                            item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                            item.Key == null ? string.Empty : item.Key.Value);
                    }
                }

                retValue = true;
            }
            else
            {
                logger.Warn("The node can not found in the newest ConfigFile. xpath:{0},key:{1},value{2}.",
                    item.FullPath, item.Key == null ? string.Empty : item.Key.Name,
                    item.Key == null ? string.Empty : item.Key.Value);
            }

            return retValue;
        }

        private static XmlNode SelectAndCreatePath(string xpath, XmlDocument old, XmlNamespaceManager oldXmlns)
        {
            XmlNode retValue;
            retValue = old.SelectSingleNode(xpath, oldXmlns);
            if (retValue == null)
            {
                var index = xpath.LastIndexOf('/');
                if (index > 0)
                {
                    var parent = SelectAndCreatePath(xpath.Substring(0, index), old, oldXmlns);
                    if (parent != null) retValue = parent.AppendChild(old.CreateElement(xpath.Substring(index + 1)));
                }
                else
                {
                    logger.Warn("The parent node can not found in the old ConfigFile. xpath:{0}.", xpath);
                }
            }

            return retValue;
        }

        public static string BuildKeySeleteString(string key, string value)
        {
            return "[@" + key + "='" + value + "']";
        }

        private static bool UpgradeChild(XmlNode newest, XmlNode old, XmlDocument doc)
        {
            //logger.Info("beging:"+newest.Name);
            var retValue = false;
            if (newest.SchemaInfo != old.SchemaInfo)
                foreach (XmlNode node in newest.ChildNodes)
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        var results = old.ChildNodes.Cast<XmlNode>()
                            .Where(item => item.Name == node.Name && item.NodeType == XmlNodeType.Element).ToList();
                        if (results.Count == 0)
                        {
                            retValue = true;
                            //logger.Info(node.Name);
                            var temp = doc.ImportNode(node, true);
                            old.AppendChild(temp);
                        }
                        else if (results.Count == 1)
                        {
                            retValue = MergeAttributes(node, results[0], doc);
                        }
                        else //>1
                        {
                            if (node.Attributes["name"] != null)
                            {
                                var hasname = results.Cast<XmlNode>().FirstOrDefault(item =>
                                    (item.Attributes["name"] == null
                                        ? string.Empty
                                        : item.Attributes["name"].Value.ToString()) ==
                                    node.Attributes["name"].Value.ToString());
                                retValue = MergeNode(node, hasname, old, doc);
                            }
                            else if (node.Attributes["key"] != null)
                            {
                                var hasname = results.Cast<XmlNode>().FirstOrDefault(item =>
                                    (item.Attributes["key"] == null
                                        ? string.Empty
                                        : item.Attributes["key"].Value.ToString()) ==
                                    node.Attributes["key"].Value.ToString());
                                retValue = MergeNode(node, hasname, old, doc);
                            }
                            else //多个同名结点而没有name属性，加在找到的第一个节点
                            {
                                var noname = results.Cast<XmlNode>().FirstOrDefault(item =>
                                    item.Attributes["name"] == null && item.Attributes["key"] == null);
                                retValue = MergeNode(node, noname, old, doc);
                            }
                        }
                    }

            return retValue;
        }

        private static bool MergeAttributes(XmlNode newest, XmlNode old, XmlDocument doc)
        {
            var retValue = false;
            foreach (XmlAttribute newatt in newest.Attributes)
            {
                var oldatt = old.Attributes.Cast<XmlAttribute>().FirstOrDefault(item => item.Name == newatt.Name);
                if (oldatt == null)
                {
                    retValue = true;
                    //logger.Info(node.Name);
                    var att = doc.CreateAttribute(newatt.Name);
                    att.Value = newatt.Value;
                    old.Attributes.Append(att);
                }
            }

            if (retValue)
                UpgradeChild(newest, old, doc);
            else
                retValue = UpgradeChild(newest, old, doc);
            return retValue;
        }

        private static bool MergeNode(XmlNode newest, XmlNode selected, XmlNode old, XmlDocument doc)
        {
            var retValue = false;
            if (selected == null)
            {
                retValue = true;
                //logger.Info(newest.Name);
                var temp = doc.ImportNode(newest, true);
                old.AppendChild(temp);
            }
            else
            {
                retValue = MergeAttributes(newest, selected, doc);
            }

            return retValue;
        }
    }
}