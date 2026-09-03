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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common;

namespace LS.BinarySerialization
{
    
    internal class LSReplaceValues:IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private byte[] rawData;
        private Dictionary<string, object> repDictionary;
        private List<LSObjectNodeValueExtensionEx> valueInfos;
        private LSObjectNodeAnalyze mAnalyzeProc;
        private Replacer.LSMemberDataInfoEx mMemberDataInfoEx;

        private List<string> mListIdPropNames;
        private List<string> mUserAndEmailPropNames;

        private List<long> mReplacedNodes;
        private List<long> ReplacedNodes
        {
            get
            {
                if (mReplacedNodes == null)
                {
                    mReplacedNodes = new List<long>();
                }
                return mReplacedNodes;
            }
        }

        internal LSReplaceValues(byte[] data, Dictionary<string, object> dictionary,LSObjectNodeAnalyze analyzeProc)
        {
            rawData = new byte[data.Length];
            Array.Copy(data, rawData, data.Length);
            valueInfos = new List<LSObjectNodeValueExtensionEx>();
            repDictionary = dictionary;
            mAnalyzeProc = analyzeProc;


            mListIdPropNames = new List<string>();
            mListIdPropNames.Add("listId");
            mListIdPropNames.Add("externalListId");
            mListIdPropNames.Add("toListId");

            mUserAndEmailPropNames = new List<string>();
            mUserAndEmailPropNames.Add("userConfig+userId");//nintex workflow
            mUserAndEmailPropNames.Add("userId");//nintex workflow
            mUserAndEmailPropNames.Add("_items");//spd workflow
            mUserAndEmailPropNames.Add("_fullAssignTo");//spd workflow
            mUserAndEmailPropNames.Add("assignedTo");//spd workflow
        }

        internal byte[] ReplaceNodeValues(bool replace)
        {
            GetMemberDataEx();
            foreach (LSObjectNode node in mAnalyzeProc.DataNodes)
            {
                switch (node.Name)
                {
                    case "Microsoft.Office.Workflow.Actions.OneTaskProperties":
                        ReplaceOneTaskProps(node);
                        break;
                    case "Microsoft.Office.Workflow.Utility.Contact":
                        ReplaceContact(node);
                        break;
                    case "Microsoft.SharePoint.Workflow.SPActivationEventArgs":
                        ReplaceActivationEventArgs(node);
                        break;
                    case "Microsoft.SharePoint.Workflow.SPWorkflowActivationProperties":
                        ReplaceActivationProps(node, replace);
                        break;
                    case "Microsoft.SharePoint.Workflow.SPWorkflowTaskProperties":
                        ReplaceTaskProps(node, replace);
                        break;
                    case "Microsoft.SharePoint.WorkflowActions.WorkflowContext":
                        ReplaceWorkflowContext(node);
                        break;
                    case "System.Guid":
                        ReplaceGuid(node);
                        break;
                    case "System.Workflow.Runtime.CorrelationProperty":
                        ReplaceCorrelationProperty(node);
                        break;
                    case "System.UnitySerializationHolder":
                        ReplaceCompiledAssembly(node);
                        break;
                    case "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef":
                        //ReplaceActivityMemberData(node);
                        //ReplaceObjectMemberDatas(node);
                        ReplaceActivityData(null, node,null);
                        break;
                    case "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef":
                        break;
                    default:
                        break;
                }
            }

            if (replace)
            {
                valueInfos.Sort(new ValuePositionIcp());

                int i = valueInfos.Count - 1;
                try
                {
                    for (; i >= 0; i--)
                    {
                        ChangeValue(valueInfos[i].valueInfo, valueInfos[i].value);
                    }
                }
                catch(Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ChangeNodeValueError, e.ToString());
                    //need not to log
                    //Console.WriteLine(i);
                }
            }
            return rawData;
        }

        public void Dispose()
        {
            if (mReplacedNodes != null)
            {
                mReplacedNodes.Clear();
                mReplacedNodes = null;
            }
            if (mMemberDataInfoEx != null)
            {
                mMemberDataInfoEx.Dispose();
            }
            if (mAnalyzeProc != null)
            {
                mAnalyzeProc.Dispose();
            }
            if (valueInfos != null)
            {
                valueInfos.Clear();
                valueInfos = null;
            }
            if (repDictionary != null)
            {
                repDictionary.Clear();
                repDictionary = null;
            }
        }

        private void ReplaceOneTaskProps(LSObjectNode node)
        { 
            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            valueInfoEx.valueInfo = node.ValueTypes["_taskItemId"];
            valueInfoEx.value = repDictionary["_taskItemId"];
            valueInfoEx.objectName = node.Name;
            valueInfoEx.memberName = "_taskItemId";
            valueInfos.Add(valueInfoEx);
        }

        private void ReplaceContact(LSObjectNode node)
        {
            //Console.WriteLine("");

            List<string> extraInfo = new List<string>();
            if (node.Members.ContainsKey("m_loginName") && node.Members["m_loginName"] != null)
            {
                LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                valueInfoEx.valueInfo = node.ValueTypes["m_loginName"];
                valueInfoEx.value = LS.BinarySerialization.Replacer.LSBinarySerReplacer.RaiseModifyLoginEvent(node.Members["m_loginName"] as string, extraInfo);
                valueInfoEx.objectName = node.Name;
                valueInfoEx.memberName = "m_loginName";
                valueInfos.Add(valueInfoEx);
            }

            if (extraInfo.Count > 0)
            {
                List<string> schema = new List<string>();
                schema.Add("m_principalId");
                schema.Add("m_displayName");
                schema.Add("m_notes");
                schema.Add("m_emailAddress");

                for (int i = 0; i < schema.Count; i++)
                {
                    string memberName = schema[i];
                    if (!node.Members.ContainsKey(memberName))
                        continue;
                    if (!node.ValueTypes.ContainsKey(memberName))
                        continue;

                    LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                    valueInfoEx.valueInfo = node.ValueTypes[memberName];
                    if (memberName.Equals("m_principalId", StringComparison.OrdinalIgnoreCase))
                        valueInfoEx.value = int.Parse(extraInfo[i]);
                    else
                        valueInfoEx.value = extraInfo[i];
                    valueInfoEx.objectName = node.Name;
                    valueInfoEx.memberName = memberName;
                    valueInfos.Add(valueInfoEx);
                }
            }

            //Dictionary<string, object> rep = new Dictionary<string, object>(5);
            //rep.Add("m_loginName", "LS\\WFTester3");
            //rep.Add("m_displayName", "abcd");
            //rep.Add("m_emailAddress", "abcd@sample.com");
            //rep.Add("m_principalType", "abcd");
            //rep.Add("m_principalId", 100);
            //foreach (string memberName in rep.Keys)
            //{
            //    if (!node.ValueTypes.ContainsKey(memberName))
            //        continue;
            //    LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            //    valueInfoEx.valueInfo = node.ValueTypes[memberName];
            //    valueInfoEx.value = rep[memberName];
            //    valueInfoEx.objectName = node.Name;
            //    valueInfoEx.memberName = memberName;
            //    valueInfos.Add(valueInfoEx);
            //}
        }

        private void ReplaceActivationEventArgs(LSObjectNode node)
        {
            //Console.WriteLine("");
            /*
            Dictionary<string, object> rep = new Dictionary<string, object>(5);
            rep.Add("ExternalDataEventArgs+identity", "LS\abcd");
            foreach (string memberName in rep.Keys)
            {
                if (!node.ValueTypes.ContainsKey(memberName))
                    continue;
                LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                valueInfoEx.valueInfo = node.ValueTypes[memberName];
                valueInfoEx.value = rep[memberName];
                valueInfoEx.objectName = node.Name;
                valueInfoEx.memberName = memberName;
                valueInfos.Add(valueInfoEx);
            }
             */
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Hist:History as key.")]
        private void ReplaceActivationProps(LSObjectNode node,bool executeReplace)
        {
            if (!executeReplace)
            {
                File.WriteAllBytes(@"c:\ActivationProperties.txt", (byte[])node.ArrayValues[0]);
                return;
            }

            Encoding encoding = new UTF8Encoding(false, true);
            XmlDocument doc = null;
            try
            {
                string xmlActivationProps = encoding.GetString((byte[])node.ArrayValues[0]);
                doc = new XmlDocument();
                doc.LoadXml(xmlActivationProps);
                foreach (XmlNode xn in doc.FirstChild.ChildNodes)
                {
                    if (xn is XmlElement)
                    {
                        XmlElement fld = (XmlElement)xn;
                        string name=fld.GetAttribute("name");
                        switch (name)
                        { 
                            case "m_siteId":
                            case "m_webId":
                            case "m_itemId":
                            case "m_listId":
                            case "m_itemGuid":
                            case "m_taskListId":
                            case "m_workflowId":
                            case "m_histListId":
                                fld.InnerXml = repDictionary[name].ToString();
                                break;
                            case "m_associationData":
                            case "m_initiationData":
                            case "m_originator":
                            case "m_originatorEmail":
                            case "m_templateName":
                            case "m_siteUrl":/*http://avepoint-sdlneh:9100/sites/WorkflowTestSiteDst*/
                            case "m_webUrl":/*http://avepoint-sdlneh:9100/sites/WorkflowTestSiteDst*/
                            case "m_listUrl":/*/sites/WorkflowTestSiteDst/Shared Documents*/
                            case "m_itemUrl":/*Shared Documents/EBSPerformanceResult.txt*/
                            case "m_histListUrl":/*/sites/WorkflowTestSiteDst/Lists/Workflow History*/
                            case "m_taskListUrl":/*/sites/WorkflowTestSiteDst/Lists/Tasks*/
                            default:
                                break;
                        }
                    }
                }
                node.ArrayValues[0] = encoding.GetBytes(doc.InnerXml);
            }
            catch(Exception ex) 
            {
                log.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
            }

            if (!node.ValueTypes.ContainsKey("0"))
                return;
            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            valueInfoEx.valueInfo = node.ValueTypes["0"];
            valueInfoEx.value = node.ArrayValues[0];
            valueInfoEx.objectName = node.Name;
            valueInfoEx.memberName = "0";
            valueInfos.Add(valueInfoEx);

            
        }

        private int fileIndex = 0;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "fld:Special field of xml.")]
        private void ReplaceTaskProps(LSObjectNode node, bool executeReplace)
        {
            if (node.ArrayValues != null && node.ArrayValues.Length > 0)
            {
                if (node.ArrayValues[0] == null)
                    return;
                if (node.ArrayValues[0] is InternalMemberValueE)
                    return;

                XmlDocument doc = null;
                try
                {
                    byte[] inData = (byte[])node.ArrayValues[0];
                    byte[] outData = null;
                    if (inData.Length > 500)
                    {
                        doc = new XmlDocument();
                        using (MemoryStream inStream = new MemoryStream(inData))
                        {
                            doc.Load(inStream);
                        }

                        XmlNodeList list = doc.SelectNodes("/object/fld/sFld[@type='String']");
                        foreach (XmlNode xe in list)
                        {
                            if (xe is XmlElement)
                            {
                                string value = xe.InnerText.ToLower();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    xe.InnerText = LS.BinarySerialization.Replacer.LSBinarySerReplacer.RaiseModifyLoginEvent(value);
                                }
                            }
                        }

                        using (MemoryStream outStream = new MemoryStream())
                        {
                            doc.Save(outStream);
                            outData = outStream.GetBuffer();
                            Array.Resize<byte>(ref outData, (int)outStream.Length);
                        }

                        LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                        valueInfoEx.valueInfo = node.ValueTypes["0"];
                        valueInfoEx.value = outData;
                        valueInfoEx.objectName = node.Name;
                        valueInfoEx.memberName = "0";
                        valueInfos.Add(valueInfoEx);
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperWorkflowResource.LoadXmlContentError, ex);
                }
                finally
                {
                    if (doc != null)
                        doc.RemoveAll();
                }
            }
            
        }

        private void ReplaceWorkflowContext(LSObjectNode node)
        {
            foreach (string memberName in repDictionary.Keys)
            {
                if (!node.ValueTypes.ContainsKey(memberName))
                    continue;
                LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                valueInfoEx.valueInfo = node.ValueTypes[memberName];
                valueInfoEx.value = repDictionary[memberName];
                valueInfos.Add(valueInfoEx);
            }
        }

        private void ReplaceGuid(LSObjectNode node)
        {
            if (!repDictionary.ContainsKey(node.GuidInternalValue.ToString().ToUpper()))
                return;

            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            valueInfoEx.valueInfo = node.ValueTypes["0"];
            valueInfoEx.value = ((Guid)repDictionary[node.GuidInternalValue.ToString().ToUpper()]).ToByteArray();
            valueInfoEx.objectName = node.Name;
            valueInfoEx.memberName = "0";
            valueInfos.Add(valueInfoEx);
        }

        private void ReplaceCorrelationProperty(LSObjectNode node)
        {
            string key = (string)node.Members["name"];
            if(!repDictionary.ContainsKey(key))
                return;
            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            valueInfoEx.valueInfo = node.ValueTypes["value"];
            valueInfoEx.value = repDictionary[key];
            valueInfoEx.objectName = node.Name;
            valueInfoEx.memberName = key;
            valueInfos.Add(valueInfoEx);
        }

        private void ReplaceCompiledAssembly(LSObjectNode node)
        {
            if (node.Members["AssemblyName"] is InternalMemberValueE)
                return;
            string key = ((string)node.Members["AssemblyName"]).ToLower();
            if (repDictionary.ContainsKey(key))
            {
                LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                valueInfoEx.valueInfo = node.ValueTypes["AssemblyName"];
                valueInfoEx.value = repDictionary[key];
                valueInfoEx.objectName = node.Name;
                valueInfoEx.memberName = key;
                valueInfos.Add(valueInfoEx);
            }
            else
            {
                foreach (KeyValuePair<string, object> pair in repDictionary)
                {
                    if (key.StartsWith(pair.Key, StringComparison.Ordinal))
                    {
                        string dstAssmName = (string)pair.Value;
                        LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
                        valueInfoEx.valueInfo = node.ValueTypes["AssemblyName"];
                        valueInfoEx.value = key.Replace(pair.Key,dstAssmName);
                        valueInfoEx.objectName = node.Name;
                        valueInfoEx.memberName = key;
                        valueInfos.Add(valueInfoEx);
                        break;
                    }
                }
            }

        }


        #region Activity Members(DON'T TOUCH Functions in This Region)
        //private List<LS.BinarySerialization.Replacer.LSMemberDataInfo> memberKeys = new List<LS.BinarySerialization.Replacer.LSMemberDataInfo>();

        

        //private void ReplaceActivityMemberData(LSObjectNode node)
        //{
        //    if (!node.Name.Equals("",StringComparison.OrdinalIgnoreCase))
        //    {
        //        return;
        //    }
        //    if (ReplacedNodes.Contains(node.ObjectId))
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        ReplacedNodes.Add(node.ObjectId);
        //    }

        //    switch (node.Type)
        //    { 
        //        case InternalObjectTypeE.Array:
        //            ReplaceArrayData(node);
        //            break;
        //        case InternalObjectTypeE.Object:
        //            ReplaceObjectMemberData(node);
        //            break;
        //        case InternalObjectTypeE.Empty:
        //        default:
        //            break;
        //    }
        //}

        //private void ReplaceArrayData(LSObjectNode node)
        //{
        //    if (node.ArrayValues == null)
        //        return;
        //    StringBuilder fixupKey = new StringBuilder();
        //    foreach (Replacer.LSMemberDataInfo info in memberKeys)
        //    {
        //        if (node.ArrayValues.Length == info.Length)
        //        {
        //            try
        //            {
        //                if (!info.Profix.Equals(Replacer.LSBinarySerReplacer.ProfixOfActivityMember) && node.ArrayValues[info.Index] is InternalMemberValueE)
        //                {
        //                    ReplaceReferenceObjectValue(long.Parse(node.MemberRefs[info.Index.ToString()]), info);
        //                }
        //                else
        //                {
        //                    if (node.ArrayValues[info.Index].GetType() != info.OldValue.GetType())
        //                        continue;
        //                    if (node.ArrayValues[info.Index].Equals(info.OldValue))
        //                    {
        //                        LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
        //                        valueInfoEx.valueInfo = node.ValueTypes[info.Index.ToString()];
        //                        valueInfoEx.value = info.NewValue;
        //                        valueInfoEx.objectName = node.Name;
        //                        valueInfoEx.memberName = "";
        //                        valueInfos.Add(valueInfoEx);
        //                    }
        //                }
        //            }
        //            catch { }
        //        }
        //    }

        //    for (int i = 0; i < node.ArrayValues.Length; i++)
        //    {
        //        if (node.ArrayValues[i] == null)
        //            break;
        //        if (node.ArrayValues[i].GetType() != typeof(string))
        //            break;
        //        string key = (string)node.ArrayValues[i];

        //        string realKey = string.Empty;
        //        foreach (KeyValuePair<string, object> pair in repDictionary)
        //        {
        //            if (!pair.Key.StartsWith("LS"))
        //                continue;
        //            int index = pair.Key.IndexOf('.');

        //            if (index > 0)
        //            {
        //                string[] splitArray = pair.Key.Split(new char[] { '.' });
        //                string temp = splitArray[splitArray.Length - 1];
        //                string profix = splitArray[0];
        //                if (profix == Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty)
        //                    temp = "DependencyObject+dependencyPropertyValues";
        //                if (temp.Equals(key))
        //                {
        //                    LS.BinarySerialization.Replacer.LSMemberDataInfo info = (LS.BinarySerialization.Replacer.LSMemberDataInfo)repDictionary[pair.Key];
        //                    LS.BinarySerialization.Replacer.LSMemberDataInfo info2 = new LS.BinarySerialization.Replacer.LSMemberDataInfo(info.OldValue, info.NewValue, info.Profix, info.DependencyPropertyName);
        //                    info2.Length = node.ArrayValues.Length;
        //                    info2.Index = i;
        //                    memberKeys.Add(info2);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ReplaceArrayValue(LSObjectNode node)
        //{
        //    if (node.ArrayValues == null || node.ArrayValues.Length == 0)
        //    {
        //        return;
        //    }

        //    #region Keys
        //    for (int i = 0; i < node.ArrayValues.Length; i++)
        //    {
        //        if (node.ArrayValues[i] == null)
        //            break;
        //        if (node.ArrayValues[i].GetType() != typeof(string))
        //            break;


        //        string key = (string)node.ArrayValues[i];
        //        string realKey = string.Empty;
        //        foreach (KeyValuePair<string, object> pair in repDictionary)
        //        {
        //            if (!pair.Key.StartsWith("LS"))
        //                continue;
        //            int index = pair.Key.IndexOf('.');

        //            if (index > 0)
        //            {
        //                string[] splitArray = pair.Key.Split(new char[] { '.' });
        //                string temp = splitArray[splitArray.Length - 1];
        //                string profix = splitArray[0];
        //                if (profix == Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty)
        //                    temp = "DependencyObject+dependencyPropertyValues";
        //                if (temp.Equals(key))
        //                {
        //                    LS.BinarySerialization.Replacer.LSMemberDataInfo info = (LS.BinarySerialization.Replacer.LSMemberDataInfo)repDictionary[pair.Key];
        //                    LS.BinarySerialization.Replacer.LSMemberDataInfo info2 = new LS.BinarySerialization.Replacer.LSMemberDataInfo(info.OldValue, info.NewValue, info.Profix, info.DependencyPropertyName);
        //                    info2.Length = node.ArrayValues.Length;
        //                    info2.Index = i;
        //                    memberKeys.Add(info2);
        //                }
        //            }
        //        }
        //    }
        //    #endregion


        //}

        //private void ReplaceObjectMemberDatas(LSObjectNode node)
        //{
        //    if (node.Members == null || node.Members.Count == 0 || (!node.Members.ContainsKey("memberDatas") && !node.Members.ContainsKey("memberData")))
        //    {
        //        return;
        //    }

        //    if (ReplacedNodes.Contains(node.ObjectId))
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        ReplacedNodes.Add(node.ObjectId);
        //    }

        //    string memberNames = "memberNames";
        //    string memberDatas = "memberDatas";
        //    List<string> names = new List<string>();
        //    Dictionary<string, Replacer.LSMemberDataInfo> memberDataDic = GetMemberDataDictionary();

        //    if (!node.Members.ContainsKey(memberDatas))
        //    {
        //        ReplaceObjectMemberData(node);
        //        return;
        //    }

        //    #region Get Names
        //    if (node.Members.ContainsKey(memberNames))
        //    {
        //        object memberNameValue = node.Members[memberNames];
        //        if (node.MemberRefs.ContainsKey(memberNames))
        //        {
        //            long nameNodeId = long.Parse(node.MemberRefs[memberNames]);
        //            LSObjectNode nameNode = GetObjectNode(nameNodeId);
        //            if (nameNode.Type == InternalObjectTypeE.Array && nameNode.ArrayValues!=null && nameNode.ArrayValues.Length>0)
        //            {
        //                foreach (object name in nameNode.ArrayValues)
        //                {
        //                    try
        //                    {
        //                        if (name is string)
        //                        {
        //                            names.Add(name as string);
        //                        }
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            names.Add(memberNameValue as string);
        //        }
        //    }
        //    #endregion

        //    if (!node.MemberRefs.ContainsKey(memberDatas))
        //    {
        //        return;
        //    }

        //    long dataNodeId = long.Parse(node.MemberRefs[memberDatas]);
        //    LSObjectNode dataNode = GetObjectNode(dataNodeId);
        //    if (dataNode.Type == InternalObjectTypeE.Array && dataNode.ArrayValues != null && dataNode.ArrayValues.Length > 0)
        //    {
        //        for (int i = 0; i < dataNode.ArrayValues.Length; i++)
        //        {
        //            object data = dataNode.ArrayValues[i];
        //            if (data is InternalMemberValueE)
        //            {
        //                #region Data is a Reference
        //                long referenceNodeId=long.Parse(dataNode.MemberRefs[i.ToString()]);
        //                LSObjectNode referenceNode = GetObjectNode(referenceNodeId);
        //                if (referenceNode.Name == "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
        //                {
        //                    foreach (Replacer.LSMemberDataInfo info in memberDataDic.Values)
        //                    {
        //                        info.Index = i;
        //                        ReplaceDependencyObjectValue(referenceNodeId, info);
        //                    }
        //                }
        //                else if (referenceNode.Name == "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
        //                {
        //                    ReplaceObjectMemberDatas(referenceNode);
        //                }
        //                #endregion
        //            }
        //            else
        //            {
        //                #region Data is not a Reference
        //                foreach(Replacer.LSMemberDataInfo info in memberDataDic.Values)
        //                {
        //                    if (!info.DependencyPropertyName.Equals(names[i]))
        //                        continue;
        //                    if (dataNode.ArrayValues[i].GetType() != info.OldValue.GetType())
        //                        continue;
        //                    if (dataNode.ArrayValues[i].Equals(info.OldValue))
        //                    {
        //                        info.Index = i;
        //                        info.Length = dataNode.ArrayValues.Length;

        //                        LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
        //                        valueInfoEx.valueInfo = dataNode.ValueTypes[info.Index.ToString()];
        //                        valueInfoEx.value = info.NewValue;
        //                        valueInfoEx.objectName = dataNode.Name;
        //                        valueInfoEx.memberName = names[i];
        //                        valueInfos.Add(valueInfoEx);
        //                    }
        //                }
        //                #endregion
        //            }
        //        }
        //    }
        //}

        //private void ReplaceObjectMemberData(LSObjectNode node)
        //{
        //    if (!node.Members.ContainsKey("memberData"))
        //        return;

        //    string memberNames = "memberName";
        //    string memberDatas = "memberData";
        //    List<string> names = new List<string>();
        //    Dictionary<string, Replacer.LSMemberDataInfo> memberDataDic = GetMemberDataDictionary();
        //    bool hasName = false;

        //    #region Get Names
        //    if (node.Members.ContainsKey(memberNames))
        //    {
        //        object memberNameValue = node.Members[memberNames];
        //        if (node.MemberRefs.ContainsKey(memberNames))
        //        {
        //            long nameNodeId = long.Parse(node.MemberRefs[memberNames]);
        //            LSObjectNode nameNode = GetObjectNode(nameNodeId);
        //            if (nameNode.Type == InternalObjectTypeE.Array && nameNode.ArrayValues != null && nameNode.ArrayValues.Length > 0)
        //            {
        //                foreach (object name in nameNode.ArrayValues)
        //                {
        //                    try
        //                    {
        //                        if (name is string)
        //                        {
        //                            names.Add(name as string);
        //                        }
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            names.Add(memberNameValue as string);
        //        }
                
        //    }
        //    #endregion

        //    hasName = names.Count > 0;

        //    #region Member Data is single data
        //    if (!node.MemberRefs.ContainsKey(memberDatas))
        //    {
        //        if (!hasName || (hasName && memberDataDic.ContainsKey(Replacer.LSBinarySerReplacer.ProfixOfActivityMember + "." + names[0])))
        //        {
        //            foreach (Replacer.LSMemberDataInfo info in memberDataDic.Values)
        //            {
        //                if (hasName && !info.DependencyPropertyName.Equals(names[0]))
        //                    continue;
        //                if (node.Members[memberDatas].GetType() != info.OldValue.GetType())
        //                    continue;
        //                if (node.Members[memberDatas].Equals(info.OldValue))
        //                {
        //                    LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
        //                    valueInfoEx.valueInfo = node.ValueTypes[memberDatas];
        //                    valueInfoEx.value = info.NewValue;
        //                    valueInfoEx.objectName = node.Name;
        //                    valueInfoEx.memberName = memberDatas;
        //                    valueInfos.Add(valueInfoEx);
        //                }
        //            }

        //        }

        //        return;
        //    }
        //    else
        //    {
        //        #region Data is a Reference
        //        long referenceNodeId = long.Parse(node.MemberRefs[memberDatas]);
        //        LSObjectNode referenceNode = GetObjectNode(referenceNodeId);
        //        if (referenceNode.Name == "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
        //        {
        //            foreach (Replacer.LSMemberDataInfo info in memberDataDic.Values)
        //            {
        //                ReplaceDependencyObjectValue(referenceNodeId, info);
        //            }
        //        }
        //        else if (referenceNode.Name == "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
        //        {
        //            ReplaceObjectMemberDatas(referenceNode);
        //        }
        //        #endregion
        //    }
        //    #endregion
        //}

        //private void ReplaceReferenceObjectValue(long rootNodeId,Replacer.LSMemberDataInfo info)
        //{
        //    switch (info.Profix)
        //    { 
        //        case Replacer.LSBinarySerReplacer.ProfixOfDependencyProperty:
        //            ReplaceDependencyObjectValue(rootNodeId, info);
        //            break;
        //        case Replacer.LSBinarySerReplacer.ProfixOfSetVariable:
        //            ReplaceSetVariableActivityValue(rootNodeId, info);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void ReplaceSetVariableActivityValue(long rootObjectId, Replacer.LSMemberDataInfo info)
        //{
        //    try
        //    {
        //        LSObjectNode node1 = GetObjectNode(rootObjectId);
        //        if (node1 == null)
        //            return;
        //        if (node1.Name != "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
        //            return;
        //        if (!(node1.Members["memberDatas"] is InternalMemberValueE))
        //            return;

        //        long node2Id = long.Parse(node1.MemberRefs["memberDatas"]);
        //        LSObjectNode node2 = GetObjectNode(node2Id);
        //        if (node2 == null)
        //            return;
        //        if (node2.Name != "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
        //            return;
        //        if (!(node2.ArrayValues[1] is InternalMemberValueE))
        //            return;

        //        long dependNodeId = long.Parse(node2.MemberRefs["1"]);
        //        ReplaceDependencyObjectValue(dependNodeId, info);
        //    }
        //    catch { }
        //}

        //private void ReplaceDependencyObjectValue(long rootObjectId, Replacer.LSMemberDataInfo info)
        //{
        //    string propertyName = info.DependencyPropertyName;
        //    object oldValue = info.OldValue;
        //    object newValue = info.NewValue;
        //    try
        //    {
        //        LSObjectNode dependencyRootNode = GetObjectNode(rootObjectId);
        //        ///root node has member keys and values
        //        ///
        //        if (dependencyRootNode == null)
        //            return;
        //        if (dependencyRootNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
        //            return;
        //        if (!(dependencyRootNode.Members["keys"] is InternalMemberValueE))
        //            return;
        //        if (!(dependencyRootNode.Members["values"] is InternalMemberValueE))
        //            return;

        //        int propertyValueIndex = -1;

        //        #region Find Property Index in Array
        //        long keysNodeId = long.Parse(dependencyRootNode.MemberRefs["keys"]);
        //        LSObjectNode dependencyKeysNode = GetObjectNode(keysNodeId);
        //        if (dependencyKeysNode == null)
        //            return;
        //        if (dependencyKeysNode.Type != InternalObjectTypeE.Array)
        //            return;
        //        if (dependencyKeysNode.ArrayValues == null || dependencyKeysNode.ArrayValues.Length == 0)
        //            return;
        //        if (dependencyKeysNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
        //            return;
        //        for (int i=0;i<dependencyKeysNode.ArrayValues.Length;i++)
        //        {
        //            try
        //            {
        //                object o=dependencyKeysNode.ArrayValues[i];
        //                if (o is InternalMemberValueE)
        //                {
        //                    long nameNodeId = long.Parse(dependencyKeysNode.MemberRefs[i.ToString()]);
        //                    LSObjectNode nameNode = GetObjectNode(nameNodeId);
        //                    if (nameNode == null)
        //                        continue;
        //                    if (nameNode.Members != null && nameNode.Members.Count > 0 && nameNode.Members.ContainsKey("name") && !(nameNode.Members["name"] is InternalMemberValueE))
        //                    {
        //                        if (nameNode.Members["name"].ToString().Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            propertyValueIndex = i;
        //                            break;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (o.ToString().Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        propertyValueIndex = i;
        //                        break;
        //                    }
        //                }
        //            }
        //            catch { }
        //        }
        //        if (propertyValueIndex == -1)
        //            return;
        //        #endregion

        //        #region Replace Property Value in Array
        //        long valuesNodeId = long.Parse(dependencyRootNode.MemberRefs["values"]);
        //        LSObjectNode dependencyValuesNode = GetObjectNode(valuesNodeId);
        //        if (dependencyValuesNode == null)
        //            return;
        //        if (dependencyValuesNode.Type != InternalObjectTypeE.Array)
        //            return;
        //        if (dependencyValuesNode.ArrayValues == null || dependencyValuesNode.ArrayValues.Length == 0)
        //            return;
        //        if (dependencyValuesNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
        //            return;

        //        object propertyValue = dependencyValuesNode.ArrayValues[propertyValueIndex];
        //        if (propertyValue is InternalMemberValueE)
        //            return;
        //        if (propertyValue.GetType() != oldValue.GetType())
        //            return;
        //        if (propertyValue.Equals(oldValue))
        //        {
        //            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
        //            valueInfoEx.valueInfo = dependencyValuesNode.ValueTypes[propertyValueIndex.ToString()];
        //            valueInfoEx.value = newValue;
        //            valueInfoEx.objectName = dependencyValuesNode.Name;
        //            valueInfoEx.memberName = "";
        //            valueInfos.Add(valueInfoEx);
        //        }
        //        #endregion
        //    }
        //    catch { }
        //}


        private void ReplaceActivityData(object key, object value, LSObjectNodeValueExtensionEx infoExt)
        {
            if (value == null)
            {
                return;
            }

            #region Filter Out Replaced Node
            if (key is LSObjectNode)
            {
                LSObjectNode temp = (LSObjectNode)key;
                if (!ReplacedNodes.Contains(temp.ObjectId))
                {
                    ReplacedNodes.Add(temp.ObjectId);
                }
            }

            if (value is LSObjectNode)
            {
                LSObjectNode temp = (LSObjectNode)value;
                if (ReplacedNodes.Contains(temp.ObjectId))
                {
                    return;
                }

                if (key == null && temp.Type == InternalObjectTypeE.Array)
                {

                }
                else if (!ReplacedNodes.Contains(temp.ObjectId))
                {
                    ReplacedNodes.Add(temp.ObjectId);
                }
            }
            #endregion


            if (!(key is LSObjectNode))
            {
                if (value is LSObjectNode)
                {
                    LSObjectNode node = (LSObjectNode)value;
                    if (node.Type == InternalObjectTypeE.Array)
                    {
                        ReplaceArrayData(key, node);
                    }
                    else if (node.Type == InternalObjectTypeE.Object)
                    {
                        ReplaceObjectMembers(node);
                    }
                }
                else if (key != null)
                {
                    ReplacePrimitiveKeyValuePair(key, value,infoExt);
                }
            }
            else
            {
                #region Key is a object
                LSObjectNode keyNode = (LSObjectNode)key;
                if (keyNode.Type == InternalObjectTypeE.Array)
                {
                    if (keyNode.ArrayValues == null || keyNode.ArrayValues.Length == 0)
                    {
                        ReplaceArrayData(null, value);
                    }
                    else
                    {

                        for (int i = 0; i < keyNode.ArrayValues.Length; i++)
                        {
                            object subKey = keyNode.ArrayValues[i];
                            if (keyNode.MemberRefs.ContainsKey(i.ToString()))
                                subKey = GetObjectNode(long.Parse(keyNode.MemberRefs[i.ToString()]));


                            object subValue = value;
                            if ((value is LSObjectNode))
                            {

                                LSObjectNode valueNode = (LSObjectNode)value;
                                if (valueNode.Type == InternalObjectTypeE.Array && valueNode.ArrayValues != null && valueNode.ArrayValues.Length == keyNode.ArrayValues.Length)
                                {
                                    if (valueNode.MemberRefs.ContainsKey(i.ToString()))
                                    {
                                        subValue = GetObjectNode(long.Parse(valueNode.MemberRefs[i.ToString()]));
                                    }
                                    else
                                    {
                                        subValue = valueNode.ArrayValues[i];
                                    }
                                    if (subValue != null)
                                    {
                                        ReplaceActivityData(subKey, subValue, GetNewValueExtension(valueNode, i.ToString()));
                                    }
                                }
                            }
                        }

                    }
                }
                else if (keyNode.Type == InternalObjectTypeE.Object)
                {
                    //ReplaceObjectMembers(keyNode);
                    ReplaceArrayData(null, value);
                }

                #endregion
            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        private void ReplaceObjectMembers(LSObjectNode node)
        {
            if (node.Members == null || node.Members.Count == 0)
            {
                return;
            }

            
            if (node.Name == "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
            {
                ReplaceDependencyObjectValue(node);
            }
            else if (node.Name == "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
            {
                
                ReplaceKeyValueInMember(node, "memberName", "memberData");
                ReplaceKeyValueInMember(node, "memberNames", "memberDatas");
            }

            foreach (KeyValuePair<string, object> pair in node.Members)
            {
                object value = pair.Value;
                if (pair.Value is InternalMemberValueE)
                {
                    long refNodeId = long.Parse(node.MemberRefs[pair.Key]);
                    value = GetObjectNode(refNodeId);
                }

                if (value != null)
                {
                    ReplaceActivityData(pair.Key, value, GetNewValueExtension(node, pair.Key));
                }
            }
        }

        private void ReplaceKeyValueInMember(LSObjectNode node, string keyName, string valueName)
        {
            object key = null;
            object value = null;
            if (node.Members.ContainsKey(keyName))
            {
                key = node.Members[keyName];
                if (key is InternalMemberValueE)
                {
                    key = GetObjectNode(long.Parse((string)node.MemberRefs[keyName]));
                }
            }

            if (node.Members.ContainsKey(valueName))
            {
                value = node.Members[valueName];
                if (value is InternalMemberValueE)
                {
                    value = GetObjectNode(long.Parse((string)node.MemberRefs[valueName]));
                }
            }


            if (value == null)
            {
                return;
            }
            ReplaceActivityData(key, value, GetNewValueExtension(node, valueName));

            if (node.Members.ContainsKey(keyName))
            {
                node.Members.Remove(keyName);
            }
            if (node.Members.ContainsKey(valueName))
            {
                node.Members.Remove(valueName);
            }
            if (node.MemberRefs.ContainsKey(keyName))
            {
                node.MemberRefs.Remove(keyName);
            }
            if (node.MemberRefs.ContainsKey(valueName))
            {
                node.MemberRefs.Remove(valueName);
            }
        }

        private void ReplaceDependencyObjectValue(LSObjectNode dependencyRootNode)
        {
            try
            {
                ///root node has member keys and values
                ///
                if (dependencyRootNode == null)
                    return;
                if (dependencyRootNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
                    return;
                if (!(dependencyRootNode.Members["keys"] is InternalMemberValueE))
                    return;
                if (!(dependencyRootNode.Members["values"] is InternalMemberValueE))
                    return;


                #region Find Property Index in Array
                long keysNodeId = long.Parse(dependencyRootNode.MemberRefs["keys"]);
                LSObjectNode dependencyKeysNode = GetObjectNode(keysNodeId);
                if (dependencyKeysNode == null)
                    return;
                if (dependencyKeysNode.Type != InternalObjectTypeE.Array)
                    return;
                if (dependencyKeysNode.ArrayValues == null || dependencyKeysNode.ArrayValues.Length == 0)
                    return;
                if (dependencyKeysNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
                    return;


                long valuesNodeId = long.Parse(dependencyRootNode.MemberRefs["values"]);
                LSObjectNode dependencyValuesNode = GetObjectNode(valuesNodeId);
                if (dependencyValuesNode == null)
                    return;
                if (dependencyValuesNode.Type != InternalObjectTypeE.Array)
                    return;
                if (dependencyValuesNode.ArrayValues == null || dependencyValuesNode.ArrayValues.Length == 0)
                    return;
                if (dependencyValuesNode.Name != "System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef")
                    return;

                if (dependencyKeysNode.ArrayValues.Length != dependencyValuesNode.ArrayValues.Length)
                    return;

                if (ReplacedNodes.Contains(dependencyKeysNode.ObjectId))
                    return;
                else
                    ReplacedNodes.Add(dependencyKeysNode.ObjectId);
                if (ReplacedNodes.Contains(dependencyValuesNode.ObjectId))
                    return;
                else
                    ReplacedNodes.Add(dependencyValuesNode.ObjectId);


                for (int i = 0; i < dependencyKeysNode.ArrayValues.Length; i++)
                {
                    try
                    {
                        object o = dependencyKeysNode.ArrayValues[i];
                        if (o is InternalMemberValueE)
                        {
                            long nameNodeId = long.Parse(dependencyKeysNode.MemberRefs[i.ToString()]);
                            LSObjectNode nameNode = GetObjectNode(nameNodeId);
                            if (nameNode == null)
                                continue;
                            if (nameNode.Members != null && nameNode.Members.Count > 0 && nameNode.Members.ContainsKey("name") && !(nameNode.Members["name"] is InternalMemberValueE))
                            {
                                o = nameNode.Members["name"];
                            }
                        }

                        object p = dependencyValuesNode.ArrayValues[i];
                        if (p is InternalMemberValueE)
                        {
                            long valueNodeId = long.Parse(dependencyValuesNode.MemberRefs[i.ToString()]);
                            LSObjectNode valueNode = GetObjectNode(valueNodeId);
                            p = valueNode;
                        }

                        if (p != null)
                        {
                            ReplaceActivityData(o, p, GetNewValueExtension(dependencyValuesNode, i.ToString()));
                        }
                    }
                    catch(Exception e) 
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceDependencyObjectValueError, e.ToString());
                    }//need not log
                }
                #endregion
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ReplaceDependencyObjectValueError, e.ToString());
            }//need not log
            finally 
            {
                ArgumentCheck.NotNull(dependencyRootNode, nameof(dependencyRootNode));
                if (dependencyRootNode.Members.ContainsKey("keys"))
                    dependencyRootNode.Members.Remove("keys");
                if (dependencyRootNode.Members.ContainsKey("values"))
                    dependencyRootNode.Members.Remove("values");
                if (dependencyRootNode.MemberRefs.ContainsKey("keys"))
                    dependencyRootNode.MemberRefs.Remove("keys");
                if (dependencyRootNode.MemberRefs.ContainsKey("values"))
                    dependencyRootNode.MemberRefs.Remove("values");
            }
        }

        private void ReplaceArrayData(object key, object value)
        {
            if (value is LSObjectNode)
            {
                LSObjectNode temp = (LSObjectNode)value;
                if (temp.Type != InternalObjectTypeE.Array)
                    return;
                if (temp.ArrayValues == null || temp.ArrayValues.Length == 0)
                    return;

                for (int i = 0; i < temp.ArrayValues.Length; i++)
                {
                    if (key == null && !temp.MemberRefs.ContainsKey(i.ToString()))
                    {
                        continue;
                    }
                    object subValue=temp.ArrayValues[i];
                    if (temp.MemberRefs.ContainsKey(i.ToString()))
                    {
                        subValue = GetObjectNode(long.Parse(temp.MemberRefs[i.ToString()]));
                    }
                    if (subValue == null)
                        continue;
                    ReplaceActivityData(key, subValue, GetNewValueExtension(temp, i.ToString()));
                }
            }
        }

        private void ReplacePrimitiveKeyValuePair(object key, object value, LSObjectNodeValueExtensionEx infoExt)
        {
            if (key == null || value == null)
                return;
            if ((key is LSObjectNode) || (value is LSObjectNode))
                return;
            if (infoExt == null)
                return;

            object newValue = value;
            if(mListIdPropNames.Contains(key.ToString().ToLower()))
            {
                newValue = Replacer.LSBinarySerReplacer.RaiseModifyListIdEvent(value.ToString());

                if (!newValue.Equals(value))
                {
                    infoExt.value = newValue;
                    infoExt.memberName = key.ToString();
                    valueInfos.Add(infoExt);
                }
            }
            else if (mUserAndEmailPropNames.Contains(key.ToString().ToLower()))
            {
                if (value is string)
                {
                    newValue = Replacer.LSBinarySerReplacer.RaiseEmailAddressEvent(value.ToString());
                    if (newValue.Equals(value))
                    {
                        newValue = Replacer.LSBinarySerReplacer.RaiseModifyLoginEvent(value.ToString());
                    }

                    if (!newValue.Equals(value))
                    {
                        infoExt.value = newValue;
                        infoExt.memberName = key.ToString();
                        valueInfos.Add(infoExt);
                    }
                }
            }
            else if (key.ToString().Equals("contentTypeId", StringComparison.OrdinalIgnoreCase))
            {
                newValue = Replacer.LSBinarySerReplacer.RaiseModifyContentTypeIdEvent(value.ToString());
                if (!newValue.Equals(value))
                {
                    infoExt.value = newValue;
                    infoExt.memberName = key.ToString();
                    valueInfos.Add(infoExt);
                }
            }
            else if (mMemberDataInfoEx.DependencyPropertyNames.Contains(key.ToString().ToLower()))
            {
                #region Data is not a Reference
                foreach (Replacer.LSMemberDataInfo info in mMemberDataInfoEx.MemberDataInfoCollection.Values)
                {
                    if (!info.DependencyPropertyName.Equals(key.ToString(), StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (value.GetType() != info.OldValue.GetType())
                        continue;
                    if (value.Equals(info.OldValue))
                    {
                        infoExt.value = info.NewValue;
                        infoExt.memberName = key.ToString();
                        valueInfos.Add(infoExt);
                        break;
                    }
                }
                #endregion
            }

            
        }

        private LSObjectNode GetObjectNode(long id)
        {
            LSObjectNode node = null;
            if (mAnalyzeProc.ObjectIdToIndex.ContainsKey(id))
            {
                int index = mAnalyzeProc.ObjectIdToIndex[id];
                node = mAnalyzeProc.AllNodes[index];
            }
            else if (mAnalyzeProc.FieldNodes.ContainsKey(id))
            {
                node = mAnalyzeProc.FieldNodes[id];
            }
            return node;
        }

        private void GetMemberDataEx()
        {
            mMemberDataInfoEx = new Replacer.LSMemberDataInfoEx();
            foreach (KeyValuePair<string, object> pair in repDictionary)
            {
                if (pair.Key.StartsWith("LS",StringComparison.Ordinal))
                {
                    Replacer.LSMemberDataInfo temp = (Replacer.LSMemberDataInfo)pair.Value;
                    string propName = temp.DependencyPropertyName.ToLower();
                    mMemberDataInfoEx.MemberDataInfoCollection.Add(pair.Key, temp);

                    if (!mMemberDataInfoEx.DependencyPropertyNames.Contains(propName))
                    {
                        mMemberDataInfoEx.DependencyPropertyNames.Add(propName);
                    }
                }
            }
        }

        private LSObjectNodeValueExtensionEx GetNewValueExtension(LSObjectNode node, string valueTypeIndex)
        {
            if (node.ValueTypes == null)
                return null;
            if (!node.ValueTypes.ContainsKey(valueTypeIndex))
                return null;
            LSObjectNodeValueExtensionEx valueInfoEx = new LSObjectNodeValueExtensionEx();
            valueInfoEx.valueInfo = node.ValueTypes[valueTypeIndex];
            //valueInfoEx.value = info.NewValue;
            valueInfoEx.objectName = node.Name;
            //valueInfoEx.memberName = key.ToString();    
            return valueInfoEx;
        }
        #endregion


        private void ChangeValue(LSObjectNodeValueExtension valueInfo,object value)
        {
            byte[] rep = null;
            int oldLen = 0;
            switch (valueInfo.ValueType)
            { 
                case InternalPrimitiveTypeEx.ByteArray:
                    rep = (byte[])value;
                    oldLen = BitConverter.ToInt32(rawData, (int)(valueInfo.Position + 5));
                    byte[] len = BitConverter.GetBytes(rep.Length);
                    rawData = LSUtilityOfBytes.LSReplaceBytes(rawData, (int)(valueInfo.Position + 5), 4, len);
                    break;
                case InternalPrimitiveTypeEx.Boolean:
                    rep = BitConverter.GetBytes((bool)value);
                    oldLen = 1;
                    break;
                case InternalPrimitiveTypeEx.Byte:
                    rep = new byte[1] { (byte)value};
                    oldLen = 1;
                    break;
                case InternalPrimitiveTypeEx.Char:
                    rep = BitConverter.GetBytes((char)value);
                    oldLen = 2;
                    break;
                case InternalPrimitiveTypeEx.DateTime:
                    rep = BitConverter.GetBytes(((DateTime)value).ToBinary());
                    oldLen = 8;
                    break;
                case InternalPrimitiveTypeEx.Double:
                    rep = BitConverter.GetBytes((double)value);
                    oldLen = 8;
                    break;
                case InternalPrimitiveTypeEx.Int16:
                    rep = BitConverter.GetBytes((short)value);
                    oldLen = 2;
                    break;
                case InternalPrimitiveTypeEx.Int32:
                    rep = BitConverter.GetBytes((int)value);
                    oldLen = 4;
                    break;
                case InternalPrimitiveTypeEx.Int64:
                    rep = BitConverter.GetBytes((long)value);
                    oldLen = 8;
                    break;
                case InternalPrimitiveTypeEx.SByte:
                    rep = new byte[1] { (byte)value};
                    oldLen = 1;
                    break;
                case InternalPrimitiveTypeEx.Single:
                    rep = BitConverter.GetBytes((float)value);
                    oldLen = 4;
                    break;
                case InternalPrimitiveTypeEx.TimeSpan:
                    rep = BitConverter.GetBytes((long)value);
                    oldLen = 8;
                    break;
                case InternalPrimitiveTypeEx.UInt16:
                    rep = BitConverter.GetBytes((UInt16)value);
                    oldLen = 2;
                    break;
                case InternalPrimitiveTypeEx.UInt32:
                    rep = BitConverter.GetBytes((UInt32)value);
                    oldLen = 4;
                    break;
                case InternalPrimitiveTypeEx.UInt64:
                    rep = BitConverter.GetBytes((UInt64)value);
                    oldLen = 8;
                    break;
                case InternalPrimitiveTypeEx.ObjectString:
                case InternalPrimitiveTypeEx.String:
                    string strValue = (string)value;
                    oldLen = (int)rawData[(int)valueInfo.PhysicalPosition];
                    if (oldLen < 128)
                    {
                        oldLen += 1;
                    }
                    else
                    {
                        oldLen = (int)rawData[(int)valueInfo.PhysicalPosition + 1] * 128 + (int)rawData[(int)valueInfo.PhysicalPosition] - 128;
                        oldLen += 2;
                    }
                    byte[] tempValue = System.Text.Encoding.UTF8.GetBytes(ChangeStringValue(strValue));
                    int newLen = tempValue.Length;
                    rep = new byte[0];
                    if (newLen < 128)
                    {
                        Array.Resize<byte>(ref rep, 1);
                        rep[0] = (byte)newLen;

                        newLen += 1;
                    }
                    else
                    {
                        Array.Resize<byte>(ref rep, 2);
                        rep[1] = (byte)(newLen / 128);
                        rep[0] = (byte)(newLen - 128 * rep[1] + 128);
                        newLen += 2;
                    }
                    LSUtilityOfBytes.LSAppendBytes(ref rep, tempValue, 0, tempValue.Length);
                    break;
                case InternalPrimitiveTypeEx.Currency:
                case InternalPrimitiveTypeEx.Decimal:
                case InternalPrimitiveTypeEx.Class:
                case InternalPrimitiveTypeEx.CrossAppDomainString:
                case InternalPrimitiveTypeEx.MemberNested:
                case InternalPrimitiveTypeEx.MemberReference:
                case InternalPrimitiveTypeEx.Null:
                case InternalPrimitiveTypeEx.Invalid:
                    return;
                default:
                    throw new Exception("Invalid Type");
                    break;
            }
            
            rawData = LSUtilityOfBytes.LSReplaceBytes(rawData, (int)valueInfo.PhysicalPosition, oldLen, rep);
        }

        private string ChangeStringValue(string inString)
        {
            return inString;
        }
    }

    internal class ValuePositionIcp : IComparer<LSObjectNodeValueExtensionEx>
    {
        public int Compare(LSObjectNodeValueExtensionEx a, LSObjectNodeValueExtensionEx b)
        {
            if(a.valueInfo.PhysicalPosition==b.valueInfo.PhysicalPosition)
                return (int)(a.valueInfo.Position-b.valueInfo.Position);
            else
                return (int)(a.valueInfo.PhysicalPosition-b.valueInfo.PhysicalPosition);
        }
    }

    internal class LSObjectNodeValueExtensionEx
    {
        internal LSObjectNodeValueExtension valueInfo;
        internal object value;

        internal string objectName;
        internal string memberName;
    }
}
