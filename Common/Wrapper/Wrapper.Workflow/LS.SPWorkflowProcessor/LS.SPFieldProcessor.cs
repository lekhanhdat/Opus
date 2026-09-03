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

using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
namespace LS.SPWorkflowProcessor
{
    public enum SPFieldProcessorScope
    {
        List,
        Web
    }

    public class AveSPField
    {
        #region Serializable Data
        //public Guid mSrcId;
        //public Guid mDstId;
        //public string mSrcInternalName;
        //public string mDstInternalName;
        //public string mSrcDisplayName;
        //public string mDstDisplayName;
        //public string mSrcType;
        //public string mDstType;

        //public string mSrcColName;
        //public string mDstColName;
        //public string mSrcColName2;
        //public string mDstColName2;


        //public bool mRequired;
        //public string mSrcSchemaXMLString;

        private SPFieldSerializableData mSerializableData = null;
        public SPFieldSerializableData SerializableData
        {
            get
            {
                if (mSerializableData == null)
                    mSerializableData = new SPFieldSerializableData();
                return mSerializableData;
            }
            set
            {
                mSerializableData = value;
            }
        }
        #endregion


        public XmlElement mSrcSchemaElement;
        public AveFieldType mType;
        public IAveField mDstSPField;


        public IAveField SPFieldInternal
        {
            get { return mDstSPField; }
        }


        public AveSPField(string schemaXML)
        {
            XmlDocument doc = null;
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("<Root>");
                builder.Append(schemaXML);
                builder.Append("</Root>");
                doc = new XmlDocument();
                doc.LoadXml(builder.ToString());
                XmlElement fieldElement = (XmlElement)doc.FirstChild.ChildNodes[0];

                this.SerializableData.mSrcId = new Guid(fieldElement.GetAttribute("ID"));
                this.SerializableData.mSrcInternalName = fieldElement.GetAttribute("Name");
                if (fieldElement.HasAttribute("DisplayName"))
                    this.SerializableData.mSrcDisplayName = fieldElement.GetAttribute("DisplayName");
                if (fieldElement.HasAttribute("Type"))
                    this.SerializableData.mSrcType = fieldElement.GetAttribute("Type");
                if (fieldElement.HasAttribute("ColName"))
                    this.SerializableData.mSrcColName = fieldElement.GetAttribute("ColName");
                if (fieldElement.HasAttribute("ColName2"))
                    this.SerializableData.mSrcColName2 = fieldElement.GetAttribute("ColName2");

                this.SerializableData.mSrcSchemaXMLString = schemaXML;
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
            }
        }

        public AveSPField(XmlElement fieldElement)
        {
            this.SerializableData.mSrcId = new Guid(fieldElement.GetAttribute("ID"));
            this.SerializableData.mSrcInternalName = fieldElement.GetAttribute("Name");
            if (fieldElement.HasAttribute("DisplayName"))
                this.SerializableData.mSrcDisplayName = fieldElement.GetAttribute("DisplayName");
            if (fieldElement.HasAttribute("Type"))
                this.SerializableData.mSrcType = fieldElement.GetAttribute("Type");
            if (fieldElement.HasAttribute("ColName"))
                this.SerializableData.mSrcColName = fieldElement.GetAttribute("ColName");
            if (fieldElement.HasAttribute("ColName2"))
                this.SerializableData.mSrcColName2 = fieldElement.GetAttribute("ColName2");

            mSrcSchemaElement = fieldElement;
            this.SerializableData.mSrcSchemaXMLString = fieldElement.OuterXml;
        }

        public AveSPField(SPFieldSerializableData data)
        {
            mSerializableData = data;
        }

        public void SetAveFieldBySPField(IAveField spField)
        {
            if (spField != null)
            {
                this.SerializableData.mDstId = spField.ID;
                this.SerializableData.mDstInternalName = spField.InternalName;
                this.SerializableData.mDstDisplayName = spField.Title;
                this.SerializableData.mDstType = spField.TypeAsString;
                mType = spField.Type;
                this.SerializableData.mRequired = spField.Required;
                this.SerializableData.mDstColName = GetColNameFromSchema("ColName", spField.SchemaXml);
                if (mType == AveFieldType.URL)
                    this.SerializableData.mDstColName2 = GetColNameFromSchema("ColName2", spField.SchemaXml);
                mDstSPField = spField;

            }
        }

        internal static string GetColNameFromSchema(string colName, string schema)
        {
            string rlt = string.Empty;
            string profix1 = colName + "=\"";
            string profix2 = "\"";
            int index1 = schema.IndexOf(profix1, StringComparison.Ordinal);
            if (index1 >= 0)
            {
                int index2 = schema.IndexOf(profix2, index1 + profix1.Length, StringComparison.Ordinal);
                if (index2 > 0)
                {
                    rlt = schema.Substring(index1 + profix1.Length, index2 - index1 - profix1.Length);
                }
            }
            return rlt;
        }
    }

    public class AveSPFieldCollection
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string,AveSPField> mAveFields;
        private Dictionary<Guid, AveSPField> mAveFieldIdDic;
        private string mSrcSchema;
        private IAveFieldCollection mDstCollection = null;

        public IAveFieldCollection CurrentFieldCollection
        {
            get { return mDstCollection; }
            set { mDstCollection = value; }
        }


        public int Count
        {
            get
            {
                return mAveFields.Count;
            }
        }

        public AveSPFieldCollection()
        {
            mAveFields = new Dictionary<string,AveSPField>();
            mAveFieldIdDic = new Dictionary<Guid, AveSPField>();
        }

        public void Dispose()
        {
            mAveFields.Clear();
            mAveFieldIdDic.Clear();
        }

        public AveSPField this[string internalName]
        {
            get
            { 
                if(mAveFields.ContainsKey(internalName))
                    return mAveFields[internalName];
                else
                    return null;
            }
        }

        public AveSPField this[Guid id]
        {
            get
            {
                if (mAveFieldIdDic.ContainsKey(id))
                    return mAveFieldIdDic[id];
                else
                    return null;
            }
        }

        public void Initialize(string srcSchema, IAveFieldCollection dstFields,bool createIfNotExist)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "AveSPFieldCollection Initialize");

            mSrcSchema = srcSchema;
            mDstCollection = dstFields;


            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                if (string.IsNullOrEmpty(srcSchema)) 
                {
                    return;
                }
                doc.LoadXml(srcSchema);
                List<AveSPField> unRestoredFields = new List<AveSPField>();//retry to restore
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if (node is XmlElement)
                    {
                        XmlElement fieldElement = (XmlElement)node;
                        try
                        {
                            string fieldType = fieldElement.Attributes["Type"].Value as string;
                            if(!string.IsNullOrEmpty(fieldType) && fieldType.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }
                        catch(Exception e)  
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetWorkflowFieldTypeError, e.ToString());
                        }
                        AveSPField aveField = new AveSPField(fieldElement);

                        IAveField spField = null;
                        object fieldObj = dstFields.GetFieldById(aveField.SerializableData.mSrcId,false); 
                        if (fieldObj == null)
                        {
                            fieldObj = dstFields.GetFieldByInternalName(aveField.SerializableData.mSrcInternalName, false);// LSInvoker.CallMethod(dstFields, "GetFieldByInternalName", new Type[] { typeof(string), typeof(bool) }, new object[] { aveField.SerializableData.mSrcInternalName, false });
                            if (fieldObj != null)
                            {
                                spField = (IAveField)fieldObj;
                                if (!IsCompatibleFieldType(aveField.SerializableData.mSrcType, spField.TypeAsString))
                                    spField = null;
                            }
                        }
                        else
                            spField = (IAveField)fieldObj;
                        if (fieldObj == null && createIfNotExist)
                        {
                            try
                            {
                                spField = CreateSPFieldForAveField(aveField);
                            }
                            catch(Exception ex)
                            {
                                unRestoredFields.Add(aveField);
                                log.Log(AveLogLevel.WARN, WrapperWorkflowResource.FieldCreatedFailed, ex);
                                continue;
                            }
                        }
                        aveField.SetAveFieldBySPField(spField);
                        mAveFields.Add(aveField.SerializableData.mSrcInternalName, aveField);
                        mAveFieldIdDic.Add(aveField.SerializableData.mSrcId, aveField);
                    }
                }
                foreach (AveSPField field in unRestoredFields)
                {
                    IAveField spField = null;
                    try
                    {
                        spField = CreateSPFieldForAveField(field);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(string.Format("Retry to create SPField failed, InternalName:{0}, DisplayName:{1}, message:{2}",
                            field.SerializableData.mSrcInternalName, field.SerializableData.mSrcDisplayName, ex));
                    }
                    field.SetAveFieldBySPField(spField);
                    mAveFields.Add(field.SerializableData.mSrcInternalName, field);
                    mAveFieldIdDic.Add(field.SerializableData.mSrcId, field);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.FLD_CollectionInitializeException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.FieldProcessorInitializeError,e);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "AveSPFieldCollection Initialize");
            }

            
        }

        private bool IsCompatibleFieldType(string type1, string type2)
        {
            if (type1 == type2)
                return true;
            else
                return false;
        }

        public AveSPField GetAveFieldByInternalName(string internalName)
        {
            AveSPField aveField = mAveFields[internalName];
            if (aveField == null)
                return null;
            if (aveField.SPFieldInternal != null)
                return mAveFields[internalName];
            else
                return null;
        }

        public AveSPField GetAveFieldById(Guid id)
        {
            AveSPField aveField = mAveFieldIdDic[id];
            if (aveField == null)
                return null;
            if (aveField.SPFieldInternal != null)
                return mAveFieldIdDic[id];
            else
                return null;
        }

        public Guid GetMappingId(Guid sourceId)
        {
            AveSPField aveField = GetAveFieldById(sourceId);
            if (aveField == null)
                return Guid.Empty;
            else
                return aveField.SerializableData.mDstId;
        }

        public AveSPField Add(string internalName)
        { 
            AveSPField aveField=mAveFields[internalName];
            if (aveField == null)
                return null;
            if (aveField.SPFieldInternal == null)
            {
                IAveField spField = CreateSPFieldForAveField(aveField);
                if (spField != null)
                {
                    aveField.SetAveFieldBySPField(spField);
                }
                else
                    return null;
            }
            return aveField;
        }

        public AveSPField Add(Guid id)
        {
            AveSPField aveField = mAveFieldIdDic[id];
            if (aveField == null)
                return null;
            return Add(aveField.SerializableData.mSrcInternalName);
        }

        public IAveField CreateSPField(string schemaXMLString, bool addToDefaultView, AveAddFieldOptions op)
        { 
            XmlDocument doc = null;
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("<Root>");
                builder.Append(schemaXMLString);
                builder.Append("</Root>");
                doc = new XmlDocument();
                doc.LoadXml(builder.ToString());
                XmlElement fieldElement = (XmlElement)doc.FirstChild.ChildNodes[0];
                return CreateSPField(fieldElement,addToDefaultView,op);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
            }
        }

        public IAveField CreateSPField(XmlElement schemaXML)
        {
            return CreateSPField(schemaXML, false, AveAddFieldOptions.DefaultValue);
        }

        public IAveField CreateSPField(XmlElement schemaXML,bool addToDefaultView,AveAddFieldOptions op)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "CreateSPField");
            string origDisplayName = schemaXML.GetAttribute("DisplayName");
            string origName = schemaXML.GetAttribute("Name");
            schemaXML.SetAttribute("DisplayName", origName);
            schemaXML.RemoveAttribute("Name");
            schemaXML.RemoveAttribute("StaticName");
            SPWorkflowProcessorRuntime.Log(Logs.FLD_FieldAttribute, "DisplayName", origDisplayName);
            SPWorkflowProcessorRuntime.Log(Logs.FLD_FieldAttribute, "Name", origName);
            //switch (schemaXML.GetAttribute("Type"))
            //{
            //    case "Calculated":
            //        break;
            //    default:
            //        break;
            //}

            try
            {
                IAveField spField = mDstCollection.AddFieldAsXml(schemaXML.OuterXml, addToDefaultView, op);
                if (origDisplayName != origName)
                {
                    spField.Title = origDisplayName;
                    spField.Update();
                }
                return spField;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.FLD_CreateFieldException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.CannotCreateListField, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "CreateSPField");
            }
        }

        private IAveField CreateSPFieldForAveField(AveSPField aveField)
        {
            return CreateSPField(aveField.mSrcSchemaElement);
        }

        public void Output(string outputFile)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(outputFile, true);
                sw.WriteLine("*********Count:" + mAveFields.Count.ToString() + "*********");
                foreach (KeyValuePair<string, AveSPField> pair in mAveFields)
                {
                    string temp = pair.Value.SerializableData.mSrcInternalName;
                    if (pair.Value.SerializableData.mDstInternalName != null)
                        temp += ":" + pair.Value.SerializableData.mDstInternalName;
                    if (pair.Value.SPFieldInternal != null)
                        temp += ":" + pair.Value.SPFieldInternal.InternalName;
                    if (pair.Value.SerializableData.mDstColName != null)
                        temp += ":" + pair.Value.SerializableData.mDstColName;
                    if (pair.Value.SerializableData.mDstColName2 != null)
                        temp += ":" + pair.Value.SerializableData.mDstColName2;
                    sw.WriteLine(temp);
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.WriteToFileError, e.ToString());
            }//need not to log
            finally 
            {
                if (sw != null)
                    sw.Close();
            }
        }
    }

    public class SPFieldProcessor
    {
        private Dictionary<Guid, Dictionary<string, string>> mDBFieldToSPFieldDic;

        private AveSPFieldCollection mAveFieldCollection;
        public AveSPFieldCollection AveFieldCollection
        {
            get { return mAveFieldCollection; }
        }

        private SPFieldProcessorScope mScope;
        public SPFieldProcessor(SPFieldProcessorScope scope)
        {
            mScope = scope;
            if (scope == SPFieldProcessorScope.List)
            {
                mAveFieldCollection = new AveSPFieldCollection();
            }
            else
                mDBFieldToSPFieldDic = new Dictionary<Guid, Dictionary<string, string>>();
        }

        public void Dispose()
        {
            if (mScope == SPFieldProcessorScope.List)
            {
                mAveFieldCollection.Dispose();
            }

            if (mScope == SPFieldProcessorScope.Web)
            {
                mDBFieldToSPFieldDic.Clear();
                mDBFieldToSPFieldDic = null;
            }
        }


        #region ************************Backup  Region************************
        public Dictionary<string, string> GetDBFieldToSPFieldDic(IAveList list)
        {
            if (!mDBFieldToSPFieldDic.ContainsKey(list.ID))
            {
                mDBFieldToSPFieldDic.Add(list.ID, GetDBFieldToSPFieldDic(list.Fields.SchemaXml));
            }
            return mDBFieldToSPFieldDic[list.ID];


        }

        private Dictionary<string, string> GetDBFieldToSPFieldDic(string schema)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetDBFieldToSPFieldDictionary");
            XmlDocument doc = null;
            Dictionary<string, string> dic = null;
            try
            {
                dic = new Dictionary<string, string>();
                doc = new XmlDocument();
                doc.LoadXml(schema);
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if ((node is XmlElement) && node.Name == "Field")
                    {
                        XmlElement field = (XmlElement)node;
                        if (!field.HasAttribute("Name") || !field.HasAttribute("ColName"))
                            continue;
                        int rowOrdinal = 0;
                        if (field.HasAttribute("RowOrdinal"))
                            rowOrdinal = int.Parse(field.GetAttribute("RowOrdinal"));

                        string name=field.GetAttribute("Name");
                        string colName=field.GetAttribute("ColName").ToLower();
                        dic.Add(rowOrdinal.ToString() + "_" + colName,name);

                        int index = 1;
                        while (true)
                        {
                            index++;
                            colName = field.GetAttribute("ColName" + index.ToString());
                            if (string.IsNullOrEmpty(colName))
                                break;
                            dic.Add(rowOrdinal.ToString() + "_" + colName, name);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.FLD_ConvertDBFieldException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.DBFieldToSPFieldError,e);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
                doc = null;
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetDBFieldToSPFieldDictionary");
            }

            return dic;
        }
        #endregion


        #region ************************Restore Region************************

        public void InitializeAveFieldCollection(string schema, IAveFieldCollection fieldCollection,bool createIfNotExist)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "InitializeAveFieldCollection");
            if (mAveFieldCollection.Count == 0)
                mAveFieldCollection.Initialize(schema, fieldCollection, createIfNotExist);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "InitializeAveFieldCollection");
        }

        public AveSPField GetOrCreateAveField(string internalName)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GetOrCreateAveField");
            AveSPField aveField = mAveFieldCollection.GetAveFieldByInternalName(internalName);
            if (aveField == null)
                aveField = mAveFieldCollection.Add(internalName);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GetOrCreateAveField");
            return aveField;
        }

        public void ConvertPropsToDBField(Hashtable props, string srcSchemaXML, IAveList dstList,bool isNewItem)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ConvertPropsToDBField");

            Hashtable cloneProps = (Hashtable)props.Clone();
            props.Clear();
            foreach (DictionaryEntry de in cloneProps)
            {
                try
                {
                    string key = de.Key.ToString();
                    int index2 = key.IndexOf('_', 1);
                    int colIndex = int.Parse(key.Substring(1, index2 - 1));
                    string colIndexStr = string.Empty;
                    string internalName = key.Substring(index2 + 1);
                    object value = de.Value;

                    SPWorkflowProcessorRuntime.Log(Logs.FLD_Property, key);
                    StringBuilder realName = new StringBuilder();
                    if (key[0] != '_')
                    {
                        realName.Append("#");
                        realName.Append(internalName);
                    }
                    else
                    {
                        AveSPField aveField = this.mAveFieldCollection.GetAveFieldByInternalName(internalName);
                        if (aveField == null)
                        {
                            aveField = this.mAveFieldCollection.Add(internalName);
                        }
                        if (aveField == null)
                            continue;

                        if (aveField.mType == AveFieldType.Calculated || aveField.mType == AveFieldType.Computed)
                            continue;

                        if (colIndex != 0)
                            colIndexStr = colIndex.ToString();


                        string colName = string.Empty;
                        if (colIndex == 0)
                            colName = aveField.SerializableData.mDstColName;
                        else if (colIndex == 2)
                            colName = aveField.SerializableData.mDstColName2;
                        if (string.IsNullOrEmpty(colName))
                            continue;



                        if (isNewItem && aveField.SerializableData.mRequired && aveField.mType!= AveFieldType.DateTime)
                        {
                            if (colIndex != 0)
                                continue;
                            realName.Append("_");
                            realName.Append(internalName);
                            if (aveField.mType == AveFieldType.URL)
                            {
                                string first = de.Value.ToString();
                                string second = first;
                                if (cloneProps.ContainsKey("_2_" + internalName))
                                    second = cloneProps["_2_" + internalName].ToString();
                                value = first + ", " + second;
                            }
                        }
                        else
                        {
                            realName.Append("#");
                            realName.Append(colName);
                        }
                    }
                    props.Add(realName.ToString(), value);
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.FLD_ConvertPropsException, de.Key.ToString(), e.Message);
                }
            }

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ConvertPropsToDBField");
        }
        #endregion
    }
}
