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
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    public class AveJsonSerializer
    {
        // Fields
        private AveJsonSerializer _baseSerializer;
        private IDictionary<Type, AveJsonEncoder> _encoders;
        private DepthScopeMonitor _depthScopeMonitor;
        private Func<DictionaryEntry, bool> _skipDelegate;
        private Func<object, bool> _skipObject;
        private static AveJsonSerializer _shared;
        private static AveLogger Log = new AveLogger(typeof(AveJsonSerializer));

        // Methods
        public AveJsonSerializer()
        {
            this._depthScopeMonitor = new DepthScopeMonitor();
            this._encoders = new Dictionary<Type, AveJsonEncoder>(0);
        }

        public AveJsonSerializer(Func<DictionaryEntry, bool> skipDelegate, Func<object, bool> skipObject)
        {
            this._depthScopeMonitor = new DepthScopeMonitor();
            this._encoders = new Dictionary<Type, AveJsonEncoder>(0);
            this._skipDelegate = skipDelegate;
            this._skipObject = skipObject;
        }

        public AveJsonSerializer(IDictionary<Type, AveJsonEncoder> encoders)
        {
            this._depthScopeMonitor = new DepthScopeMonitor();
            this._encoders = encoders;
        }

        public AveJsonSerializer(IDictionary<Type, AveJsonEncoder> encoders, AveJsonSerializer baseSerializer)
            : this(encoders)
        {
            this._baseSerializer = baseSerializer;
        }

        public AveJsonEncoder GetEncoderForType(Type t)
        {
            AveJsonEncoder encoder = null;
            if (this._encoders.ContainsKey(t))
            {
                encoder = this._encoders[t];
            }
            if (encoder != null)
            {
                return encoder;
            }
            if (this.BaseSerializer == null)
            {
                return null;
            }
            return this.BaseSerializer.GetEncoderForType(t);
        }

        private string SerializeToJson(IDictionary dict)
        {
            if (this._depthScopeMonitor.ExceedMaxDepth())
            {
                return string.Empty;
            }
            using (this._depthScopeMonitor.BeginScope())
            {
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                if (dict != null)
                {
                    writer.Write("{");
                    bool flag = false;
                    foreach (DictionaryEntry entry in dict)
                    {
                        if ((entry.Key != null) && (entry.Value != null) && (_skipDelegate == null || !_skipDelegate(entry)))
                        {
                            string str = this.WriteKeyValuePair<object, object>(entry.Key, entry.Value);
                            if (!string.IsNullOrEmpty(str))
                            {
                                if (flag)
                                {
                                    writer.Write(",");
                                }
                                writer.Write(str);
                                flag = true;
                            }
                        }
                    }
                    writer.Write("}");
                }
                string str2 = writer.ToString();
                writer.Dispose();
                return str2;
            }
        }

        private string SerializeToJson(IEnumerable enmbl)
        {
            if (this._depthScopeMonitor.ExceedMaxDepth())
            {
                return string.Empty;
            }
            using (this._depthScopeMonitor.BeginScope())
            {
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                if (enmbl != null && enmbl.GetType() != typeof(byte[]))
                {
                    writer.Write("[");
                    bool flag = false;
                    foreach (object obj2 in enmbl)
                    {
                        if (flag)
                        {
                            writer.Write(",");
                        }
                        string str = this.SerializeToJson(obj2);
                        if (!string.IsNullOrEmpty(str))
                        {
                            writer.Write(str);
                        }
                        flag = true;
                    }
                    writer.Write("]");
                }
                string str2 = writer.ToString();
                writer.Dispose();
                return str2;
            }
        }

        private string SerializeToJson(DataRow dr)
        {
            if (this._depthScopeMonitor.ExceedMaxDepth())
            {
                return null;
            }
            using (this._depthScopeMonitor.BeginScope())
            {
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                if (((dr != null) && (dr.Table != null)) && (dr.Table.Columns != null))
                {
                    writer.Write("{");
                    bool flag = false;
                    foreach (DataColumn column in dr.Table.Columns)
                    {
                        string str = this.WriteKeyValuePair<string, object>(column.ColumnName, dr[column]);
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (flag)
                            {
                                writer.Write(",");
                            }
                            writer.Write(str);
                            flag = true;
                        }
                    }
                    writer.Write("}");
                }
                string str2 = writer.ToString();
                writer.Dispose();
                return str2;
            }
        }

        private string SerializeToJson(DataSet ds)
        {
            if ((ds == null) || (ds.Tables == null) || this._depthScopeMonitor.ExceedMaxDepth())
            {
                return null;
            }
            Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
            foreach (DataTable table in ds.Tables)
            {
                dictionary[table.TableName] = table;
            }
            return this.SerializeToJson((IDictionary)dictionary);
        }

        private string SerializeToJson(DataTable dt)
        {
            return this.SerializeToJson((IEnumerable)dt.Rows);
        }

        public string SerializeToJson(object o)
        {
            if ((o == null) || (o == DBNull.Value) || this._depthScopeMonitor.ExceedMaxDepth() || (_skipObject != null && _skipObject(o)))
            {
                return null;
            }
            try
            {
                AveJsonEncoder encoderForType = this.GetEncoderForType(o.GetType());
                if (encoderForType != null)
                {
                    return encoderForType(this, o);
                }
                AveJsonGenerator generator = o as AveJsonGenerator;
                if (generator != null)
                {
                    return generator(this);
                }
                IAveJsonSerializable serializable = o as IAveJsonSerializable;
                if (serializable != null)
                {
                    return serializable.ToJson(this);
                }
                bool? nullable = o as bool?;
                if (nullable.HasValue)
                {
                    return nullable.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
                }
                decimal? nullable2 = o as decimal?;
                if (nullable2.HasValue)
                {
                    return nullable2.Value.ToString(CultureInfo.InvariantCulture);
                }
                DateTime? nullable3 = o as DateTime?;
                if (nullable3.HasValue)
                {
                    return AveJsonUtility.SerializeDateTime(nullable3.Value);
                }
                if ((o is float) || (o is double))
                {
                    double num2 = (double)o;
                    return num2.ToString(CultureInfo.InvariantCulture);
                }
                if (((o is char) || (o is string)) || (o is Guid))
                {
                    return AveJsonUtility.QuoteString(AveJsonUtility.JsonEncode(o.ToString()));
                }
                if (((o is Enum) || (o is sbyte)) || (((o is short) || (o is int)) || (o is long)))
                {
                    long num3 = (long)Convert.ChangeType(o, typeof(long), CultureInfo.InvariantCulture);
                    return num3.ToString(CultureInfo.InvariantCulture);
                }
                if (((o is byte) || (o is ushort)) || ((o is uint) || (o is ulong)))
                {
                    ulong num4 = (ulong)Convert.ChangeType(o, typeof(ulong), CultureInfo.InvariantCulture);
                    return num4.ToString(CultureInfo.InvariantCulture);
                }
                DataSet ds = o as DataSet;
                if (ds != null)
                {
                    return this.SerializeToJson(ds);
                }
                DataTable dt = o as DataTable;
                if (dt != null)
                {
                    return this.SerializeToJson(dt);
                }
                DataRow dr = o as DataRow;
                if (dr != null)
                {
                    return this.SerializeToJson(dr);
                }
                IDictionary dict = o as IDictionary;
                if (dict != null)
                {
                    return this.SerializeToJson(dict);
                }
                IEnumerable enmbl = o as IEnumerable;
                if (enmbl != null)
                {
                    return this.SerializeToJson(enmbl);
                }
                return AveJsonUtility.SerializeToJsonFromProperties(this, o);
            }
            catch (Exception ex)
            {
                Log.Debug("Error occurred while serializer object.Message:{0}.", ex.ToString());
                return null;
            }
        }

        private string WriteKeyValuePair<T, U>(T key, U value)
        {
            string str = this.SerializeToJson(value);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(this.SerializeToJson(key));
            builder.Append(':');
            builder.Append(str);
            return builder.ToString();
        }

        // Properties
        public AveJsonSerializer BaseSerializer
        {
            get
            {
                return this._baseSerializer;
            }
            set
            {
                this._baseSerializer = value;
            }
        }

        public static AveJsonSerializer Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new AveJsonSerializer();
                }
                return _shared;
            }
        }
    }

    internal class DepthScopeMonitor
    {
        private const int MaxDepth = 10;
        private int depth = 0;

        public DepthScopeMonitor()
        {
        }

        public IDisposable BeginScope()
        {
            return new DepthScope(this);
        }

        internal void Increment()
        {
            this.depth++;
        }

        internal void Decrement()
        {
            this.depth--;
        }

        public bool ExceedMaxDepth()
        {
            return this.depth > MaxDepth;
        }
    }

    internal class DepthScope : IDisposable
    {
        private DepthScopeMonitor scopeMonitor;

        public DepthScope(DepthScopeMonitor scopeMonitor)
        {
            this.scopeMonitor = scopeMonitor;
            this.scopeMonitor.Increment();
        }

        public void Dispose()
        {
            this.scopeMonitor.Decrement();
        }
    }
}
