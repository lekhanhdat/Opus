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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    #endregion

    /// <summary>
    /// This class is used as a common utility 
    /// </summary>
    public class ReflectHelper
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static List<Type> primaryTypeList = new List<Type>();

        /// <summary>
        /// This section is to handle the dot net primary base type
        /// </summary>
        static ReflectHelper()
        {
            primaryTypeList.Add(typeof(Int16));
            primaryTypeList.Add(typeof(Int32));
            primaryTypeList.Add(typeof(Int64));
            primaryTypeList.Add(typeof(UInt16));
            primaryTypeList.Add(typeof(UInt32));
            primaryTypeList.Add(typeof(UInt64));
            primaryTypeList.Add(typeof(SByte));
            primaryTypeList.Add(typeof(Byte));
            primaryTypeList.Add(typeof(Decimal));
            primaryTypeList.Add(typeof(Double));
            primaryTypeList.Add(typeof(Single));
            primaryTypeList.Add(typeof(Enum));
            primaryTypeList.Add(typeof(Boolean));
            primaryTypeList.Add(typeof(DateTime));
            primaryTypeList.Add(typeof(String));
            primaryTypeList.Add(typeof(Object));
        }

        /// <summary>
        /// to add a automatic increments field to data table
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <returns>return Data table added identity id </returns>
        public static DataTable AddIdentityColumn(DataTable dataTable)
        {
            if (!dataTable.Columns.Contains("identityId"))
            {
                dataTable.Columns.Add("identityId");
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    dataTable.Rows[i]["identityId"] = (i + 1).ToString();
                }
            }
            return dataTable;
        }

        /// <summary>
        /// check if the data table has rows 
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <returns></returns>
        public static bool IsHaveRows(DataTable dataTable)
        {
            return (dataTable != null && dataTable.Rows.Count > 0);
        }

        /// <summary>
        /// Convert Data table to a list of Entity
        /// </summary>
        /// <typeparam name="T">entity T </typeparam>
        /// <param name="dataTable">data table</param>
        /// <returns>the generic list result</returns>
        public static List<T> DataTableToList<T>(DataTable dataTable)
            where T : class
        {
            var list = new List<T>();
            if (IsHaveRows(dataTable))
            {
                var model = default(T);
                ConvertToList<DataRow>(dataTable.Rows).ForEach(row =>
                {
                    model = Activator.CreateInstance<T>();
                    ConvertToList<DataColumn>(dataTable.Columns).ForEach(column =>
                    {
                        var value = row[column.ColumnName];
                        var info = model.GetType().GetProperty(column.ColumnName);
                        if (info != null && info.CanWrite && !Validator.IsNullOrEmpty(value))
                        {
                            if (info.PropertyType.IsEnum)
                            {
                                value = Enum.Parse(info.PropertyType, value.ToString());
                            }
                            else
                            {
                                if (primaryTypeList.Contains(info.PropertyType))
                                    value = Convert.ChangeType(value, info.PropertyType);
                                else value = SerializerHelper.DeserializeFromXmlString(value.ToString(), info.PropertyType);
                            }
                            info.SetValue(model, value, null);
                        }
                    });
                    list.Add(model);
                });
            }
            return list;
        }

        /// <summary>
        /// convert entity list to DataTable
        /// </summary>
        /// <typeparam name="T">entity</typeparam>
        /// <param name="list"> entity list</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> list)
            where T : class
        {
            var dataTable = new DataTable(typeof(T).Name);
            if (list != null && list.Count > 0)
            {
                var propertyInfoArray = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var createColumn = true;
                list.ForEach(obj =>
                {
                    if (obj != null)
                    {
                        var row = dataTable.NewRow();
                        Array.ForEach<PropertyInfo>(propertyInfoArray,
                           info =>
                           {
                               if (createColumn)
                                   dataTable.Columns.Add(new DataColumn(info.Name, info.PropertyType));
                               row[info.Name] = info.GetValue(obj, null);
                           });

                        if (createColumn)
                            createColumn = false;

                        dataTable.Rows.Add(row);
                    }
                });
            }
            return dataTable;
        }

        /// <summary>
        /// this method only support that the simple xml element with primary type
        /// For example <unit createDate="2009-12-28 09:51:21" howLong="30" name="Moss 2007 Lotus Notes" quantity="1" quantityUnit="GB" /> 
        /// </summary>
        /// <typeparam name="T">the type will  convert to</typeparam>
        /// <param name="dataElement">xml element which contains the data</param>
        /// <returns>converted object</returns>
        public static List<T> ConvertXmlElementToListObject<T>(XmlElement dataElement)
            where T : class, new()
        {
            var result = new List<T>();
            var xmlElementList = ConvertToList<XmlElement>(dataElement.ChildNodes);
            xmlElementList.ForEach(element => { result.Add(ConvertXmlElementToObject<T>(element)); });
            return result;
        }
        /// <summary>
        /// this method only support that the simple xml element with primary type
        /// For example <unit createDate="2009-12-28 09:51:21" howLong="30" name="Moss 2007 Lotus Notes" quantity="1" quantityUnit="GB" /> 
        ///
        /// This method is rewrite to supporting more complex object.
        /// 
        /// </summary>
        /// <typeparam name="T">the type will  convert to</typeparam>
        /// <param name="dataElement">xml element which contains the data</param>
        /// <returns>converted object</returns>
        public static T ConvertXmlElementToObject<T>(XmlElement dataElement)
            where T : class, new()
        {
            var result = default(T);
            if (dataElement != null)
            {
                result = Activator.CreateInstance<T>();
                Array.ForEach<PropertyInfo>(typeof(T).GetProperties(), info =>
                {
                    try
                    {
                        if (info.CanWrite)
                        {
                            Object value = GerneratePropertyValue(dataElement, info);
                            if (!Validator.IsNullOrEmpty(value))
                                info.SetValue(result, value, null);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"error happend ConvertXmlElementToObject,e:{e}");
                    }
                });
            }
            return result;
        }

        /// <summary>
        /// The method is to handle the complex type 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="dataElement"></param>
        /// <returns></returns>
        static Object GetComplexTypeValue(PropertyInfo info, XmlElement dataElement)
        {
            Object result = default(Object);
            try
            {
                XmlNodeList nodeList = dataElement.GetElementsByTagName(info.Name);
                if (nodeList != null
                    && nodeList.Count > 0)
                {
                    if (info.PropertyType.IsArray)
                    {
                        Type arrayElementType = info.PropertyType.GetElementType();
                        var closeGenericListType = typeof(List<>).MakeGenericType(arrayElementType);
                        var closeGenericListInstance = Activator.CreateInstance(closeGenericListType);

                        var elementList = ConvertToList<XmlElement>(nodeList);
                        elementList.ForEach(innerDataElement =>
                        {
                            if (innerDataElement != null)
                            {
                                Object elementTypeInstance = Activator.CreateInstance(arrayElementType);
                                Array.ForEach<PropertyInfo>(arrayElementType.GetProperties(),
                                    innerInfo =>
                                    {
                                        try
                                        {
                                            if (innerInfo.CanWrite)
                                            {
                                                Object value = GerneratePropertyValue(innerDataElement, innerInfo);
                                                if (!Validator.IsNullOrEmpty(value))
                                                    innerInfo.SetValue(elementTypeInstance, value, null);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Debug(e.ToString());
                                            //logHelper.LogToEvent("ReflectUtility002", AvePoint.Common.MetaData.LogLevel.Warn, e.ToString());
                                        }
                                    });
                                (closeGenericListInstance as IList).Add(elementTypeInstance);
                            }
                        });
                        result = closeGenericListType.GetMethod("ToArray").Invoke(closeGenericListInstance, null);
                    }
                    else
                    {
                        result = Activator.CreateInstance(info.PropertyType);
                        XmlElement innerDataElement = nodeList[0] as XmlElement;
                        if (innerDataElement != null)
                        {
                            Array.ForEach<PropertyInfo>(info.PropertyType.GetProperties(),
                                innerInfo =>
                                {
                                    try
                                    {
                                        if (innerInfo.CanWrite)
                                        {
                                            Object value = GerneratePropertyValue(innerDataElement, innerInfo);
                                            if (!Validator.IsNullOrEmpty(value))
                                                innerInfo.SetValue(result, value, null);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Debug(e.ToString());
                                        //logHelper.LogToEvent("ReflectUtility003", AvePoint.Common.MetaData.LogLevel.Warn, e.ToString());
                                    }
                                });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug(e.ToString());
                //logHelper.LogToEvent("ReflectUtility004", AvePoint.Common.MetaData.LogLevel.Warn, e.ToString());
            }
            return result;
        }

        /// <summary>
        /// To set the property value method and travel the complex type
        /// </summary>
        /// <param name="dataElement"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        static Object GerneratePropertyValue(XmlElement dataElement, PropertyInfo info)
        {
            Object value = default(Object);
            if (primaryTypeList.Contains(info.PropertyType))
            {
                if (info.PropertyType.IsEnum)
                    value = Enum.Parse(info.PropertyType, dataElement.GetAttribute(info.Name), true);
                else value = Convert.ChangeType(dataElement.GetAttribute(info.Name), info.PropertyType);
            }
            else value = GetComplexTypeValue(info, dataElement);
            return value;
        }

        /// <summary>
        /// This method used to generate a xml element object 
        /// </summary>
        /// <param name="data">T data</param>
        /// <param name="ownerDocment">xml element owner document</param>
        /// <param name="elementName">node name</param>
        /// <returns>xml element result</returns>
        public static XmlElement ConvertObjectToXmlELement<T>(T data, XmlDocument ownerDocment, String elementName)
            where T : class, new()
        {
            var result = default(XmlElement);
            if (ownerDocment == null)
                ownerDocment = new XmlDocument();
            if (String.IsNullOrEmpty(elementName))
                elementName = typeof(T).Name;

            result = ownerDocment.CreateElement(elementName);

            //This section must be changed in the future,
            //=================================================================================
            //=================================================================================
            //=================================================================================
            Array.ForEach<PropertyInfo>(
                typeof(T).GetProperties(),
                info =>
                {
                    String value = (info.GetValue(data, null) ?? String.Empty).ToString();
                    result.SetAttribute(info.Name, value);
                });
            //=================================================================================
            //=================================================================================
            //=================================================================================
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="ownerDocment"></param>
        /// <returns></returns>
        public static XmlElement ConvertObjectToXmlELement<T>(T data, XmlDocument ownerDocment)
            where T : class, new()
        {
            return ConvertObjectToXmlELement(data, ownerDocment, null);
        }

        /// <summary>
        /// an overload method for convert data to xml element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static XmlElement ConvertObjectToXmlELement<T>(T data)
            where T : class, new()
        {
            return ConvertObjectToXmlELement(data, null);
        }

        /// <summary>
        /// This method just convert to a generic list type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static List<T> ConvertToList<T>(IEnumerable<T> datas)
            where T : class
        {
            var result = new List<T>();
            result.AddRange(datas);
            return result;
        }

        /// <summary>
        /// This method just convert to a generic list type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static List<T> ConvertToList<T>(IEnumerable datas)
            where T : class
        {
            var result = new List<T>();
            foreach (Object o in datas)
            {
                result.Add(o as T);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static String GenerateSelectCommand(Type type, String tableName)
        {
            var result = new StringBuilder();
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static String GenerateInsertCommond(Type type, String tableName)
        {
            var result = new StringBuilder();
            if (type != null
                && !String.IsNullOrEmpty(tableName))
            {
                var propertyInfoList = ConvertToList<PropertyInfo>(type.GetProperties());
                result.AppendFormat("INSERT INTO [{0}] (", tableName);
                for (int i = 0; i < propertyInfoList.Count; i++)
                {
                    if (i < propertyInfoList.Count - 1)
                        result.AppendFormat("{0}, ", propertyInfoList[i].Name);
                    else result.AppendFormat("{0}) VALUES(", propertyInfoList[i].Name);
                }
                for (int i = 0; i < propertyInfoList.Count; i++)
                {
                    if (i < propertyInfoList.Count - 1)
                        result.AppendFormat("@{0}, ", propertyInfoList[i].Name);
                    else result.AppendFormat("@{0}) ", propertyInfoList[i].Name);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static String GenerateCreateTableCommond(Type type, String tableName)
        {
            var result = new StringBuilder();
            if (type != null
                && !String.IsNullOrEmpty(tableName))
            {
                var propertyInfoList = ConvertToList<PropertyInfo>(type.GetProperties());
                result.AppendFormat("CREATE TABLE IF NOT EXISTS [{0}] (", tableName);
                for (int i = 0; i < propertyInfoList.Count; i++)
                {
                    if (i < propertyInfoList.Count - 1)
                        result.AppendFormat("[{0}] TEXT, ", propertyInfoList[i].Name);
                    else result.AppendFormat("[{0}] TEXT)", propertyInfoList[i].Name);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Get the property value of a object instance
        /// </summary>
        /// <param name="entity">the instance</param>
        /// <param name="info">property information</param>
        /// <returns>the value in System.Objcet type</returns>
        public static Object GetPropertyValue(Object entity, PropertyInfo info)
        {
            var result = default(Object);
            if (primaryTypeList.Contains(info.PropertyType))
                result = info.GetValue(entity, null) ?? String.Empty;
            else result = SerializerHelper.SerializeToXmlString(entity);
            return result;
        }
    }
}
