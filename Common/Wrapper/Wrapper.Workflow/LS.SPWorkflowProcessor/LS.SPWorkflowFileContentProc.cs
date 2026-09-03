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
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace LS.SPWorkflowProcessor
{
    internal enum SPWorkflowFileContentProcType
    { 
        Invalid,
        Config,
        Xoml,
        Rules,
        Aspx,
        Xaml
    }

    public class SPWorkflowFileContentCustomProc
    {
        private SPWorkflowFileContentProc mConfigFileProc;
        private SPWorkflowFileContentProc mXomlFileProc;
        private SPWorkflowFileContentProc mRulesFileProc;
        private SPWorkflowFileContentProc mAspxFileProc;
        private SPWorkflowFileContentProc mXamlFileProc;

        public SPWorkflowFileContentProc ConfigFileProcessor
        {
            get { return mConfigFileProc; }
            set { mConfigFileProc = value; }
        }

        public SPWorkflowFileContentProc XomlFileProcessor
        {
            get { return mXomlFileProc; }
            set { mXomlFileProc = value; }
        }

        public SPWorkflowFileContentProc RulesFileProcessor
        {
            get { return mRulesFileProc; }
            set { mRulesFileProc = value; }
        }

        public SPWorkflowFileContentProc AspxFileProcessor
        {
            get { return mAspxFileProc; }
            set { mAspxFileProc = value; }
        }

        public SPWorkflowFileContentProc XamlFileProcessor
        {
            get { return mXamlFileProc; }
            set { mXamlFileProc = value; }
        }
    }

    public class SPWorkflowFileContentProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected IAveFile mFile = null;
        public IAveFile SPFile
        {
            get { return mFile; }
        }
        protected byte[] mOriginalContent = null;
        public byte[] OriginalContent
        {
            get
            {
                return mOriginalContent;
            }
        }

        private static Dictionary<Guid, SPWorkflowFileContentCustomProc> mCustomProcs;
        public static Dictionary<Guid, SPWorkflowFileContentCustomProc> CustomContentProcessors
        {
            get 
            {
                if (mCustomProcs == null)
                    mCustomProcs = new Dictionary<Guid, SPWorkflowFileContentCustomProc>();
                return mCustomProcs;
            }
            set
            {
                mCustomProcs = value;
            }
        }

        private static SPWorkflowFileContentProc GetInstance(Guid baseId, SPWorkflowFileContentProcType procType)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SPWorkflowFileContentProc.GetInstance");
            SPWorkflowFileContentProc instance = null;
            SPWorkflowFileContentCustomProc customProc = null;
            if (CustomContentProcessors.ContainsKey(baseId))
                customProc = CustomContentProcessors[baseId];
            else if(CustomContentProcessors.ContainsKey(Guid.Empty))
                customProc = CustomContentProcessors[Guid.Empty];

            if (customProc!=null)
            {
                switch (procType)
                {
                    case SPWorkflowFileContentProcType.Config:
                        if (customProc.ConfigFileProcessor != null)
                            instance = customProc.ConfigFileProcessor;
                        else
                            instance = new ConfigFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xoml:
                        if (customProc.XomlFileProcessor != null)
                            instance = customProc.XomlFileProcessor;
                        else
                            instance = new XomlFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Rules:
                        if (customProc.RulesFileProcessor != null)
                            instance = customProc.RulesFileProcessor;
                        else
                            instance = new RulesFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Aspx:
                        if (customProc.AspxFileProcessor != null)
                            instance = customProc.AspxFileProcessor;
                        else
                            instance = new AspxFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xaml:
                        if (customProc.XamlFileProcessor != null)
                            instance = customProc.XamlFileProcessor;
                        else
                            instance = new XamlFileProc();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (procType)
                {
                    case SPWorkflowFileContentProcType.Config:
                        instance = new ConfigFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xoml:
                        instance = new XomlFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Rules:
                        instance = new RulesFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Aspx:
                        instance = new AspxFileProc();
                        break;
                    case SPWorkflowFileContentProcType.Xaml:
                        instance = new XamlFileProc();
                        break;
                    default:
                        break;
                }
            }
            if (instance != null)
            {
                SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_CustomProcAssemblyName, procType.ToString(), instance.GetType().Assembly.FullName);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SPWorkflowFileContentProc.GetInstance");
            return instance;
        }

        public static SPWorkflowFileContentProc CreateInstance(Guid wfAssociationBaseId, IAveFile spFile, byte[] content)
        {
            SPWorkflowFileContentProc instance = null;
            string extension = spFile == null ? "xaml" : spFile.GetExtension().ToLower(CultureInfo.CurrentCulture);
            switch (extension)
            {
                case "xml":
                    instance = GetInstance(wfAssociationBaseId, SPWorkflowFileContentProcType.Config);
                    break;
                case "xoml":
                    instance = GetInstance(wfAssociationBaseId, SPWorkflowFileContentProcType.Xoml);
                    break;
                case "rules":
                    instance = GetInstance(wfAssociationBaseId, SPWorkflowFileContentProcType.Rules);
                    break;
                case "aspx":
                    instance = GetInstance(wfAssociationBaseId, SPWorkflowFileContentProcType.Aspx);
                    break;
                case "xaml":
                    instance = GetInstance(wfAssociationBaseId, SPWorkflowFileContentProcType.Xaml);
                    instance.mOriginalContent = content;
                    break;
                default:
                    //throw new Exception("Not supported.");
                    break;
            }
            if (instance != null)
                instance.mFile = spFile;
            return instance;
        }

        public static SPWorkflowFileContentProc CreateInstance(Guid wfAssociationBaseId, IAveFile spFile)
        {
            return CreateInstance(wfAssociationBaseId, spFile, null);
        }

        public virtual string ReplaceContent(Dictionary<string, object> dic)
        {
            string strContent = string.Empty;
            if (mFile != null)
            {
                using (StreamReader objReader = new StreamReader(mFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                {
                    strContent = objReader.ReadToEnd();
                }

                foreach (KeyValuePair<string, object> pair in dic)
                {
                    int replacedCount = 0;
                    strContent = LSUtilityOfBytes.LSReplaceStringIgnoreCase(strContent, pair.Key, pair.Value.ToString(), int.MaxValue, out replacedCount);
                }
            }
            return strContent;
        }

        public virtual string ReplaceUserInNintexWorkflow(string content)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            //nsmgr.AddNamespace("ns3", "clr-namespace:Nintex.Workflow.HumanApproval;Assembly=Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12");
            //XmlNodeList nodes = xmlDoc.SelectNodes(".//*[@User!='' or @UserID!='']", nsmgr);
            XmlNodeList nodes = xmlDoc.SelectNodes(".//*[@User!='' or @UserID!='']");
            foreach (XmlNode node in nodes)
            {
                try
                {
                    XmlElement xe = (XmlElement)node;
                    if (xe != null)
                    {
                        if (xe.HasAttribute("IsUser") && !Boolean.Parse(xe.GetAttribute("IsUser"))) 
                        {
                            continue;
                        }
                        if (xe.HasAttribute("User"))
                        {
                            string name = xe.GetAttribute("User");
                            if(name.Equals("{x:Null}", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            if (name.Contains("@"))
                            {
                                string newName = SPWorkflowCommon.OnModifyEmailAddress(null,name);
                                if (!newName.Equals(name))
                                {
                                    xe.SetAttribute("User", newName);
                                    continue;
                                }
                            }
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(name);
                            if (user != null)
                            {
                                xe.SetAttribute("User", user.LoginName);
                            }
                        }
                        if (xe.HasAttribute("UserID"))
                        {
                            string name = xe.GetAttribute("UserID");
                            if(name.Equals("{x:Null}", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            if (name.Contains("@"))
                            {
                                string newName = SPWorkflowCommon.OnModifyEmailAddress(null,name);
                                if (!newName.Equals(name))
                                {
                                    xe.SetAttribute("UserID", newName);
                                    continue;
                                }
                            }
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(name);
                            if (user != null)
                            {
                                xe.SetAttribute("UserID", user.LoginName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.NintexReplaceUserError, e);
                }
            }

            content = xmlDoc.OuterXml;
            return content;
        }

        public virtual string ReplaceUserInSPDkflow(string content)
        {
            string contenttemp = content;
            contenttemp = ReplaceUserByAttributeName(contenttemp, "AssignedTo");
            contenttemp = ReplaceUserByAttributeName(contenttemp, "UserName");//For created by/modified by/if is a valid user
            contenttemp = ReplaceUserByAttributeName(contenttemp, "UserValue");
            contenttemp = ReplaceUserByAttributeName(contenttemp, "To");
            contenttemp = ReplaceUserByAttributeName(contenttemp, "CC");
            contenttemp = ReplaceUserForFindValueActivityInSPDkflow(contenttemp);

            contenttemp = ReplaceUsersFor13ModeWorkflow(contenttemp);
            return contenttemp;
        }
        private string ReplaceUserForFindValueActivityInSPDkflow(string content)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);
                XmlNodeList findValueActivities = doc.SelectNodes(".//*[@ExternalFieldName='Editor']|.//*[@ExternalFieldName='Author']|.//*[@ExternalFieldName='CheckoutUser']|.//*[@FieldName='Author']|.//*[@FieldName='Editor']|.//*[@FieldName='CheckoutUser']");
                foreach (XmlNode node in findValueActivities)
                {
                    if (node is XmlElement)
                    {
                        XmlElement topNode = node as XmlElement;
                        try
                        {
                            XmlElement secondNode = topNode.ChildNodes.OfType<XmlElement>().FirstOrDefault<XmlElement>();
                            if (secondNode != null)
                            {
                                XmlElement userInfoNode = secondNode.ChildNodes.OfType<XmlElement>().FirstOrDefault<XmlElement>();
                                if (userInfoNode != null && !string.IsNullOrEmpty(userInfoNode.InnerText))
                                {
                                    IAveUser user = SPPermissionProcessor.GetOrCreateUser(userInfoNode.InnerText);
                                    if (user != null)
                                    {
                                        userInfoNode.InnerText = user.LoginName;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Debug("An error occurred while replace user in workflow definition or template./r/nNodeInfo:{0},/r/nError:{1}", node?.OuterXml, e);
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }

        private string ReplaceUserByAttributeName(string content, string attributeName)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);

                XmlNodeList nodes = doc.SelectNodes(".//*[@" + attributeName + "!='']");
                foreach (XmlNode node in nodes)
                {
                    if (node is XmlElement)
                    {
                        XmlElement xe = node as XmlElement;
                        string attributeValue = xe.GetAttribute(attributeName);

                        if (attributeValue.StartsWith("{ActivityBind", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlNode refNode = doc.SelectSingleNode(".//*[@Value][@ReturnValue='" + attributeName + "']");
                            if (refNode != null)
                            {
                                XmlElement xe1 = refNode as XmlElement;
                                string refValue = xe1.GetAttribute("Value");

                                IAveUser user = SPPermissionProcessor.GetOrCreateUser(refValue);
                                if (user != null)
                                {
                                    xe1.SetAttribute("Value", user.LoginName);
                                }
                            }
                        }
                        else
                        {
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(attributeValue);
                            if (user != null)
                            {
                                xe.SetAttribute(attributeName, user.LoginName);
                            }
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, ex);
            }
            finally
            {
                doc.RemoveAll();
            }

            return content;
        }

        private string ReplaceUsersFor13ModeWorkflow(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlNamespaceManager xnsm = new XmlNamespaceManager(doc.NameTable);
                xnsm.AddNamespace("local", doc.DocumentElement.GetNamespaceOfPrefix("local"));
                xnsm.AddNamespace("local1", doc.DocumentElement.GetNamespaceOfPrefix("local1"));
                xnsm.AddNamespace("p", doc.DocumentElement.GetNamespaceOfPrefix("p"));
                xnsm.AddNamespace("p1", doc.DocumentElement.GetNamespaceOfPrefix("p1"));
                xnsm.AddNamespace("p2", doc.DocumentElement.GetNamespaceOfPrefix("p2"));
                ReplaceEqualUsers(doc, xnsm);//For if XXX equals/notequals  User
                ReplaceExpandInitFormUsers(doc, xnsm, new string[] { "local", "p" });//For Email to /  start a process with
                ReplaceExpandInitFormUsers(doc, xnsm, new string[] { "p1", "p2" });
                return doc.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceUsersFor13ModeWorkflow. /r/nXmlInfo:{0}, /r/nError:{1}", content, ex);
                return content;
            }
        }

        private void ReplaceEqualUsers(XmlDocument doc, XmlNamespaceManager xnsm)
        {

            try
            {
                var persons = doc.SelectNodes(".//local1:IsEqualUser", xnsm);
                if (persons != null)
                {
                    foreach (XmlNode person in persons)
                    {
                        try
                        {
                            var personElement = person as XmlElement;
                            if (personElement != null)
                            {

                                var userLogin = personElement.GetAttribute("Right");
                                if (!string.IsNullOrEmpty(userLogin))
                                {
                                    var user = SPPermissionProcessor.GetOrCreateUser(userLogin);
                                    if (user != null)
                                    {
                                        personElement.SetAttribute("Right", user.LoginName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An error occurred while replace single user in ReplaceEqualsUsersFor13ModeWorkflow.NodeInfo:{0},Error:{1}", person.OuterXml, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceEqualsUsersFor13ModeWorkflow./r/nError:{0}", ex);
            }
        }

        private void ReplaceExpandInitFormUsers(XmlDocument doc, XmlNamespaceManager xnsm, string[] args)
        {

            try
            {
                XmlNodeList users = doc.SelectNodes(string.Format(".//{0}:ExpandInitFormUsers", args[0]), xnsm);
                foreach (XmlNode user in users)
                {
                    try
                    {
                        XmlNode userCollection = user.SelectSingleNode(string.Format(".//{0}:BuildCollection.Values", args[1]), xnsm);
                        if (userCollection != null && userCollection.ChildNodes.Count > 0)
                        {
                            ArgumentNullException.ThrowIfNull(userCollection.ChildNodes.OfType<XmlElement>());
                            foreach (XmlElement person in userCollection.ChildNodes.OfType<XmlElement>())
                            {
                                string oldLogin = person.InnerText;
                                IAveUser loginUser = SPPermissionProcessor.GetOrCreateUser(oldLogin);
                                if (loginUser != null)
                                {
                                    person.InnerText = loginUser.LoginName;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while replace single user in BuildCollection.NodeInfo:{0},Error:{1}", user.OuterXml, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while ReplaceExpandInitFormUsers./r/nError:{0}", ex);
            }
        }

        public virtual string ReplaceUserInSPDkflowRules(string content)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                XmlNodeList nodes = doc.DocumentElement.GetElementsByTagName("ns0:CodePrimitiveExpression.Value");
                string originalStr = string.Empty;
                string pattern = string.Empty;
                int lenGroup = -1;
                int lenUser = -1;
                Regex regex = new Regex(string.Empty);
                foreach (XmlNode node in nodes)
                {
                    originalStr = node.InnerText;
                    if (originalStr.Contains("\\"))
                    {
                        lenGroup = originalStr.IndexOf('\\');
                        lenUser = originalStr.Substring(lenGroup + 1).Length;
                        pattern = @"([^\/\[\]:;\|=,\+\*\?<>@]){" + lenGroup + @"}\\{1}([^\/\[\]:;\|=,\+\*\?<>@]){" + lenUser + "}";
                        regex = new Regex(pattern);
                        if (regex.IsMatch(originalStr))
                        {
                            IAveUser user = SPPermissionProcessor.GetOrCreateUser(originalStr);
                            if (user != null)
                            {
                                node.InnerText = user.LoginName;
                            }
                        }
                    }
                }
                content = doc.OuterXml;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.UserReplaceError, e);
            }
            finally
            {
                doc.RemoveAll();
            }
            return content;
        }
    }

    internal sealed class ConfigFileProc : SPWorkflowFileContentProc
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Hist:History as key.")]
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Config File Processor Replace");
            string strContent = string.Empty;

            try
            {
                if (mFile != null)
                {
                    using (StreamReader objReader = new StreamReader(mFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                    {
                        strContent = objReader.ReadToEnd();
                    }

                    XmlDocument xmlConfig = null;
                    try
                    {
                        xmlConfig = new XmlDocument();
                        xmlConfig.LoadXml(strContent);
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@DocLibID") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@DocLibID").Value = (string)dic["TemplateListId"];
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion").Value = (string)dic["XomlFileVersion"];
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion") != null)
                        {

                            xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion").Value = dic.ContainsKey("RulesFileVersion") ?
                                (string)dic["RulesFileVersion"] : null;
                           
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@BaseID") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@BaseID").Value = (string)dic["BaseID"];
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@ContentTypeID") != null)
                        {
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@ContentTypeID").Value != string.Empty)
                            {
                                xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@ContentTypeID").Value = (string)dic["ContentTypeId"];
                            }
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@Category") != null)
                        {
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@Category").Value.Contains("ContentType;"))
                            {                              
                                xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@Category").Value = "ContentType;" + (string)dic["ContentTypeId"];
                            }                          
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@ListID") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@ListID").Value = (string)dic["ParentId"];
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@TaskListID") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@TaskListID").Value = (string)dic["TaskListId"];
                        }
                        if (xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@HistoryListID") != null)
                        {
                            xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@HistoryListID").Value = (string)dic["HistListId"];
                        }
                        else
                        {
                            try
                            {
                                XmlNode assoNode = xmlConfig.SelectSingleNode("/WorkflowConfig/Association");
                                XmlElement assoHis = (XmlElement)assoNode;
                                assoHis.SetAttribute("HistoryListID", (string)dic["HistListId"]);
                            }
                            catch(Exception ex)
                            {
                                SPWorkflowProcessorRuntime.Log(Logs.Common_XmlFileHandleException, ex.Message);
                                //SPWorkflowProcessorRuntime.Log(LogLevel.Exception, LogScopeEnum.WorkflowAssociationScope, ex.ToString(), "Error:AppendXmlAttributteException.");
                            }
                        }

                        XmlNodeList ctList = xmlConfig.SelectNodes("/WorkflowConfig/ContentTypes/ContentType");
                        if (ctList != null)
                        {
                            foreach (XmlElement ctNode in ctList)
                            {
                                string oldCTId = ctNode.GetAttribute("ContentTypeID");
                                if (!string.IsNullOrEmpty(oldCTId))
                                {
                                    oldCTId = oldCTId.ToUpperEx(2, oldCTId.Length - 2);
                                    if (dic.ContainsKey(oldCTId))
                                        ctNode.SetAttribute("ContentTypeID", (string)dic[oldCTId]);
                                }
                            }
                        }


                        strContent = xmlConfig.OuterXml;
                        try
                        {
                            mFile.CheckOut(false, string.Empty);
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                        }
                        mFile.SaveBinary(Encoding.UTF8.GetBytes(strContent));
                        mFile.Update();

                        try
                        {
                            mFile.CheckIn(string.Empty);
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
                        }
                    }
                    finally
                    {
                        if (xmlConfig != null)
                            xmlConfig.RemoveAll();
                    }
                }
                else
                {
                    throw new ApplicationException("Config file cannot be found");
                }
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Config File Processor Replace");
            }
            return strContent;
        }

        public static string GetTemplateLibIdStr(byte[] content)
        {
            string rlt = string.Empty;
            XmlDocument xmlConfig = null;
            try
            {
                string strContent = string.Empty;
                using (MemoryStream stream = new MemoryStream(content))
                {
                    using (StreamReader objReader = new StreamReader(stream))
                    {
                        strContent = objReader.ReadToEnd();
                    }
                }
                xmlConfig = new XmlDocument();
                xmlConfig.LoadXml(strContent);
                if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@DocLibID") != null)
                {
                    rlt = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@DocLibID").Value;
                }

            }
            catch(Exception ex) 
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
            }
            finally
            {
                if (xmlConfig != null)
                    xmlConfig.RemoveAll();
            }

            return rlt;
        }
    }

    internal sealed class XomlFileProc : SPWorkflowFileContentProc
    {
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Xoml File Processor Replace");
            string strContent = base.ReplaceContent(dic);
            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                strContent = base.ReplaceUserInNintexWorkflow(strContent);
                strContent = base.ReplaceUserInSPDkflow(strContent);
            }
            string charSet = mFile.CharSetName;
            if (string.IsNullOrEmpty(charSet))
                charSet = "utf-8";
            try
            {
                mFile.CheckOut(false, string.Empty);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
            }

            mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
            mFile.Update();
            try
            {
                mFile.CheckIn(string.Empty);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
            }
            SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_FileCharsetName, mFile.Name, charSet);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Xoml File Processor Replace");
            return strContent;
        }
    }

    internal sealed class RulesFileProc : SPWorkflowFileContentProc
    {
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Rules File Processor Replace");
            string strContent = base.ReplaceContent(dic);
            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                strContent = base.ReplaceUserInSPDkflowRules(strContent);
            }
            string charSet = mFile.CharSetName;
            if (string.IsNullOrEmpty(charSet))
                charSet = "utf-8";
            try
            {
                mFile.CheckOut(false, string.Empty);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
            }

            mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
            mFile.Update();
            try
            {
                mFile.CheckIn(string.Empty);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Rules File Processor Replace");
            return strContent;
        }
    }

    internal sealed class AspxFileProc : SPWorkflowFileContentProc
    {
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default ASPX File Processor Replace");
            string strContent = string.Empty;
            //WebPartPropertiesProc wpProc = new WebPartPropertiesProc();
            //wpProc.ReplaceAllWebPartProperties(spFile, dic);
            strContent = base.ReplaceContent(dic);
            string charSet = mFile.CharSetName;
            if (string.IsNullOrEmpty(charSet))
                charSet = "utf-8";
            try
            {
                mFile.CheckOut(false, string.Empty);
            }
            catch(Exception e) { SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message); }

            mFile.SaveBinary(Encoding.GetEncoding(charSet).GetBytes(strContent));
            mFile.Update();
            try
            {
                mFile.CheckIn(string.Empty);
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
            }
            SPWorkflowProcessorRuntime.Log(Logs.FileContentProc_FileCharsetName, mFile.Name, charSet);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default ASPX File Processor Replace");
            return strContent;
        }
    }

    internal sealed class XamlFileProc : SPWorkflowFileContentProc
    {
        public override string ReplaceContent(Dictionary<string, object> dic)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Default Xaml File Processor Replace");
            string strContent = Encoding.UTF8.GetString(OriginalContent);


            foreach (KeyValuePair<string, object> pair in dic)
            {
                int replacedCount = 0;
                strContent = LSUtilityOfBytes.LSReplaceStringIgnoreCase(strContent, pair.Key, pair.Value.ToString(), int.MaxValue, out replacedCount);
            }

            if (SPWorkflowProcessorRuntime.ProcessMarkOnlyWorkflow)
            {
                strContent = base.ReplaceUserInSPDkflow(strContent);
            }

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Default Xaml File Processor Replace");
            return strContent;
        }
    }

    internal class WebPartPropertiesProc 
    {
        public void ReplaceAllWebPartProperties(IAveFile spFile, Dictionary<string, object> dic)
        {

            IAveLimitedWebPartManager wpManager = spFile.GetLimitedWebPartManager(AvePersonalizationScope.Shared);
            foreach (IAveWebPart wp in wpManager.WebParts)
            {
                WebPartPropertiesProc instance = null;
                string typeId = wp.WebPartTypeID;// (Guid)LSInvoker.GetProperty(wp, "WebPartTypeID");
                //switch (typeId.ToString().ToUpper())
                //{
                //    case "":
                //        break;
                //    default:
                //        break;
                //}

                //if (instance != null)
                //    instance.ReplaceWebPartProperties(wp, dic);
            }
            wpManager.Dispose();
            wpManager.Web.Dispose();

        }

        public virtual void ReplaceWebPartProperties(IAveWebPart wp, Dictionary<string, object> dic)
        {
            
        }
    }

    internal sealed class DataFormWebPartProc : WebPartPropertiesProc
    {
        public override void ReplaceWebPartProperties(IAveWebPart wp, Dictionary<string, object> dic)
        {
            base.ReplaceWebPartProperties(wp, dic);
        }

    }
}
