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

using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
namespace LS.BinarySerialization
{
    internal static class BinaryConverter
    {
        internal static object ReadTypeInfo(BinaryTypeEnum binaryTypeEnum, LSBinaryParser input, out int assemId)
        {
            object obj2 = null;
            int num = 0;
            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                case BinaryTypeEnum.PrimitiveArray:
                    obj2 = (InternalPrimitiveTypeE)input.ReadByte();
                    break;

                case BinaryTypeEnum.String:
                case BinaryTypeEnum.Object:
                case BinaryTypeEnum.ObjectArray:
                case BinaryTypeEnum.StringArray:
                    break;

                case BinaryTypeEnum.ObjectUrt:
                    obj2 = input.ReadString();
                    break;

                case BinaryTypeEnum.ObjectUser:
                    obj2 = input.ReadString();
                    num = input.ReadInt32();
                    break;

                default:
                    throw new Exception("Serialization_TypeRead:"+ binaryTypeEnum.ToString());
            }
            assemId = num;
            return obj2;
        }

        internal static BinaryTypeEnum GetParserBinaryTypeInfo(Type type, out object typeInformation)
        {
            Type typeofBinaryConverter = Type.GetType("System.Runtime.Serialization.Formatters.Binary.BinaryConverter");

            typeInformation=null;
            object[] refArgs = new object[] { type,typeInformation };
            int value = (int)LSInvoker.CallStaticMethod(typeofBinaryConverter, "GetParserBinaryTypeInfo", refArgs);
            typeInformation = refArgs[1];
            return (BinaryTypeEnum)value;
        }

        internal static void TypeFromInfo(BinaryTypeEnum binaryTypeEnum, object typeInformation, LSObjectReader objectReader, BinaryAssemblyInfo assemblyInfo, out InternalPrimitiveTypeE primitiveTypeEnum, out string typeString, out Type type, out bool isVariant)
        {
            isVariant = false;
            primitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
            typeString = null;
            type = null;
            switch (binaryTypeEnum)
            {
                case BinaryTypeEnum.Primitive:
                    primitiveTypeEnum = (InternalPrimitiveTypeE)typeInformation;
                    typeString = Converter.ToComType(primitiveTypeEnum);
                    type = Converter.ToType(primitiveTypeEnum);
                    return;

                case BinaryTypeEnum.String:
                    type = Converter.typeofString;
                    return;

                case BinaryTypeEnum.Object:
                    type = Converter.typeofObject;
                    isVariant = true;
                    return;

                case BinaryTypeEnum.ObjectUrt:
                case BinaryTypeEnum.ObjectUser:
                    if (typeInformation == null)
                    {
                        break;
                    }
                    typeString = typeInformation.ToString();
                    type = objectReader.GetType(assemblyInfo, typeString);
                    if (type != Converter.typeofObject)
                    {
                        break;
                    }
                    isVariant = true;
                    return;

                case BinaryTypeEnum.ObjectArray:
                    type = Converter.typeofObjectArray;
                    return;

                case BinaryTypeEnum.StringArray:
                    type = Converter.typeofStringArray;
                    return;

                case BinaryTypeEnum.PrimitiveArray:
                    primitiveTypeEnum = (InternalPrimitiveTypeE)typeInformation;
                    type = Converter.ToArrayType(primitiveTypeEnum);
                    return;

                default:
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_TypeRead"), new object[] { binaryTypeEnum.ToString() }));
            }
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


    }
}
