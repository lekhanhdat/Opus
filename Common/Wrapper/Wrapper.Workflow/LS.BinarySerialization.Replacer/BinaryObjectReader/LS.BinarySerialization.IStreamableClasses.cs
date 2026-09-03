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
using System.Linq;
using System.Text;

using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
namespace LS.BinarySerialization
{
    internal sealed class BinaryArray
    {
        internal int assemId;
        internal BinaryArrayTypeEnum binaryArrayTypeEnum;
        private BinaryHeaderEnum binaryHeaderEnum;
        internal BinaryTypeEnum binaryTypeEnum;
        internal int[] lengthA;
        internal int[] lowerBoundA;
        internal int objectId;
        internal int rank;
        internal object typeInformation;

        internal BinaryArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        public void Read(LSBinaryParser input)
        {
            switch (this.binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.Primitive;
                    this.typeInformation = (InternalPrimitiveTypeE)input.ReadByte();
                    return;

                case BinaryHeaderEnum.ArraySingleObject:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.Object;
                    this.typeInformation = null;
                    return;

                case BinaryHeaderEnum.ArraySingleString:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.String;
                    this.typeInformation = null;
                    return;
            }
            this.objectId = input.ReadInt32();
            this.binaryArrayTypeEnum = (BinaryArrayTypeEnum)input.ReadByte();
            this.rank = input.ReadInt32();
            this.lengthA = new int[this.rank];
            this.lowerBoundA = new int[this.rank];
            for (int i = 0; i < this.rank; i++)
            {
                this.lengthA[i] = input.ReadInt32();
            }
            if (((this.binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset)) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
            {
                for (int j = 0; j < this.rank; j++)
                {
                    this.lowerBoundA[j] = input.ReadInt32();
                }
            }
            this.binaryTypeEnum = (BinaryTypeEnum)input.ReadByte();
            this.typeInformation = BinaryConverter.ReadTypeInfo(this.binaryTypeEnum, input, out this.assemId);
        }
    }

    internal sealed class BinaryCrossAppDomainMap
    {
        // Fields
        internal int crossAppDomainArrayIndex;

        public void Read(LSBinaryParser input)
        {
            this.crossAppDomainArrayIndex = input.ReadInt32();
        }

        public void Dump()
        {
        }
    }

    internal sealed class BinaryCrossAppDomainString
    {
        internal int objectId;
        internal int value;

        public void Read(LSBinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.value = input.ReadInt32();
        }

        public void Dump()
        {
        }
    }

    internal sealed class BinaryObject
    {
        // Fields
        internal int mapId;
        internal int objectId;

        public void Read(LSBinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.mapId = input.ReadInt32();
        }

        public void Dump()
        { }

    }

    internal sealed class BinaryObjectString
    {
        internal int objectId;
        internal string value;

        public void Read(LSBinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.value = input.ReadString();
        }

        public void Dump()
        {
        }
    }

    internal sealed class BinaryObjectWithMap
    {
        internal int assemId;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal string[] memberNames;
        internal string name;
        internal int numMembers;
        internal int objectId;

        internal BinaryObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        public void Read(LSBinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.name = input.ReadString();
            this.numMembers = input.ReadInt32();
            this.memberNames = new string[this.numMembers];
            for (int i = 0; i < this.numMembers; i++)
            {
                this.memberNames[i] = input.ReadString();
            }
            if (this.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                this.assemId = input.ReadInt32();
            }
        }

        public void Dump()
        {
        }
    }

    internal sealed class BinaryObjectWithMapTyped
    {
        internal int assemId;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal int[] memberAssemIds;
        internal string[] memberNames;
        internal string name;
        internal int numMembers;
        internal int objectId;
        internal object[] typeInformationA;

        internal BinaryObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        public void Read(LSBinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.name = input.ReadString();
            this.numMembers = input.ReadInt32();
            this.memberNames = new string[this.numMembers];
            this.binaryTypeEnumA = new BinaryTypeEnum[this.numMembers];
            this.typeInformationA = new object[this.numMembers];
            this.memberAssemIds = new int[this.numMembers];
            for (int i = 0; i < this.numMembers; i++)
            {
                this.memberNames[i] = input.ReadString();
            }
            for (int j = 0; j < this.numMembers; j++)
            {
                this.binaryTypeEnumA[j] = (BinaryTypeEnum)input.ReadByte();
            }
            for (int k = 0; k < this.numMembers; k++)
            {
                if ((this.binaryTypeEnumA[k] != BinaryTypeEnum.ObjectUrt) && (this.binaryTypeEnumA[k] != BinaryTypeEnum.ObjectUser))
                {
                    this.typeInformationA[k] = BinaryConverter.ReadTypeInfo(this.binaryTypeEnumA[k], input, out this.memberAssemIds[k]);
                }
                else
                {
                    BinaryConverter.ReadTypeInfo(this.binaryTypeEnumA[k], input, out this.memberAssemIds[k]);
                }
            }
            if (this.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                this.assemId = input.ReadInt32();
            }
        }


    }

    internal sealed class MemberPrimitiveTyped
    {
        internal InternalPrimitiveTypeE primitiveTypeEnum;
        internal object value;

        public void Read(LSBinaryParser input)
        {
            this.primitiveTypeEnum = (InternalPrimitiveTypeE)input.ReadByte();
            this.value = input.ReadValue(this.primitiveTypeEnum);
        }

        public void Dump()
        {
        }
    }

    internal sealed class MemberReference
    {
        internal int idRef;

        public void Read(LSBinaryParser input)
        {
            this.idRef = input.ReadInt32();
        }

        public void Dump()
        {
        }
    }

    internal sealed class ObjectNull
    {
        internal int nullCount;

        public void Read(LSBinaryParser input)
        {
            this.Read(input, BinaryHeaderEnum.ObjectNull);
        }

        public void Read(LSBinaryParser input, BinaryHeaderEnum binaryHeaderEnum)
        {
            switch (binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ObjectNull:
                    this.nullCount = 1;
                    return;

                case BinaryHeaderEnum.MessageEnd:
                case BinaryHeaderEnum.Assembly:
                    break;

                case BinaryHeaderEnum.ObjectNullMultiple256:
                    this.nullCount = input.ReadByte();
                    return;

                case BinaryHeaderEnum.ObjectNullMultiple:
                    this.nullCount = input.ReadInt32();
                    break;

                default:
                    return;
            }
        }

        public void Dump()
        {
        }
    }

    internal sealed class BinaryMethodCall
    {
        // Fields
        private object[] args;
        private Type[] argTypes;
        private bool bArgsPrimitive;
        private object[] callA;
        private object callContext;
        private Type[] instArgs;
        private MessageEnum messageEnum;
        private string methodName;
        private object methodSignature;
        private object properties;
        private string scallContext;
        private string typeName;
        private string uri;

        public BinaryMethodCall()
        {
            this.bArgsPrimitive = true;
        }

        internal void Read(LSBinaryParser input)
        {
            this.messageEnum = (MessageEnum)input.ReadInt32();
            this.methodName = (string)IOUtil.ReadWithCode(input);
            this.typeName = (string)IOUtil.ReadWithCode(input);
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInline))
            {
                this.scallContext = (string)IOUtil.ReadWithCode(input);
                //LogicalCallContext context = new LogicalCallContext();
                //context.RemotingData.LogicalCallID = this.scallContext;
                //this.callContext = context;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInline))
            {
                this.args = IOUtil.ReadArgs(input);
            }
        }

        /*
        internal IMethodCallMessage ReadArray(object[] callA, object handlerObject)
        {
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsIsArray))
            {
                this.args = callA;
            }
            else
            {
                int num = 0;
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Method"), new object[0]));
                    }
                    this.args = (object[])callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.GenericMethod))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Method"), new object[0]));
                    }
                    this.instArgs = (Type[])callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.MethodSignatureInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Method"), new object[0]));
                    }
                    this.methodSignature = callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Method"), new object[0]));
                    }
                    this.callContext = callA[num++];
                }
                if (IOUtil.FlagTest(this.messageEnum, MessageEnum.PropertyInArray))
                {
                    if (callA.Length < num)
                    {
                        throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Method"), new object[0]));
                    }
                    this.properties = callA[num++];
                }
            }
            return new MethodCall(handlerObject, new BinaryMethodCallMessage(this.uri, this.methodName, this.typeName, this.instArgs, this.args, this.methodSignature, (LogicalCallContext)this.callContext, (object[])this.properties));
        }
        */

        internal void Dump()
        {
        }

 

    }

    internal sealed class BinaryMethodReturn
    {
        // Fields
        private object[] args;
        private Type[] argTypes;
        private bool bArgsPrimitive;
        private object[] callA;
        private object callContext;
        private Exception exception;
        private static object instanceOfVoid;
        private MessageEnum messageEnum;
        private object properties;
        private Type returnType;
        private object returnValue;
        private string scallContext;

        static BinaryMethodReturn()
        {
            instanceOfVoid = FormatterServices.GetUninitializedObject(Converter.typeofSystemVoid);
        }

        internal BinaryMethodReturn()
        {
            this.bArgsPrimitive = true;
        }

        public void Read(LSBinaryParser input)
        {
            this.messageEnum = (MessageEnum)input.ReadInt32();
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.NoReturnValue))
            {
                this.returnValue = null;
            }
            else if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueVoid))
            {
                this.returnValue = instanceOfVoid;
            }
            else if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ReturnValueInline))
            {
                this.returnValue = IOUtil.ReadWithCode(input);
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ContextInline))
            {
                this.scallContext = (string)IOUtil.ReadWithCode(input);
                //LogicalCallContext context = new LogicalCallContext();
                //context.RemotingData.LogicalCallID = this.scallContext;
                //this.callContext = context;
            }
            if (IOUtil.FlagTest(this.messageEnum, MessageEnum.ArgsInline))
            {
                this.args = IOUtil.ReadArgs(input);
            }
        }

        public void Dump()
        {
        }

 


    }

    internal sealed class MessageEnd
    {
        public void Read(LSBinaryParser input)
        {
        }

        public void Dump()
        {
        }

        public void Dump(Stream sout)
        {
        }
    }

    internal sealed class MemberPrimitiveUnTyped
    {
        // Fields
        internal InternalPrimitiveTypeE typeInformation;
        internal object value;

        public void Read(LSBinaryParser input)
        {
            this.value = input.ReadValue(this.typeInformation);
        }

        internal void Set(InternalPrimitiveTypeE typeInformation)
        {
            this.typeInformation = typeInformation;
        }

        internal void Set(InternalPrimitiveTypeE typeInformation, object value)
        {
            this.typeInformation = typeInformation;
            this.value = value;
        }

        public void Dump()
        {
        }
    }
}
