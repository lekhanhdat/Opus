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

namespace LS.BinarySerialization
{
    [Serializable]
    internal enum InternalMemberTypeE
    {
        Empty,
        Header,
        Field,
        Item
    }

    [Serializable]
    internal enum InternalMemberValueE
    {
        Empty,
        InlineValue,
        Nested,
        Reference,
        Null
    }

    [Serializable]
    public enum InternalObjectTypeE
    {
        Empty,
        Object,
        Array
    }

    [Serializable]
    internal enum BinaryArrayTypeEnum
    {
        Single,
        Jagged,
        Rectangular,
        SingleOffset,
        JaggedOffset,
        RectangularOffset
    }

    [Serializable]
    internal enum BinaryTypeEnum
    {
        Primitive,
        String,
        Object,
        ObjectUrt,
        ObjectUser,
        ObjectArray,
        StringArray,
        PrimitiveArray
    }

    [Serializable]
    internal enum InternalSerializerTypeE
    {
        Binary = 2,
        Soap = 1
    }

    [Serializable]
    internal enum BinaryHeaderEnum
    {
        SerializedStreamHeader,
        Object,
        ObjectWithMap,
        ObjectWithMapAssemId,
        ObjectWithMapTyped,
        ObjectWithMapTypedAssemId,
        ObjectString,
        Array,
        MemberPrimitiveTyped,
        MemberReference,
        ObjectNull,
        MessageEnd,
        Assembly,
        ObjectNullMultiple256,
        ObjectNullMultiple,
        ArraySinglePrimitive,
        ArraySingleObject,
        ArraySingleString,
        CrossAppDomainMap,
        CrossAppDomainString,
        CrossAppDomainAssembly,
        MethodCall,
        MethodReturn,
        Invalid
    }

    [Serializable]
    internal enum InternalPrimitiveTypeE
    {
        Invalid,
        Boolean,
        Byte,
        Char,
        Currency,
        Decimal,
        Double,
        Int16,
        Int32,
        Int64,
        SByte,
        Single,
        TimeSpan,
        DateTime,
        UInt16,
        UInt32,
        UInt64,
        Null,
        String
    }

    [Serializable]
    public enum InternalPrimitiveTypeEx
    {
        Invalid,
        Boolean,
        Byte,
        Char,
        Currency,
        Decimal,
        Double,
        Int16,
        Int32,
        Int64,
        SByte,
        Single,
        TimeSpan,
        DateTime,
        UInt16,
        UInt32,
        UInt64,
        Null,
        String,
        ByteArray,
        ObjectString,
        CrossAppDomainString,
        Class,
        MemberReference,
        MemberNested,
        ArraySinglePrimitive,
        Array,
        ArraySingleObject,
        ArraySingleString,
        ArrayOthers
    }

    [Serializable]
    internal enum InternalParseTypeE
    {
        Empty,
        SerializedStreamHeader,
        Object,
        Member,
        ObjectEnd,
        MemberEnd,
        Headers,
        HeadersEnd,
        SerializedStreamHeaderEnd,
        Envelope,
        EnvelopeEnd,
        Body,
        BodyEnd
    }

    [Serializable]
    internal enum InternalArrayTypeE
    {
        Empty,
        Single,
        Jagged,
        Rectangular,
        Base64
    }

    [Serializable]
    internal enum InternalObjectPositionE
    {
        Empty,
        Top,
        Child,
        Headers
    }

    [Serializable, Flags]
    internal enum MessageEnum
    {
        ArgsInArray = 8,
        ArgsInline = 2,
        ArgsIsArray = 4,
        ContextInArray = 0x40,
        ContextInline = 0x20,
        ExceptionInArray = 0x2000,
        GenericMethod = 0x8000,
        MethodSignatureInArray = 0x80,
        NoArgs = 1,
        NoContext = 0x10,
        NoReturnValue = 0x200,
        PropertyInArray = 0x100,
        ReturnValueInArray = 0x1000,
        ReturnValueInline = 0x800,
        ReturnValueVoid = 0x400
    }

    [Serializable]
    internal enum PermissionType
    {
        EnvironmentPermission = 10,
        FileDialogPermission = 11,
        FileIOPermission = 12,
        FullTrust = 7,
        ReflectionMemberAccess = 4,
        ReflectionPermission = 13,
        ReflectionRestrictedMemberAccess = 6,
        ReflectionTypeInfo = 2,
        SecurityAssert = 3,
        SecurityBindingRedirects = 8,
        SecurityControlEvidence = 0x10,
        SecurityControlPrincipal = 0x11,
        SecurityPermission = 14,
        SecuritySerialization = 5,
        SecuritySkipVerification = 1,
        SecurityUnmngdCodeAccess = 0,
        UIPermission = 9
    }

    [Serializable]
    internal enum ValueFixupEnum
    {
        Empty,
        Array,
        Header,
        Member
    }

    [Serializable]
    internal enum StackCrawlMark
    {
        LookForMe,
        LookForMyCaller,
        LookForMyCallersCaller,
        LookForThread
    }








    internal sealed class SerializationHeaderRecord
    {
        internal Int32 binaryFormatterMajorVersion = 1;
        internal Int32 binaryFormatterMinorVersion = 0;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal Int32 topId;
        internal Int32 headerId;
        internal Int32 majorVersion;
        internal Int32 minorVersion;

        internal SerializationHeaderRecord()
        {
        }

        internal SerializationHeaderRecord(BinaryHeaderEnum binaryHeaderEnum, Int32 topId, Int32 headerId, Int32 majorVersion, Int32 minorVersion)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
            this.topId = topId;
            this.headerId = headerId;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        //public void Write(LSBinaryWriter sout)
        //{
        //    majorVersion = binaryFormatterMajorVersion;
        //    minorVersion = binaryFormatterMinorVersion;
        //    sout.WriteByte((Byte)binaryHeaderEnum);
        //    sout.WriteInt32(topId);
        //    sout.WriteInt32(headerId);
        //    sout.WriteInt32(binaryFormatterMajorVersion);
        //    sout.WriteInt32(binaryFormatterMinorVersion);
        //}

        private static int GetInt32(byte[] buffer, int index)
        {
            return (int)(buffer[index] | buffer[index + 1] << 8 | buffer[index + 2] << 16 | buffer[index + 3] << 24);
        }

        public void Read(LSBinaryParser input)
        {
            byte[] headerBytes = input.ReadBytes(17);
            // Throw if we couldnt read header bytes 
            if (headerBytes.Length < 17)
               throw new Exception("End of stream");

            majorVersion = GetInt32(headerBytes, 9);
            if (majorVersion > binaryFormatterMajorVersion)
                throw new Exception(String.Format(CultureInfo.CurrentCulture, "Serialization_InvalidFormat{0}", BitConverter.ToString(headerBytes)));
            
            // binaryHeaderEnum has already been read
            binaryHeaderEnum = (BinaryHeaderEnum)headerBytes[0];
            topId = GetInt32(headerBytes, 1);
            headerId = GetInt32(headerBytes, 5);
            minorVersion = GetInt32(headerBytes, 13);
        }

    }

    internal static class IOUtil
    {
        internal static bool FlagTest(MessageEnum flag, MessageEnum target)
        {
            return ((flag & target) == target);
        }

        internal static object[] ReadArgs(LSBinaryParser input)
        {
            int num = input.ReadInt32();
            object[] objArray = new object[num];
            for (int i = 0; i < num; i++)
            {
                objArray[i] = ReadWithCode(input);
            }
            return objArray;
        }

        internal static object ReadWithCode(LSBinaryParser input)
        {
            InternalPrimitiveTypeE code = (InternalPrimitiveTypeE)input.ReadByte();
            switch (code)
            {
                case InternalPrimitiveTypeE.Null:
                    return null;

                case InternalPrimitiveTypeE.String:
                    return input.ReadString();
            }
            return input.ReadValue(code);
        }


    }

    
}