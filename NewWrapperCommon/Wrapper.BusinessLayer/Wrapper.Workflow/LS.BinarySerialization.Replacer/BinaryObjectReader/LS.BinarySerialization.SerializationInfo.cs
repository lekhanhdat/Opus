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
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace LS.BinarySerialization
{
    public sealed class SerializationInfo
    {
        // Fields
        private const int defaultSize = 4;
        internal string m_assemName;
        internal IFormatterConverter m_converter;
        internal int m_currMember;
        internal object[] m_data;
        internal string m_fullTypeName;
        internal string[] m_members;
        internal Type[] m_types;

        private Type m_internalType;

        private Type parentType;
        private object parentObject;
        public Type ParentType
        {
            get
            {
                if (parentType == null)
                    parentType = Type.GetType("");
                return parentType;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public object Parent
        {
            get
            {
                if (parentObject == null)
                {
                    BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    ConstructorInfo[] cstrs = ParentType.GetConstructors(flags);

                    ConstructorInfo cstr = cstrs[0];
                    foreach (ConstructorInfo c in cstrs)
                    {
                        if (c.GetParameters().Length == 2)
                        {
                            cstr = c;
                            break;
                        }
                    }
                    parentObject = cstr.Invoke(new object[] {m_internalType,m_converter});
                }
                if (parentObject != null)
                { 
                    LSInvoker.SetRawProperty(parentObject,"m_assemName",m_assemName);
                    LSInvoker.SetRawProperty(parentObject,"m_converter",m_converter);
                    LSInvoker.SetRawProperty(parentObject,"m_currMember",m_currMember);
                    LSInvoker.SetRawProperty(parentObject,"m_data",m_data);
                    LSInvoker.SetRawProperty(parentObject,"m_fullTypeName",m_fullTypeName);
                    LSInvoker.SetRawProperty(parentObject,"m_members",m_members);
                    LSInvoker.SetRawProperty(parentObject, "m_types", m_types);
                }
                return parentObject;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this.m_assemName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_assemName = value;
            }
        }

        public string FullTypeName
        {
            get
            {
                return this.m_fullTypeName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_fullTypeName = value;
            }
        }

        public int MemberCount
        {
            get
            {
                return this.m_currMember;
            }
        }

        internal string[] MemberNames
        {
            get
            {
                return this.m_members;
            }
        }

        internal object[] MemberValues
        {
            get
            {
                return this.m_data;
            }
        }



        public SerializationInfo(Type type, IFormatterConverter converter)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            this.m_fullTypeName = type.FullName;
            this.m_assemName = type.Module.Assembly.FullName;
            this.m_members = new string[4];
            this.m_data = new object[4];
            this.m_types = new Type[4];
            this.m_converter = converter;
            this.m_currMember = 0;
            this.m_internalType = type;
        }

        public void AddValue(string name, bool value)
        {
            this.AddValue(name, value, typeof(bool));
        }

        public void AddValue(string name, byte value)
        {
            this.AddValue(name, value, typeof(byte));
        }

        public void AddValue(string name, char value)
        {
            this.AddValue(name, value, typeof(char));
        }

        public void AddValue(string name, DateTime value)
        {
            this.AddValue(name, value, typeof(DateTime));
        }

        public void AddValue(string name, decimal value)
        {
            this.AddValue(name, value, typeof(decimal));
        }

        public void AddValue(string name, double value)
        {
            this.AddValue(name, value, typeof(double));
        }

        public void AddValue(string name, short value)
        {
            this.AddValue(name, value, typeof(short));
        }

        public void AddValue(string name, int value)
        {
            this.AddValue(name, value, typeof(int));
        }

        public void AddValue(string name, long value)
        {
            this.AddValue(name, value, typeof(long));
        }

        public void AddValue(string name, object value)
        {
            if (value == null)
            {
                this.AddValue(name, value, typeof(object));
            }
            else
            {
                this.AddValue(name, value, value.GetType());
            }
        }

        public void AddValue(string name, sbyte value)
        {
            this.AddValue(name, value, typeof(sbyte));
        }

        public void AddValue(string name, float value)
        {
            this.AddValue(name, value, typeof(float));
        }

        public void AddValue(string name, ushort value)
        {
            this.AddValue(name, value, typeof(ushort));
        }

        public void AddValue(string name, uint value)
        {
            this.AddValue(name, value, typeof(uint));
        }

        public void AddValue(string name, ulong value)
        {
            this.AddValue(name, value, typeof(ulong));
        }

        public void AddValue(string name, object value, Type type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            for (int i = 0; i < this.m_currMember; i++)
            {
                if (this.m_members[i].Equals(name))
                {
                    throw new SerializationException(LSEnvironment.GetResourceString("Serialization_SameNameTwice"));
                }
            }
            this.AddValue(name, value, type, this.m_currMember);
        }

        internal void AddValue(string name, object value, Type type, int index)
        {
            if (index >= this.m_members.Length)
            {
                this.ExpandArrays();
            }
            this.m_members[index] = name;
            this.m_data[index] = value;
            this.m_types[index] = type;
            this.m_currMember++;
        }

        private void ExpandArrays()
        {
            int num = this.m_currMember * 2;
            if ((num < this.m_currMember) && (0x7fffffff > this.m_currMember))
            {
                num = 0x7fffffff;
            }
            string[] destinationArray = new string[num];
            object[] objArray = new object[num];
            Type[] typeArray = new Type[num];
            Array.Copy(this.m_members, destinationArray, this.m_currMember);
            Array.Copy(this.m_data, objArray, this.m_currMember);
            Array.Copy(this.m_types, typeArray, this.m_currMember);
            this.m_members = destinationArray;
            this.m_data = objArray;
            this.m_types = typeArray;
        }

        private int FindElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            for (int i = 0; i < this.m_currMember; i++)
            {
                if (this.m_members[i].Equals(name))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool GetBoolean(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(bool))
            {
                return (bool)element;
            }
            return this.m_converter.ToBoolean(element);
        }

        public byte GetByte(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(byte))
            {
                return (byte)element;
            }
            return this.m_converter.ToByte(element);
        }

        public char GetChar(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(char))
            {
                return (char)element;
            }
            return this.m_converter.ToChar(element);
        }

        public DateTime GetDateTime(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(DateTime))
            {
                return (DateTime)element;
            }
            return this.m_converter.ToDateTime(element);
        }

        public decimal GetDecimal(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(decimal))
            {
                return (decimal)element;
            }
            return this.m_converter.ToDecimal(element);
        }

        public double GetDouble(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(double))
            {
                return (double)element;
            }
            return this.m_converter.ToDouble(element);
        }

        private object GetElement(string name, out Type foundType)
        {
            int index = this.FindElement(name);
            if (index == -1)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_NotFound"), new object[] { name }));
            }
            foundType = this.m_types[index];
            return this.m_data[index];
        }

        private object GetElementNoThrow(string name, out Type foundType)
        {
            int index = this.FindElement(name);
            if (index == -1)
            {
                foundType = null;
                return null;
            }
            foundType = this.m_types[index];
            return this.m_data[index];
        }

        public SerializationInfoEnumerator GetEnumerator()
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfo[] cstrs = Type.GetType("System.Runtime.Serialization.SerializationInfoEnumerator").GetConstructors(flags);

            ConstructorInfo cstr = cstrs[0];
            foreach (ConstructorInfo c in cstrs)
            {
                if (c.GetParameters().Length == 4)
                {
                    cstr = c;
                    break;
                }
            }
            return (SerializationInfoEnumerator)cstr.Invoke(new object[] { this.m_members, this.m_data, this.m_types, this.m_currMember });
        }

        public short GetInt16(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(short))
            {
                return (short)element;
            }
            return this.m_converter.ToInt16(element);
        }

        public int GetInt32(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(int))
            {
                return (int)element;
            }
            return this.m_converter.ToInt32(element);
        }

        public long GetInt64(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(long))
            {
                return (long)element;
            }
            return this.m_converter.ToInt64(element);
        }

        public sbyte GetSByte(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(sbyte))
            {
                return (sbyte)element;
            }
            return this.m_converter.ToSByte(element);
        }

        public float GetSingle(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(float))
            {
                return (float)element;
            }
            return this.m_converter.ToSingle(element);
        }

        public string GetString(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if ((type != typeof(string)) && (element != null))
            {
                return this.m_converter.ToString(element);
            }
            return (string)element;
        }

        public ushort GetUInt16(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(ushort))
            {
                return (ushort)element;
            }
            return this.m_converter.ToUInt16(element);
        }

        public uint GetUInt32(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(uint))
            {
                return (uint)element;
            }
            return this.m_converter.ToUInt32(element);
        }

        public ulong GetUInt64(string name)
        {
            Type type;
            object element = this.GetElement(name, out type);
            if (type == typeof(ulong))
            {
                return (ulong)element;
            }
            return this.m_converter.ToUInt64(element);
        }

        public object GetValue(string name, Type type)
        {
            Type type2;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            object element = this.GetElement(name, out type2);
            if (RemotingServices.IsTransparentProxy(element))
            {
                //Modified
                //if (RemotingServices.ProxyCheckCast(RemotingServices.GetRealProxy(element), type))
                if ((bool)LSInvoker.CallStaticMethod(typeof(RemotingServices), "ProxyCheckCast", new Type[] { typeof(RealProxy), typeof(Type) }, new object[] { RemotingServices.GetRealProxy(element), type }))
                {
                    return element;
                }
            }
            else if (((type2 == type) || type.IsAssignableFrom(type2)) || (element == null))
            {
                return element;
            }
            return this.m_converter.Convert(element, type);
        }

        internal object GetValueNoThrow(string name, Type type)
        {
            Type type2;
            object elementNoThrow = this.GetElementNoThrow(name, out type2);
            if (elementNoThrow == null)
            {
                return null;
            }
            if (RemotingServices.IsTransparentProxy(elementNoThrow))
            {
                //Modified
                //if (RemotingServices.ProxyCheckCast(RemotingServices.GetRealProxy(elementNoThrow), type))
                if ((bool)LSInvoker.CallStaticMethod(typeof(RemotingServices), "ProxyCheckCast", new Type[] { typeof(RealProxy), typeof(Type) }, new object[] { RemotingServices.GetRealProxy(elementNoThrow), type }))
                {
                    return elementNoThrow;
                }
            }
            else if (((type2 == type) || type.IsAssignableFrom(type2)) || (elementNoThrow == null))
            {
                return elementNoThrow;
            }
            return this.m_converter.Convert(elementNoThrow, type);
        }

        public void SetType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.m_fullTypeName = type.FullName;
            this.m_assemName = type.Module.Assembly.FullName;
        }

        internal void UpdateValue(string name, object value, Type type)
        {
            int index = this.FindElement(name);
            if (index < 0)
            {
                this.AddValue(name, value, type, this.m_currMember);
            }
            else
            {
                this.m_members[index] = name;
                this.m_data[index] = value;
                this.m_types[index] = type;
            }
        }


    }
}
