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
using System.Runtime.Serialization;

namespace LS.BinarySerialization
{
    internal sealed class Converter
    {
        // Fields
        private static Type[] arrayTypeA;
        private static InternalPrimitiveTypeE[] codeA;
        private static int primitiveTypeEnumLength;
        private static Type[] typeA;
        private static TypeCode[] typeCodeA;
        internal static Type typeofBoolean;
        internal static Type typeofBooleanArray;
        internal static Type typeofByte;
        internal static Type typeofByteArray;
        internal static Type typeofChar;
        internal static Type typeofCharArray;
        internal static Type typeofConverter;
        internal static Type typeofDateTime;
        internal static Type typeofDateTimeArray;
        internal static Type typeofDecimal;
        internal static Type typeofDecimalArray;
        internal static Type typeofDouble;
        internal static Type typeofDoubleArray;
        internal static Type typeofInt16;
        internal static Type typeofInt16Array;
        internal static Type typeofInt32;
        internal static Type typeofInt32Array;
        internal static Type typeofInt64;
        internal static Type typeofInt64Array;
        internal static Type typeofISerializable;
        internal static Type typeofMarshalByRefObject;
        internal static Type typeofObject;
        internal static Type typeofObjectArray;
        internal static Type typeofSByte;
        internal static Type typeofSByteArray;
        internal static Type typeofSingle;
        internal static Type typeofSingleArray;
        internal static Type typeofString;
        internal static Type typeofStringArray;
        internal static Type typeofSystemVoid;
        internal static Type typeofTimeSpan;
        internal static Type typeofTimeSpanArray;
        internal static Type typeofTypeArray;
        internal static Type typeofUInt16;
        internal static Type typeofUInt16Array;
        internal static Type typeofUInt32;
        internal static Type typeofUInt32Array;
        internal static Type typeofUInt64;
        internal static Type typeofUInt64Array;
        internal static Assembly urtAssembly;
        internal static string urtAssemblyString;
        private static string[] valueA;

        static Converter()
        {
            primitiveTypeEnumLength = 0x11;
            typeofISerializable = typeof(ISerializable);
            typeofString = typeof(string);
            typeofConverter = typeof(Converter);
            typeofBoolean = typeof(bool);
            typeofByte = typeof(byte);
            typeofChar = typeof(char);
            typeofDecimal = typeof(decimal);
            typeofDouble = typeof(double);
            typeofInt16 = typeof(short);
            typeofInt32 = typeof(int);
            typeofInt64 = typeof(long);
            typeofSByte = typeof(sbyte);
            typeofSingle = typeof(float);
            typeofTimeSpan = typeof(TimeSpan);
            typeofDateTime = typeof(DateTime);
            typeofUInt16 = typeof(ushort);
            typeofUInt32 = typeof(uint);
            typeofUInt64 = typeof(ulong);
            typeofObject = typeof(object);
            typeofSystemVoid = typeof(void);
            urtAssembly = Assembly.GetAssembly(typeofString);
            urtAssemblyString = urtAssembly.FullName;
            typeofTypeArray = typeof(Type[]);
            typeofObjectArray = typeof(object[]);
            typeofStringArray = typeof(string[]);
            typeofBooleanArray = typeof(bool[]);
            typeofByteArray = typeof(byte[]);
            typeofCharArray = typeof(char[]);
            typeofDecimalArray = typeof(decimal[]);
            typeofDoubleArray = typeof(double[]);
            typeofInt16Array = typeof(short[]);
            typeofInt32Array = typeof(int[]);
            typeofInt64Array = typeof(long[]);
            typeofSByteArray = typeof(sbyte[]);
            typeofSingleArray = typeof(float[]);
            typeofTimeSpanArray = typeof(TimeSpan[]);
            typeofDateTimeArray = typeof(DateTime[]);
            typeofUInt16Array = typeof(ushort[]);
            typeofUInt32Array = typeof(uint[]);
            typeofUInt64Array = typeof(ulong[]);
            typeofMarshalByRefObject = typeof(MarshalByRefObject);
        }

        internal static string ToComType(InternalPrimitiveTypeE code)
        {
            if (valueA == null)
            {
                InitValueA();
            }
            return valueA[(int)code];
        }

        internal static Type ToType(InternalPrimitiveTypeE code)
        {
            if (typeA == null)
            {
                InitTypeA();
            }
            return typeA[(int)code];
        }

        internal static Type ToArrayType(InternalPrimitiveTypeE code)
        {
            if (arrayTypeA == null)
            {
                InitArrayTypeA();
            }
            return arrayTypeA[(int)code];
        }

        internal static bool IsWriteAsByteArray(InternalPrimitiveTypeE code)
        {
            bool flag = false;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Char:
                case InternalPrimitiveTypeE.Double:
                case InternalPrimitiveTypeE.Int16:
                case InternalPrimitiveTypeE.Int32:
                case InternalPrimitiveTypeE.Int64:
                case InternalPrimitiveTypeE.SByte:
                case InternalPrimitiveTypeE.Single:
                case InternalPrimitiveTypeE.UInt16:
                case InternalPrimitiveTypeE.UInt32:
                case InternalPrimitiveTypeE.UInt64:
                    return true;

                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return flag;
            }
            return flag;
        }

        internal static int TypeLength(InternalPrimitiveTypeE code)
        {
            int num = 0;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    return 1;

                case InternalPrimitiveTypeE.Byte:
                    return 1;

                case InternalPrimitiveTypeE.Char:
                    return 2;

                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return num;

                case InternalPrimitiveTypeE.Double:
                    return 8;

                case InternalPrimitiveTypeE.Int16:
                    return 2;

                case InternalPrimitiveTypeE.Int32:
                    return 4;

                case InternalPrimitiveTypeE.Int64:
                    return 8;

                case InternalPrimitiveTypeE.SByte:
                    return 1;

                case InternalPrimitiveTypeE.Single:
                    return 4;

                case InternalPrimitiveTypeE.UInt16:
                    return 2;

                case InternalPrimitiveTypeE.UInt32:
                    return 4;

                case InternalPrimitiveTypeE.UInt64:
                    return 8;
            }
            return num;
        }

        internal static Array CreatePrimitiveArray(InternalPrimitiveTypeE code, int length)
        {
            Array array = null;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    return new bool[length];

                case InternalPrimitiveTypeE.Byte:
                    return new byte[length];

                case InternalPrimitiveTypeE.Char:
                    return new char[length];

                case InternalPrimitiveTypeE.Currency:
                    return array;

                case InternalPrimitiveTypeE.Decimal:
                    return new decimal[length];

                case InternalPrimitiveTypeE.Double:
                    return new double[length];

                case InternalPrimitiveTypeE.Int16:
                    return new short[length];

                case InternalPrimitiveTypeE.Int32:
                    return new int[length];

                case InternalPrimitiveTypeE.Int64:
                    return new long[length];

                case InternalPrimitiveTypeE.SByte:
                    return new sbyte[length];

                case InternalPrimitiveTypeE.Single:
                    return new float[length];

                case InternalPrimitiveTypeE.TimeSpan:
                    return new TimeSpan[length];

                case InternalPrimitiveTypeE.DateTime:
                    return new DateTime[length];

                case InternalPrimitiveTypeE.UInt16:
                    return new ushort[length];

                case InternalPrimitiveTypeE.UInt32:
                    return new uint[length];

                case InternalPrimitiveTypeE.UInt64:
                    return new ulong[length];
            }
            return array;
        }

        private static void InitValueA()
        {
            string[] strArray = new string[primitiveTypeEnumLength];
            strArray[0] = null;
            strArray[1] = "Boolean";
            strArray[2] = "Byte";
            strArray[3] = "Char";
            strArray[5] = "Decimal";
            strArray[6] = "Double";
            strArray[7] = "Int16";
            strArray[8] = "Int32";
            strArray[9] = "Int64";
            strArray[10] = "SByte";
            strArray[11] = "Single";
            strArray[12] = "TimeSpan";
            strArray[13] = "DateTime";
            strArray[14] = "UInt16";
            strArray[15] = "UInt32";
            strArray[0x10] = "UInt64";
            valueA = strArray;
        }

        private static void InitTypeA()
        {
            Type[] typeArray = new Type[primitiveTypeEnumLength];
            typeArray[0] = null;
            typeArray[1] = typeofBoolean;
            typeArray[2] = typeofByte;
            typeArray[3] = typeofChar;
            typeArray[5] = typeofDecimal;
            typeArray[6] = typeofDouble;
            typeArray[7] = typeofInt16;
            typeArray[8] = typeofInt32;
            typeArray[9] = typeofInt64;
            typeArray[10] = typeofSByte;
            typeArray[11] = typeofSingle;
            typeArray[12] = typeofTimeSpan;
            typeArray[13] = typeofDateTime;
            typeArray[14] = typeofUInt16;
            typeArray[15] = typeofUInt32;
            typeArray[0x10] = typeofUInt64;
            typeA = typeArray;
        }

        private static void InitArrayTypeA()
        {
            Type[] typeArray = new Type[primitiveTypeEnumLength];
            typeArray[0] = null;
            typeArray[1] = typeofBooleanArray;
            typeArray[2] = typeofByteArray;
            typeArray[3] = typeofCharArray;
            typeArray[5] = typeofDecimalArray;
            typeArray[6] = typeofDoubleArray;
            typeArray[7] = typeofInt16Array;
            typeArray[8] = typeofInt32Array;
            typeArray[9] = typeofInt64Array;
            typeArray[10] = typeofSByteArray;
            typeArray[11] = typeofSingleArray;
            typeArray[12] = typeofTimeSpanArray;
            typeArray[13] = typeofDateTimeArray;
            typeArray[14] = typeofUInt16Array;
            typeArray[15] = typeofUInt32Array;
            typeArray[0x10] = typeofUInt64Array;
            arrayTypeA = typeArray;
        }

        internal static InternalPrimitiveTypeE ToCode(Type type)
        {
            if ((type != null) && !type.IsPrimitive)
            {
                if (type == typeofDateTime)
                {
                    return InternalPrimitiveTypeE.DateTime;
                }
                if (type == typeofTimeSpan)
                {
                    return InternalPrimitiveTypeE.TimeSpan;
                }
                if (type == typeofDecimal)
                {
                    return InternalPrimitiveTypeE.Decimal;
                }
                return InternalPrimitiveTypeE.Invalid;
            }
            return ToPrimitiveTypeEnum(Type.GetTypeCode(type));
        }

        internal static InternalPrimitiveTypeE ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (codeA == null)
            {
                InitCodeA();
            }
            return codeA[(int)typeCode];
        }

        private static void InitCodeA()
        {
            codeA = new InternalPrimitiveTypeE[] { 
        InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Boolean, InternalPrimitiveTypeE.Char, InternalPrimitiveTypeE.SByte, InternalPrimitiveTypeE.Byte, InternalPrimitiveTypeE.Int16, InternalPrimitiveTypeE.UInt16, InternalPrimitiveTypeE.Int32, InternalPrimitiveTypeE.UInt32, InternalPrimitiveTypeE.Int64, InternalPrimitiveTypeE.UInt64, InternalPrimitiveTypeE.Single, InternalPrimitiveTypeE.Double, InternalPrimitiveTypeE.Decimal, 
        InternalPrimitiveTypeE.DateTime, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid
     };
        }

        internal static object FromString(string value, InternalPrimitiveTypeE code)
        {
            if (code != InternalPrimitiveTypeE.Invalid)
            {
                return Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture);
            }
            return value;
        }

        internal static TypeCode ToTypeCode(InternalPrimitiveTypeE code)
        {
            if (typeCodeA == null)
            {
                InitTypeCodeA();
            }
            return typeCodeA[(int)code];
        }

        private static void InitTypeCodeA()
        {
            TypeCode[] codeArray = new TypeCode[primitiveTypeEnumLength];
            codeArray[0] = TypeCode.Object;
            codeArray[1] = TypeCode.Boolean;
            codeArray[2] = TypeCode.Byte;
            codeArray[3] = TypeCode.Char;
            codeArray[5] = TypeCode.Decimal;
            codeArray[6] = TypeCode.Double;
            codeArray[7] = TypeCode.Int16;
            codeArray[8] = TypeCode.Int32;
            codeArray[9] = TypeCode.Int64;
            codeArray[10] = TypeCode.SByte;
            codeArray[11] = TypeCode.Single;
            codeArray[12] = TypeCode.Object;
            codeArray[13] = TypeCode.DateTime;
            codeArray[14] = TypeCode.UInt16;
            codeArray[15] = TypeCode.UInt32;
            codeArray[0x10] = TypeCode.UInt64;
            typeCodeA = codeArray;
        }

 


 



    }
}
