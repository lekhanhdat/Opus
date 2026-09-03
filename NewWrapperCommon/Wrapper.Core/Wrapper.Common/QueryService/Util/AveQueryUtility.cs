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
using System.Reflection;
using System.Xml;
using System.Data.SqlClient;
using System.Reflection.Emit;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Linq;

namespace AvePoint.Wrapper.Common
{
    internal class AveQueryUtility
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> mFieldMaps = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        internal static Dictionary<string, FieldInfo> GetFieldMap(Type type, string prefix)
        {
#if DEBUG
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.SiteUsers.GetObjectData.GetFieldMap"))
            {
#endif

                if (!mFieldMaps.ContainsKey(type))
                {
                    Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();
                    foreach (FieldInfo fieldInfo in type.GetFields())
                    {
                        if (string.IsNullOrEmpty(prefix))
                        {
                            fieldMap[fieldInfo.Name] = fieldInfo;
                        }
                        else
                        {
                            fieldMap[prefix + fieldInfo.Name] = fieldInfo;
                        }
                    }
                    lock (mFieldMaps)
                    {
                        if (!mFieldMaps.ContainsKey(type))
                        {
                            mFieldMaps[type] = fieldMap;
                        }
                    }
                }
                return mFieldMaps[type];
#if DEBUG
            }
#endif
        }

        internal static void GetDBRow(IDictionary<string, object> data, AveQueryWorker queryWorker, string cmdText)
        {
            GetDBRow(data, queryWorker, cmdText, 0);
        }

        internal static void GetDBRow(IDictionary<string, object> data, AveQueryWorker queryWorker, string cmdText, int startIndex)
        {
            using (SqlDataReader dr = queryWorker.ExecuteReader(cmdText))
            {
                if (!dr.Read())
                {
                    throw new AveException("Cannot find data.");
                }
                GetDBRow(data, dr, startIndex);
            }
        }

        internal static bool TryGetDBRow(IDictionary<string, object> data, AveQueryWorker queryWorker, string cmdText)
        {
            return TryGetDBRow(data, queryWorker, cmdText, 0);
        }

        internal static bool TryGetDBRow(IDictionary<string, object> data, AveQueryWorker queryWorker, string cmdText, int startIndex)
        {
            try
            {
                using (SqlDataReader dr = queryWorker.ExecuteReader(cmdText))
                {
                    if (!dr.Read())
                    {
                        return false;
                    }
                    GetDBRow(data, dr, startIndex);
                    return true;
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        internal static void GetDBRow(IDictionary<string, object> data, SqlDataReader sqlReader)
        {
            GetDBRow(data, sqlReader, 0);
        }

        internal static void GetDBRow(IDictionary<string, object> data, SqlDataReader sqlReader, int startIndex)
        {
            int fieldCount = sqlReader.FieldCount;
            for (int i = startIndex; i < fieldCount; i++)
            {
                if (sqlReader.IsDBNull(i))
                {
                    continue;
                }

                string name = sqlReader.GetName(i);
                object value = sqlReader.GetValue(i);
                data[name] = value;
                //if (name.Equals("tp_ColumnSet", StringComparison.OrdinalIgnoreCase))
                //{
                //    AddColumnSetToDictionary(value, data, string.Empty);
                //}
                //else if (name.Equals("UD#tp_ColumnSet", StringComparison.OrdinalIgnoreCase))
                //{
                //    AddColumnSetToDictionary(value, data, "UD#");
                //}
                //else
                //{
                //    data[name] = value;
                //}

            }
        }

        internal static void AddColumnSetToDictionary(object value, IDictionary<string, object> data, string prefix)
        {
            if (string.IsNullOrEmpty(value as string))
                return;

            System.Globalization.CultureInfo temp = null;
            try
            {
                //change current culture to en-us
                if (System.Threading.Thread.CurrentThread.CurrentCulture.LCID != 1033)
                {
                    temp = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1033);
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<ColumnSet>" + value.ToString() + "</ColumnSet>");
                XmlNodeList columnSet = xmlDoc.FirstChild.ChildNodes;
                foreach (XmlNode column in columnSet)
                {
                    string columnName = column.Name;
                    object columnValue = null;

                    if (columnName.StartsWith("bit", StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = Convert.ToBoolean(Int32.Parse(column.InnerText));
                    }
                    else if (columnName.StartsWith("datetime", StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = Convert.ToDateTime(column.InnerText);
                    }
                    else if (columnName.StartsWith("float", StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = Double.Parse(column.InnerText, CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    }

                    else if (columnName.StartsWith("int", StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = Convert.ToInt32(column.InnerText);
                    }
                    //else if (columnName.StartsWith("ntext", StringComparison.OrdinalIgnoreCase))
                    //{

                    //}
                    //else if (columnName.StartsWith("nvarchar", StringComparison.OrdinalIgnoreCase))
                    //{

                    //}
                    //else if (columnName.StartsWith("sql_variant", StringComparison.OrdinalIgnoreCase))
                    //{

                    //}
                    else if (columnName.StartsWith("uniqueidentifier", StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = new Guid(column.InnerText);
                    }
                    //else if (columnName.StartsWith("geography", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    //圆球形坐标系中的数据
                    //    columnValue = Encoding.UTF8.GetBytes(column.InnerText);
                    //}
                    else
                    {
                        columnValue = column.InnerText;
                    }

                    data[prefix + columnName] = columnValue;

                }
            }
            finally
            {
                if (temp != null)
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(temp.LCID);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "avd is AllDocVersions table. ")]
        public static string GetAllDocVersionsForSpecialLibrary_Select_AllDocVersions(List<Guid> docIds)
        {
            //当Document Library的Version数量被限制时，SharePint有Bug：在界面中看不到的小Version(已经不存在于AllDocs表和AllDocVersions表)也会出现在AUD表中，造成查出多余数据。 所以这种情况需要连AllDocVersions表。
            const string condition = "WHERE avd.SiteId = @SiteId And avd.Id in({0})";
            var idCollectionString = GetCondByCommaSeparatedList(docIds);
            return AveDiscoverQueryString.AllDocVersionsForSpecialLibrary
                .Replace("@WHERE", string.Format(condition, idCollectionString));
        }

        public static string GetAllDocVersionsUserData_Select_AllUserData_AllDocs_AllDocVersions(List<Guid> docIds, AveDiscoverReader discoverReader)
        {
            var baseCommand = discoverReader.GetAllVersionsQueryString();
            var condition = discoverReader.GetVersionConditionWithDocIds();
            var idCollectionString = GetCondByCommaSeparatedList(docIds);
            return baseCommand.Replace("@WHERE", string.Format(condition, idCollectionString));
        }

        public static string GetCondByCommaSeparatedList<T>(IEnumerable<T> collection)
        {
            if (collection == null || collection.Count() == 0)
            { return String.Empty; }
            StringBuilder text = new StringBuilder();
            foreach (var item in collection)
            {
                text.Append(string.Format("'{0}',", item));
            }
            text.Length -= 1;
            return text.ToString();
        }

        internal static List<T> GetDBRows<T>(AveQueryWorker queryWorker, string cmdText)
        {
            return GetDBRows<T>(queryWorker, cmdText, null);
        }

        internal static List<T> GetDBRows<T>(AveQueryWorker queryWorker, string cmdText, string prefix)
        {
            List<T> values = null;
            GetDBRows<T>(ref values, queryWorker, cmdText, prefix);
            return values;
        }

        internal static void GetDBRows<T>(ref List<T> values, AveQueryWorker queryWorker, string cmdText)
        {
            GetDBRows<T>(ref values, queryWorker, cmdText, null);
        }

        internal static void GetDBRows<T>(ref List<T> values, AveQueryWorker queryWorker, string cmdText, string prefix)
        {
            Type type = typeof(T);
            Dictionary<string, FieldInfo> fieldMap = GetFieldMap(type, prefix);
            try
            {
                using (SqlDataReader dr = queryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        T value = AveTypeUtility.CreateNewInstance<T>();//(T)AveAssemblyUtility.CreateInstanceByType(type);
                        GetDBRow(value, dr, fieldMap, 0);
                        if (values == null)
                        {
                            values = new List<T>();
                        }
                        values.Add(value);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        internal static List<Dictionary<string, object>> GetDBRows(AveQueryWorker queryWorker, string cmdText)
        {
            List<Dictionary<string, object>> rows = null;
            try
            {
                using (SqlDataReader dr = queryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        GetDBRow(dic, dr);
                        if (rows == null)
                        {
                            rows = new List<Dictionary<string, object>>();
                        }
                        rows.Add(dic);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return rows;
        }

        internal static void GetDBRow(object data, AveQueryWorker queryWorker, string cmdText)
        {
            GetDBRow(data, queryWorker, cmdText, null, 0);
        }

        internal static void GetDBRow(object data, AveQueryWorker queryWorker, string cmdText, string prefix)
        {
            GetDBRow(data, queryWorker, cmdText, prefix, 0);
        }

        internal static void GetDBRow(object data, AveQueryWorker queryWorker, string cmdText, string prefix, int startIndex)
        {
            Dictionary<string, FieldInfo> fieldMap = GetFieldMap(data.GetType(), prefix);
            try
            {
                using (SqlDataReader dr = queryWorker.ExecuteReader(cmdText))
                {
                    if (!dr.Read())
                    {
                        throw new AveException("Cannot find data.");
                    }
                    GetDBRow(data, dr, fieldMap, startIndex);
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception e)
            {
                var aaa = e.ToString();
            }
        }

        internal static void GetDBRow(object data, SqlDataReader sqlReader, Dictionary<string, FieldInfo> fieldMap, int startIndex)
        {
            if (data == null)
            {
                return;
            }
            int fieldCount = sqlReader.FieldCount;

            for (int i = startIndex; i < fieldCount; i++)
            {
                if (sqlReader.IsDBNull(i))
                {
                    continue;
                }
                string name = sqlReader.GetName(i);
                object value = sqlReader.GetValue(i);
                if (fieldMap.ContainsKey(name))
                {
                    Type fieldType = fieldMap[name].FieldType;
                    if (sqlReader.GetFieldType(i).IsAssignableFrom(fieldType))
                    {
                        fieldMap[name].SetValue(data, value);
                    }
                    else
                    {
                        fieldMap[name].SetValue(data, AveConvert.ChangeType(value, fieldType));
                    }
                }
            }
        }
    }

    public static class AveTypeUtility
    {
        public delegate object CreateInstance();
        //public delegate void SetFieldValue(object obj, object value);
        //public delegate object GetFieldValue(object obj);

        /// <summary>
        /// Save 下面已经添加lock
        /// </summary>
        private static Dictionary<Type, CreateInstance> collections = new Dictionary<Type, CreateInstance>();

        public static CreateInstance GetConstructorMethod(Type type)
        {
            CreateInstance instance = null;

            lock (collections)
            {
                if (collections.ContainsKey(type))
                {
                    instance = collections[type];
                }
            }

            if (instance == null)
            {
                instance = MakeConstructorMethod(type);
                lock (collections)
                {
                    if (!collections.ContainsKey(type))
                    {
                        collections[type] = instance;
                    }
                }
            }

            return instance;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ctor is the part of method name. ")]
        public static CreateInstance MakeConstructorMethod(Type type)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(new Type[0]);
            if (constructorInfo == null)
            {
                throw new Exception(string.Format("Cannot find default constructor for type:{0}", type.FullName));
            }
            DynamicMethod dynamicMethod = new DynamicMethod(type.FullName + "Ctor", type, new Type[0], type.Module);
            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);

            return (CreateInstance)dynamicMethod.CreateDelegate(typeof(CreateInstance));
        }

        public static T CreateNewInstance<T>()
        {
#if DEBUG
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.SiteUsers.GetObjectData.CreateNewInstance"))
            {
#endif
                CreateInstance instance = GetConstructorMethod(typeof(T));
                return (T)instance();
#if DEBUG
            }
#endif
        }

        //public static SetFieldValue CreateSetterImpl(FieldInfo fieldInfo)
        //{
        //    Type objceType = fieldInfo.ReflectedType;

        //    DynamicMethod dynamicMethod = new DynamicMethod(string.Format("{0}_{1}_Setter", objceType.Name, fieldInfo.Name),
        //        typeof(void), new Type[] { typeof(object), typeof(object) }, objceType);
        //    ILGenerator generator = dynamicMethod.GetILGenerator();
        //    generator.Emit(OpCodes.Ldarg_0);
        //    generator.Emit(OpCodes.Ldarg_1);
        //    generator.Emit(OpCodes.Stfld, fieldInfo);
        //    generator.Emit(OpCodes.Ret);

        //    return (SetFieldValue)dynamicMethod.CreateDelegate(typeof(SetFieldValue));
        //}

        //public static GetFieldValue CreateGetterImpl(FieldInfo fieldInfo)
        //{
        //    Type objceType = fieldInfo.ReflectedType;

        //    DynamicMethod dynamicMethod = new DynamicMethod(string.Format("{0}_{1}_Getter", objceType.Name, fieldInfo.Name),
        //        typeof(object), new Type[] { typeof(object) }, objceType);
        //    ILGenerator generator = dynamicMethod.GetILGenerator();
        //    generator.Emit(OpCodes.Ldarg_0);
        //    generator.Emit(OpCodes.Ldfld, fieldInfo);
        //    generator.Emit(OpCodes.Ret);

        //    return (GetFieldValue)dynamicMethod.CreateDelegate(typeof(GetFieldValue));
        //}
    }

    //public class FieldInfoWrapper
    //{
    //    public FieldInfo Field;
    //    private AveTypeUtility.SetFieldValue Setter;
    //    private AveTypeUtility.GetFieldValue Getter;

    //    public FieldInfoWrapper(FieldInfo field)
    //    {
    //        this.Field = field;
    //        this.Setter = AveTypeUtility.CreateSetterImpl(field);
    //        this.Getter = AveTypeUtility.CreateGetterImpl(field);
    //    }

    //    public void Set(object obj, object value)
    //    {
    //        if (Setter != null)
    //        {
    //            Setter(obj, value);
    //        }
    //    }

    //    public object Get(object obj)
    //    {
    //        if (Getter != null)
    //        {
    //            return Getter(obj);
    //        }
    //        return null;
    //    }
    //}
}
