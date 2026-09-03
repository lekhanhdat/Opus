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




#define _ObjectParse
#define GenerateTree


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Collections;
using AvePoint.Wrapper.Common;

namespace LS.BinarySerialization
{

    internal sealed class SerStack
    {
        internal Object[] objects = new Object[5];
        internal String stackId;
        internal int top = -1;
        internal int next = 0;

        private Type parentType;
        private Object parentObject;

        public Object Parent
        {
            get
            {
                LSInvoker.SetRawProperty(parentObject, "objects", objects);
                LSInvoker.SetRawProperty(parentObject, "stackId", stackId);
                LSInvoker.SetRawProperty(parentObject, "top", top);
                LSInvoker.SetRawProperty(parentObject, "next", next);
                return parentObject;
            }

        }

        private void CreateParentInstance(string id)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfo[] cstrs = parentType.GetConstructors(flags);

            ConstructorInfo cstr = cstrs[0];
            foreach (ConstructorInfo c in cstrs)
            {
                if (c.GetParameters().Length == 1)
                {
                    cstr = c;
                    break;
                }
            }
            parentObject = cstr.Invoke(new object[] { id });

        }

        internal SerStack()
        {
            stackId = "System";
            parentType = Type.GetType("System.Runtime.Serialization.Formatters.Binary.SerStack");
            CreateParentInstance("System");
        }

        internal SerStack(String stackId)
        {
            this.stackId = stackId;
            parentType = Type.GetType("System.Runtime.Serialization.Formatters.Binary.SerStack");
            CreateParentInstance(stackId);
        }

        // Push the object onto the stack
        internal void Push(Object obj)
        {
#if _DEBUG 
            SerTrace.Log(this, "Push ",stackId," ",((obj is ITrace)?((ITrace)obj).Trace():""));
#endif
            if (top == (objects.Length - 1))
            {
                IncreaseCapacity();
            }
            objects[++top] = obj;
        }

        // Pop the object from the stack
        internal Object Pop()
        {
            if (top < 0)
                return null;

            Object obj = objects[top];
            objects[top--] = null;
#if _DEBUG
            SerTrace.Log(this, "Pop ",stackId," ",((obj is ITrace)?((ITrace)obj).Trace():"")); 
#endif
            return obj;
        }

        internal void IncreaseCapacity()
        {
            int size = objects.Length * 2;
            Object[] newItems = new Object[size];
            Array.Copy(objects, 0, newItems, 0, objects.Length);
            objects = newItems;
        }

        // Gets the object on the top of the stack 
        internal Object Peek()
        {
            if (top < 0)
                return null;
#if _DEBUG
            SerTrace.Log(this, "Peek ",stackId," ",((objects[top] is ITrace)?((ITrace)objects[top]).Trace():""));
#endif
            return objects[top];
        }

        // Gets the second entry in the stack.
        internal Object PeekPeek()
        {
            if (top < 1)
                return null;
#if _DEBUG
            SerTrace.Log(this, "PeekPeek ",stackId," ",((objects[top - 1] is ITrace)?((ITrace)objects[top - 1]).Trace():"")); 
#endif
            return objects[top - 1];
        }

        // The number of entries in the stack 
        internal int Count()
        {
            return top + 1;
        }

        // The number of entries in the stack
        internal bool IsEmpty()
        {
            if (top > 0)
                return false;
            else
                return true;
        }

        internal void Dump()
        {
            for (int i = 0; i < Count(); i++)
            {
                Object obj = objects[i];
            }
        }
    }

    public class LSBinaryParser : IDisposable
    {
        BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        internal Object expectedTypeInformation;

        SerStack stack;
        private SerStack opPool;
        Stream input;
        LSObjectReader objectReader;
        BinaryReader dataReader;
        Type BinaryHeaderEnumCls;
        internal long headerId;
        internal long topId;
        internal SizedArray objectMapIdTable;
        internal SizedArray assemIdToAssemblyTable;	// Used to hold assembly information





        internal BinaryObjectString objectString = null;
        internal BinaryCrossAppDomainString crossAppDomainString = null;
        internal MemberReference memberReference = null;
        internal MemberPrimitiveTyped memberPrimitiveTyped = null;
        internal ObjectNull objectNull = null;
        internal BinaryObject binaryObject = null;
        internal BinaryObjectWithMap bowm;
        internal MessageEnd messageEnd;
        internal MemberPrimitiveUnTyped memberPrimitiveUnTyped;



        internal ISurrogateSelector m_surrogates;
        internal StreamingContext m_context;
        internal SerializationBinder m_binder;

        private static Encoding encoding = new UTF8Encoding(false, true);



        //For Analyze 
        private LSObjectNode CurrentObjectNode;
        private long position;
        //

        private LSObjectNodeCollection lsObjectNodeCollection;
        internal LSObjectNodeCollection LSObjectNodeCollection
        {
            get
            {
                if (lsObjectNodeCollection == null)
                    lsObjectNodeCollection = new LSObjectNodeCollection();
                return lsObjectNodeCollection;
            }
        }


        internal ParseRecord PRS;
        internal ParseRecord prs
        {
            get
            {
                if (PRS == null)
                    PRS = new ParseRecord();
                return PRS;
            }
        }

        private BinaryAssemblyInfo systemAssemblyInfo;
        internal BinaryAssemblyInfo SystemAssemblyInfo
        {
            get
            {
                if (systemAssemblyInfo == null)
                {
                    Assembly urtAssembly = Assembly.GetAssembly(typeof(string));
                    string urtAssemblyString = urtAssembly.FullName;

                    systemAssemblyInfo = new BinaryAssemblyInfo(urtAssemblyString, urtAssembly);
                }
                return systemAssemblyInfo;
            }
        }

        internal SizedArray AssemIdToAssemblyTable
        {
            get
            {
                if (this.assemIdToAssemblyTable == null)
                {
                    this.assemIdToAssemblyTable = new SizedArray(2);
                }
                return this.assemIdToAssemblyTable;
            }
        }

        internal SizedArray ObjectMapIdTable
        {
            get
            {
                if (this.objectMapIdTable == null)
                {
                    this.objectMapIdTable = new SizedArray();
                }
                return this.objectMapIdTable;
            }
        }

        public LSBinaryParser(Stream stream, LSObjectReader objectReader)
        {
            this.stack = new SerStack("ObjectProgressStack");
            this.expectedType = BinaryTypeEnum.ObjectUrt;
            this.input = stream;
            this.objectReader = objectReader;
            this.dataReader = new BinaryReader(this.input, encoding);
            this.lsObjectNodeCollection = new LSObjectNodeCollection();

            BinaryHeaderEnumCls = Type.GetType("System.Runtime.Serialization.Formatters.Binary.BinaryHeaderEnum");
            Array vals = Enum.GetValues(BinaryHeaderEnumCls);
        }

        internal void Run()
        {
            int loopCount = 0;
            bool isLoop = true;

            Stream internalStream = (Stream)LSInvoker.GetRawProperty(dataReader, "m_stream");
            ReadBegin();
            ReadSerializationHeaderRecord();

            #region Loop
            while (isLoop)
            {
                BinaryHeaderEnum binaryHeaderEnum = BinaryHeaderEnum.Object;
                loopCount++;


                position = internalStream.Position;
                //StreamWriter sw = new StreamWriter(@"c:\SerPosition.txt");
                //sw.WriteLine(position.ToString());
                //sw.Close();

                switch (expectedType)
                {
                    case BinaryTypeEnum.ObjectUrt:
                    case BinaryTypeEnum.ObjectUser:
                    case BinaryTypeEnum.String:
                    case BinaryTypeEnum.Object:
                    case BinaryTypeEnum.ObjectArray:
                    case BinaryTypeEnum.StringArray:
                    case BinaryTypeEnum.PrimitiveArray:
                        Byte inByte = dataReader.ReadByte();
                        binaryHeaderEnum = (BinaryHeaderEnum)inByte;
                        Console.WriteLine("Beginning of loop " + ((Enum)binaryHeaderEnum).ToString());
                        switch (binaryHeaderEnum)
                        {
                            case BinaryHeaderEnum.Assembly:
                            case BinaryHeaderEnum.CrossAppDomainAssembly:
                                ReadAssembly(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.Object:
                                ReadObject();
                                break;
                            case BinaryHeaderEnum.CrossAppDomainMap:
                                ReadCrossAppDomainMap();
                                break;
                            case BinaryHeaderEnum.ObjectWithMap:
                            case BinaryHeaderEnum.ObjectWithMapAssemId:
                                ReadObjectWithMap(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.ObjectWithMapTyped:
                            case BinaryHeaderEnum.ObjectWithMapTypedAssemId:
                                ReadObjectWithMapTyped(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.MethodCall:
                            case BinaryHeaderEnum.MethodReturn:
                                ReadMethodObject(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.ObjectString:
                            case BinaryHeaderEnum.CrossAppDomainString:
                                ReadObjectString(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.Array:
                            case BinaryHeaderEnum.ArraySinglePrimitive:
                            case BinaryHeaderEnum.ArraySingleObject:
                            case BinaryHeaderEnum.ArraySingleString:
                                ReadArray(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.MemberPrimitiveTyped:
                                ReadMemberPrimitiveTyped();
                                break;
                            case BinaryHeaderEnum.MemberReference:
                                ReadMemberReference();
                                break;
                            case BinaryHeaderEnum.ObjectNull:
                            case BinaryHeaderEnum.ObjectNullMultiple256:
                            case BinaryHeaderEnum.ObjectNullMultiple:
                                ReadObjectNull(binaryHeaderEnum);
                                break;
                            case BinaryHeaderEnum.MessageEnd:
                                isLoop = false;
                                ReadMessageEnd();
                                ReadEnd(loopCount);
                                break;
                            default:
                                throw new Exception(String.Format(CultureInfo.CurrentCulture, "Serialization_BinaryHeader:{0}", inByte));
                        }
                        break;
                    case BinaryTypeEnum.Primitive:
                        ReadMemberPrimitiveUnTyped();
                        break;
                    default:
                        throw new Exception("Serialization_TypeExpected");

                }




                if (binaryHeaderEnum != BinaryHeaderEnum.Assembly)
                {

                    bool isData = false;
                    while (!isData)
                    {
                        ObjectProgress op = (ObjectProgress)stack.Peek();
                        if (op == null)
                        {
                            expectedType = BinaryTypeEnum.ObjectUrt;
                            expectedTypeInformation = null;
                            isData = true;
                        }
                        else
                        {
                            isData = op.GetNext(out op.expectedType, out op.expectedTypeInformation);
                            expectedType = op.expectedType;
                            expectedTypeInformation = op.expectedTypeInformation;
                            if (!isData)
                            {
                                stack.Dump();
                                prs.Init();
                                if (op.memberValueEnum == InternalMemberValueE.Nested)
                                {
                                    // Nested object 
                                    prs.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                                    prs.PRmemberTypeEnum = op.memberTypeEnum;
                                    prs.PRmemberValueEnum = op.memberValueEnum;

#if ObjectParse
                                    objectReader.Parse(prs); 
#endif
#if GenerateTree
                                    lsObjectNodeCollection.AddMemberNode(prs);
#endif
                                }
                                else
                                {
                                    // Top level object
                                    prs.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                                    prs.PRmemberTypeEnum = op.memberTypeEnum;
                                    prs.PRmemberValueEnum = op.memberValueEnum;

#if ObjectParse
                                    objectReader.Parse(prs);
#endif
#if GenerateTree
                                    lsObjectNodeCollection.AddMemberNode(prs);
#endif
                                }
                                stack.Pop();
                                PutOp(op);
                            }
                        }
                    }
                }
            }
            #endregion



        }

        internal void ReadSerializationHeaderRecord()
        {
            SerializationHeaderRecord record = new SerializationHeaderRecord();
            record.Read(this);
            record.Dump();
            this.topId = (record.topId > 0) ? (long)LSInvoker.CallMethod(objectReader, "GetId", new Type[] { typeof(long) }, new object[] { (long)record.topId }) : ((long)record.topId);
            this.headerId = (record.headerId > 0) ? (long)LSInvoker.CallMethod(objectReader, "GetId", new Type[] { typeof(long) }, new object[] { (long)record.headerId }) : ((long)record.headerId);
        }

        internal void ReadBegin()
        {
            Console.WriteLine("BINARY\n%%%%%BinaryReaderBegin%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
        }

        internal void ReadEnd(int loopCount)
        {
            Console.WriteLine("BINARY\n%%%%%BinaryReaderEnd(" + loopCount.ToString() + ")%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
        }

        /* 
         * Primitive Reads from Stream 
         * @internalonly
         */

        internal bool ReadBoolean()
        {
            return dataReader.ReadBoolean();
        }

        internal Byte ReadByte()
        {
            return dataReader.ReadByte();
        }

        internal Byte[] ReadBytes(int length)
        {
            return dataReader.ReadBytes(length);
        }

        // Note: this method does a blocking read!
        internal void ReadBytes(byte[] byteA, int offset, int size)
        {
            while (size > 0)
            {
                int n = dataReader.Read(byteA, offset, size);
                if (n == 0)
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Workflow_EndOfFile);
                offset += n;
                size -= n;
            }
        }

        internal Char ReadChar()
        {
            return dataReader.ReadChar();
        }

        internal Char[] ReadChars(int length)
        {
            return dataReader.ReadChars(length);
        }

        internal Decimal ReadDecimal()
        {
            return Decimal.Parse(dataReader.ReadString(), CultureInfo.InvariantCulture);
        }

        internal Single ReadSingle()
        {
            return dataReader.ReadSingle();
        }

        internal Double ReadDouble()
        {
            return dataReader.ReadDouble();
        }

        internal Int16 ReadInt16()
        {
            return dataReader.ReadInt16();
        }

        internal Int32 ReadInt32()
        {
            return dataReader.ReadInt32();
        }

        internal Int64 ReadInt64()
        {
            return dataReader.ReadInt64();
        }

        internal SByte ReadSByte()
        {
            return (SByte)ReadByte();
        }

        internal String ReadString()
        {
            return dataReader.ReadString();
        }

        internal TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadInt64());
        }

        internal DateTime ReadDateTime()
        {

            return DateTime.FromBinary(ReadInt64());
        }

        internal UInt16 ReadUInt16()
        {
            return dataReader.ReadUInt16();
        }

        internal UInt32 ReadUInt32()
        {
            return dataReader.ReadUInt32();
        }

        internal UInt64 ReadUInt64()
        {
            return dataReader.ReadUInt64();
        }




        //Reflection
        internal void ReadAssembly(BinaryHeaderEnum binaryHeaderEnum)
        {
            int assmId = 0;
            string assmName = string.Empty;
            if (binaryHeaderEnum == BinaryHeaderEnum.CrossAppDomainAssembly)
            {
                assmId = ReadInt32();
                int assmIndex = ReadInt32();
                Dump();
                assmName = LSInvoker.CallMethod(objectReader, "CrossAppDomainArray", new Type[] { typeof(int) }, new object[] { assmIndex }) as string;
            }
            else
            {
                assmId = ReadInt32();
                assmName = ReadString();
                Dump();
            }

            this.AssemIdToAssemblyTable[assmId] = new BinaryAssemblyInfo(assmName);
        }

        private void ReadObject()
        {
            if (binaryObject == null)
                binaryObject = new BinaryObject();
            binaryObject.Read(this);
            binaryObject.Dump();

            ObjectMap objectMap = (ObjectMap)ObjectMapIdTable[binaryObject.mapId];
            if (objectMap == null)
                throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Map"), binaryObject.mapId));

            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);

            op.objectTypeEnum = InternalObjectTypeE.Object;
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;
            op.typeInformationA = objectMap.typeInformationA;
            op.memberLength = op.binaryTypeEnumA.Length;
            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();
            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object
                op.name = objectMap.objectName;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;
            }
            else
            {
                // Nested Object 
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        pr.PRname = objectOp.name;
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        op.memberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    case InternalObjectTypeE.Array:
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        op.memberTypeEnum = InternalMemberTypeE.Item;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_Map"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }
            }


            pr.PRobjectId = objectReader.GetId((long)binaryObject.objectId);
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);

            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;

            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
            pr.PRkeyDt = objectMap.objectName;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;


#if ObjectParse
            objectReader.Parse(pr);
#endif
#if GenerateTree
            //for analyze
            pr.PRobjectPosition = position;
            pr.PRmemberTypes = op.memberTypes;
            pr.PRhasMember = true;
            pr.PRobjectInternalType = InternalPrimitiveTypeEx.Class;
            lsObjectNodeCollection.AddMemberNode(pr);
#endif
        }

        internal void ReadCrossAppDomainMap()
        {
            BinaryCrossAppDomainMap record = new BinaryCrossAppDomainMap();
            record.Read(this);
            record.Dump();
            Object mapObject = objectReader.CrossAppDomainArray(record.crossAppDomainArrayIndex);
            BinaryObjectWithMap binaryObjectWithMap = mapObject as BinaryObjectWithMap;
            if (binaryObjectWithMap != null)
            {
                binaryObjectWithMap.Dump();
                ReadObjectWithMap(binaryObjectWithMap);
            }
            else
            {
                BinaryObjectWithMapTyped binaryObjectWithMapTyped = mapObject as BinaryObjectWithMapTyped;
                if (binaryObjectWithMapTyped != null)
                {
#if _DEBUG				
                    binaryObjectWithMapTyped.Dump(); 
#endif
                    ReadObjectWithMapTyped(binaryObjectWithMapTyped);
                }
                else
                    throw new SerializationException("Serialization_CrossAppDomainError");
            }
        }

        internal void ReadObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (bowm == null)
                bowm = new BinaryObjectWithMap(binaryHeaderEnum);
            else
                bowm.binaryHeaderEnum = binaryHeaderEnum;
            bowm.Read(this);
            bowm.Dump();
            ReadObjectWithMap(bowm);
        }

        private void ReadObjectWithMap(BinaryObjectWithMap record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);


            if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                if (record.assemId < 1)
                    throw new SerializationException("Serialization_Assembly");

                assemblyInfo = ((BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId]);

                if (assemblyInfo == null)
                    throw new SerializationException("Serialization_Assembly");
            }
            else if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMap)
            {

                assemblyInfo = SystemAssemblyInfo; //Urt assembly
            }

            Type objectType = objectReader.GetType(assemblyInfo, record.name);

            ObjectMap objectMap = ObjectMap.Create(record.name, objectType, record.memberNames, objectReader, record.objectId, assemblyInfo);
            ObjectMapIdTable[record.objectId] = objectMap;

            op.objectTypeEnum = InternalObjectTypeE.Object;
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.typeInformationA = objectMap.typeInformationA;
            op.memberLength = op.binaryTypeEnumA.Length;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;

            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();

            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object
                op.name = record.name;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;

            }
            else
            {
                // Nested Object
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        pr.PRname = objectOp.name;
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        op.memberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    case InternalObjectTypeE.Array:
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        op.memberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ObjectTypeEnum"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }

            }
            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
            pr.PRobjectId = objectReader.GetId((long)record.objectId);
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);

            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;

            pr.PRkeyDt = record.name;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;

#if ObjectParse
            objectReader.Parse(pr);
#endif
#if GenerateTree
            pr.PRmemberTypes = op.memberTypes;
            pr.PRobjectPosition = position;
            pr.PRhasMember = true;
            pr.PRobjectInternalType = InternalPrimitiveTypeEx.Class;
            lsObjectNodeCollection.AddMemberNode(pr);
#endif
        }

        internal void ReadObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            BinaryObjectWithMapTyped bowmt = new BinaryObjectWithMapTyped(binaryHeaderEnum);
            bowmt.Read(this);
            ReadObjectWithMapTyped(bowmt);
        }

        private void ReadObjectWithMapTyped(BinaryObjectWithMapTyped record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);

            if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                if (record.assemId < 1)
                    throw new Exception("Serialization_AssemblyId");
                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId];
                if (assemblyInfo == null)
                    throw new Exception("Serialization_AssemblyId");
            }
            else if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTyped)
            {
                assemblyInfo = SystemAssemblyInfo; // Urt assembly
            }
            ObjectMap objectMap = ObjectMap.Create(record.name, record.memberNames, record.binaryTypeEnumA, record.typeInformationA, record.memberAssemIds, objectReader, record.objectId, assemblyInfo, AssemIdToAssemblyTable);
            ObjectMapIdTable[record.objectId] = objectMap;
            op.objectTypeEnum = InternalObjectTypeE.Object;
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.typeInformationA = objectMap.typeInformationA;
            op.memberLength = op.binaryTypeEnumA.Length;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;
            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();

            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object 
                op.name = record.name;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;
            }
            else
            {
                // Nested Object 
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        pr.PRname = objectOp.name;
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        op.memberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    case InternalObjectTypeE.Array:
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        op.memberTypeEnum = InternalMemberTypeE.Item;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ObjectTypeEnum"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }

            }
            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);
            pr.PRobjectId = objectReader.GetId((long)record.objectId);
            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
            pr.PRkeyDt = record.name;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;

#if ObjectParse
            objectReader.Parse(pr); 
#endif
#if GenerateTree
            pr.PRmemberTypes = op.memberTypes;
            pr.PRobjectPosition = position;
            pr.PRhasMember = true;
            pr.PRobjectInternalType = InternalPrimitiveTypeEx.Class;
            lsObjectNodeCollection.AddMemberNode(pr);
#endif
        }

        internal void ReadMethodObject(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (binaryHeaderEnum == BinaryHeaderEnum.MethodCall)
            {
                BinaryMethodCall record = new BinaryMethodCall();
                record.Read(this);
                record.Dump();
#if ObjectParse
                objectReader.SetMethodCall(record);
#endif
            }
            else
            {
                BinaryMethodReturn record = new BinaryMethodReturn();
                record.Read(this);
                record.Dump();
#if ObjectParse
                objectReader.SetMethodReturn(record);
#endif
            }
        }

        private void ReadObjectString(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (this.objectString == null)
            {
                this.objectString = new BinaryObjectString();
            }
            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectString)
            {
                this.objectString.Read(this);
                this.objectString.Dump();
            }
            else
            {
                if (crossAppDomainString == null)
                    crossAppDomainString = new BinaryCrossAppDomainString();
                crossAppDomainString.Read(this);
                crossAppDomainString.Dump();
                objectString.value = objectReader.CrossAppDomainArray(crossAppDomainString.value) as String;
                if (objectString.value == null)
                    throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_CrossAppDomainError"), "String", crossAppDomainString.value));

                objectString.objectId = crossAppDomainString.objectId;
            }

            prs.Init();
            prs.PRparseTypeEnum = InternalParseTypeE.Object;
            prs.PRobjectId = objectReader.GetId(objectString.objectId);

            if (prs.PRobjectId == topId)
                prs.PRobjectPositionEnum = InternalObjectPositionE.Top;

            prs.PRobjectTypeEnum = InternalObjectTypeE.Object;

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.PRvalue = objectString.value;
            prs.PRkeyDt = "System.String";
            prs.PRdtType = Converter.typeofString;
            prs.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            prs.PRvarValue = objectString.value; //Need to set it because ObjectReader is picking up value from variant, not pr.PRvalue

            if (objectOp == null)
            {
                // Top level String
                prs.PRparseTypeEnum = InternalParseTypeE.Object;
                prs.PRname = "System.String";
            }
            else
            {
                // Nested in an Object

                prs.PRparseTypeEnum = InternalParseTypeE.Member;
                prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        prs.PRname = objectOp.name;
                        prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    case InternalObjectTypeE.Array:
                        prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ObjectTypeEnum"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }

            }
#if ObjectParse
            objectReader.Parse(prs); 
#endif

#if GenerateTree
            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectString)
                prs.PRobjectInternalType = InternalPrimitiveTypeEx.ObjectString;
            else
                prs.PRobjectInternalType = InternalPrimitiveTypeEx.CrossAppDomainString;
            prs.PRobjectPosition = position;
            prs.PRobjectValuePosition = position + 5;
            prs.PRhasMember = false;
            lsObjectNodeCollection.AddMemberNode(prs);
#endif
        }

        private void ReadArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            BinaryArray record = new BinaryArray(binaryHeaderEnum);
            record.Read(this);
#if _DEBUG 
 			record.Dump(); 

			SerTrace.Log( this, "Read 1 ",((Enum)binaryHeaderEnum).ToString()); 
#endif
            if (record.binaryTypeEnum == BinaryTypeEnum.ObjectUser)
            {
                if (record.assemId < 1)
                    throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_AssemblyId"), record.typeInformation));

                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId];
            }
            else
                assemblyInfo = SystemAssemblyInfo; //Urt assembly

            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;

            op.objectTypeEnum = InternalObjectTypeE.Array;
            op.binaryTypeEnum = record.binaryTypeEnum;
            op.typeInformation = record.typeInformation;

            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();
            if ((objectOp == null) || (record.objectId > 0))
            {
                // Non-Nested Object
                op.name = "System.Array";
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;
            }
            else
            {
                // Nested Object			
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        pr.PRname = objectOp.name;
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        op.memberTypeEnum = InternalMemberTypeE.Field;
                        pr.PRkeyDt = objectOp.name;
                        pr.PRdtType = objectOp.dtType;
                        break;
                    case InternalObjectTypeE.Array:
                        pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        op.memberTypeEnum = InternalMemberTypeE.Item;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ObjectTypeEnum"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }
            }


            pr.PRobjectId = objectReader.GetId((long)record.objectId);
            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
            else if ((headerId > 0) && (pr.PRobjectId == headerId))
                pr.PRobjectPositionEnum = InternalObjectPositionE.Headers; // Headers are an array of header objects 
            else
                pr.PRobjectPositionEnum = InternalObjectPositionE.Child;

            pr.PRobjectTypeEnum = InternalObjectTypeE.Array;

            BinaryConverter.TypeFromInfo(record.binaryTypeEnum, record.typeInformation, objectReader, assemblyInfo,
                                         out pr.PRarrayElementTypeCode, out pr.PRarrayElementTypeString,
                                         out pr.PRarrayElementType, out pr.PRisArrayVariant);

            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;


            pr.PRrank = record.rank;
            pr.PRlengthA = record.lengthA;
            pr.PRlowerBoundA = record.lowerBoundA;
            bool isPrimitiveArray = false;

            switch (record.binaryArrayTypeEnum)
            {
                case BinaryArrayTypeEnum.Single:
                case BinaryArrayTypeEnum.SingleOffset:
                    op.numItems = record.lengthA[0];
                    pr.PRarrayTypeEnum = InternalArrayTypeE.Single;
                    if (Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode) &&
                        (record.lowerBoundA[0] == 0))
                    {
                        isPrimitiveArray = true;
                        pr.PRisPrimitiveArray = true;
                        ReadArrayAsBytes(pr);
                    }
                    break;
                case BinaryArrayTypeEnum.Jagged:
                case BinaryArrayTypeEnum.JaggedOffset:
                    op.numItems = record.lengthA[0];
                    pr.PRarrayTypeEnum = InternalArrayTypeE.Jagged;
                    break;
                case BinaryArrayTypeEnum.Rectangular:
                case BinaryArrayTypeEnum.RectangularOffset:
                    int arrayLength = 1;
                    for (int i = 0; i < record.rank; i++)
                        arrayLength = arrayLength * record.lengthA[i];
                    op.numItems = arrayLength;
                    pr.PRarrayTypeEnum = InternalArrayTypeE.Rectangular;
                    break;
                default:
                    throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ArrayType"), ((Enum)record.binaryArrayTypeEnum).ToString()));
            }

            if (!isPrimitiveArray)
                stack.Push(op);
            else
            {
                PutOp(op);
            }
#if ObjectParse
            objectReader.Parse(pr);
#endif
#if GenerateTree
            if (binaryHeaderEnum == BinaryHeaderEnum.ArraySinglePrimitive)
            {
                pr.PRobjectArrayType = InternalPrimitiveTypeEx.ArraySinglePrimitive;
                //array(1 byte)+objectid(4 byte)+len(4 byte)+type(1 byte)+value
                pr.PRobjectValuePosition = position + 10;
            }
            else if (binaryHeaderEnum == BinaryHeaderEnum.ArraySingleObject)
            {
                pr.PRobjectArrayType = InternalPrimitiveTypeEx.ArraySingleObject;
                //array(1 byte)+objectid(4 byte)+len(4 byte)+value
                pr.PRobjectValuePosition = position + 9;
            }
            else if (binaryHeaderEnum == BinaryHeaderEnum.ArraySingleString)
            {
                pr.PRobjectArrayType = InternalPrimitiveTypeEx.ArraySingleString;
                //array(1 byte)+objectid(4 byte)+len(4 byte)+value
                pr.PRobjectValuePosition = position + 9;
            }
            else
            {
                pr.PRobjectArrayType = record.typeInformation;
            }
            pr.PRhasMember = false;
            pr.PRobjectInternalType = InternalPrimitiveTypeEx.Array;
            pr.PRobjectPosition = position;
            lsObjectNodeCollection.AddMemberNode(pr);
#endif
            if (isPrimitiveArray)
            {
                pr.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
#if ObjectParse
                objectReader.Parse(pr);
#endif
#if GenerateTree
                lsObjectNodeCollection.AddMemberNode(pr);
#endif
            }
        }

        private byte[] byteBuffer;
        private const int chunkSize = 4096;
        private void ReadArrayAsBytes(ParseRecord pr)
        {
            if (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Byte)
                pr.PRnewObj = ReadBytes(pr.PRlengthA[0]);
            else if (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Char)
                pr.PRnewObj = ReadChars(pr.PRlengthA[0]);
            else
            {
                int typeLength = Converter.TypeLength(pr.PRarrayElementTypeCode);

                pr.PRnewObj = Converter.CreatePrimitiveArray(pr.PRarrayElementTypeCode, pr.PRlengthA[0]);

                Array array = (Array)pr.PRnewObj;
                int arrayOffset = 0;
                if (byteBuffer == null)
                    byteBuffer = new byte[chunkSize];

                while (arrayOffset < array.Length)
                {
                    int numArrayItems = Math.Min(chunkSize / typeLength, array.Length - arrayOffset);
                    int bufferUsed = numArrayItems * typeLength;
                    ReadBytes(byteBuffer, 0, bufferUsed);
#if BIGENDIAN 
					// we know that we are reading a primitive type, so just do a simple swap
					for (int i = 0; i < bufferUsed; i += typeLength) 
					{
 						for (int j = 0; j < typeLength / 2; j++)
						{
 							byte tmp = byteBuffer[i + j]; 
 							byteBuffer[i + j] = byteBuffer[i + typeLength - 1 - j];
							byteBuffer[i + typeLength - 1 - j] = tmp; 
 						} 
					}
#endif
                    //Modified
                    //Buffer.InternalBlockCopy(byteBuffer, 0, array, arrayOffset * typeLength, bufferUsed);
                    Type typeofBuffer = Type.GetType("System.Buffer");
                    LSInvoker.CallStaticMethod(typeofBuffer, "InternalBlockCopy", new Type[] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) },
                        new object[] { byteBuffer, 0, array, arrayOffset * typeLength, bufferUsed });
                    arrayOffset += numArrayItems;
                }
            }
        }

        private void ReadMemberPrimitiveTyped()
        {
            if (memberPrimitiveTyped == null)
                memberPrimitiveTyped = new MemberPrimitiveTyped();

            memberPrimitiveTyped.Read(this);
            memberPrimitiveTyped.Dump();

            prs.PRobjectTypeEnum = InternalObjectTypeE.Object; //Get rid of
            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRvarValue = memberPrimitiveTyped.value;
            prs.PRkeyDt = Converter.ToComType(memberPrimitiveTyped.primitiveTypeEnum);
            prs.PRdtType = Converter.ToType(memberPrimitiveTyped.primitiveTypeEnum);
            prs.PRdtTypeCode = memberPrimitiveTyped.primitiveTypeEnum;

            if (objectOp == null)
            {
                // Top level boxed primitive
                prs.PRparseTypeEnum = InternalParseTypeE.Object;
                prs.PRname = "System.Variant";
            }
            else
            {
                // Nested in an Object

                prs.PRparseTypeEnum = InternalParseTypeE.Member;
                prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

                switch (objectOp.objectTypeEnum)
                {
                    case InternalObjectTypeE.Object:
                        prs.PRname = objectOp.name;
                        prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                        break;
                    case InternalObjectTypeE.Array:
                        prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                        break;
                    default:
                        throw new SerializationException(String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_ObjectTypeEnum"), ((Enum)objectOp.objectTypeEnum).ToString()));
                }
            }
#if ObjectParse
            objectReader.Parse(prs);
#endif
#if GenerateTree
            prs.PRhasMember = false;
            prs.PRobjectPosition = position;
            prs.PRobjectValuePosition = position + 2;
            prs.PRobjectInternalType = (InternalPrimitiveTypeEx)memberPrimitiveTyped.primitiveTypeEnum;
            lsObjectNodeCollection.AddMemberNode(prs);
#endif
        }

        private void ReadMemberReference()
        {
            if (memberReference == null)
                memberReference = new MemberReference();
            memberReference.Read(this);
            memberReference.Dump();

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRidRef = objectReader.GetId((long)memberReference.idRef);
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.Reference;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
                prs.PRdtType = objectOp.dtType;
            }
            else
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;

#if ObjectParse
            objectReader.Parse(prs); 
#endif
#if GenerateTree
            prs.PRhasMember = false;
            prs.PRobjectInternalType = InternalPrimitiveTypeEx.MemberReference;
            prs.PRobjectPosition = position;
            prs.PRobjectValuePosition = position + 5;
            lsObjectNodeCollection.AddMemberNode(prs);
#endif
        }

        private void ReadObjectNull(BinaryHeaderEnum binaryHeaderEnum)
        {
            if (objectNull == null)
                objectNull = new ObjectNull();

            objectNull.Read(this, binaryHeaderEnum);
            objectNull.Dump();

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.Null;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
                prs.PRdtType = objectOp.dtType;
            }
            else
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                prs.PRnullCount = objectNull.nullCount;
                //only one null position has been incremented by GetNext
                //The position needs to be reset for the rest of the nulls 
                objectOp.ArrayCountIncrement(objectNull.nullCount - 1);
            }

#if ObjectParse
            objectReader.Parse(prs);
#endif
#if GenerateTree
            prs.PRhasMember = false;
            prs.PRobjectInternalType = InternalPrimitiveTypeEx.Null;
            prs.PRobjectPosition = position;
            prs.PRobjectValuePosition = position;
            lsObjectNodeCollection.AddMemberNode(prs);
#endif
        }

        private void ReadMessageEnd()
        {
            if (messageEnd == null)
                messageEnd = new MessageEnd();

            messageEnd.Read(this);

            messageEnd.Dump();

            if (!stack.IsEmpty())
            {
                stack.Dump();
                throw new SerializationException(LSEnvironment.GetResourceString("Serialization_StreamEnd"));
            }
        }

        private void ReadMemberPrimitiveUnTyped()
        {
            ObjectProgress objectOp = (ObjectProgress)stack.Peek();
            if (memberPrimitiveUnTyped == null)
                memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
            memberPrimitiveUnTyped.Set((InternalPrimitiveTypeE)expectedTypeInformation);
            memberPrimitiveUnTyped.Read(this);
            memberPrimitiveUnTyped.Dump();

            prs.Init();
            prs.PRvarValue = memberPrimitiveUnTyped.value;

            prs.PRdtTypeCode = (InternalPrimitiveTypeE)expectedTypeInformation;
            prs.PRdtType = Converter.ToType(prs.PRdtTypeCode);
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
            }
            else
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
#if ObjectParse
            objectReader.Parse(prs);
#endif
#if GenerateTree
            prs.PRhasMember = false;
            prs.PRobjectInternalType = (InternalPrimitiveTypeEx)expectedTypeInformation;
            prs.PRobjectPosition = position;
            prs.PRobjectValuePosition = position;
            lsObjectNodeCollection.AddMemberNode(prs);
#endif
        }

        internal Object ReadValue(InternalPrimitiveTypeE code)
        {
            Object var = null;

            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    var = ReadBoolean();
                    break;
                case InternalPrimitiveTypeE.Byte:
                    var = ReadByte();
                    break;
                case InternalPrimitiveTypeE.Char:
                    var = ReadChar();
                    break;
                case InternalPrimitiveTypeE.Double:
                    var = ReadDouble();
                    break;
                case InternalPrimitiveTypeE.Int16:
                    var = ReadInt16();
                    break;
                case InternalPrimitiveTypeE.Int32:
                    var = ReadInt32();
                    break;
                case InternalPrimitiveTypeE.Int64:
                    var = ReadInt64();
                    break;
                case InternalPrimitiveTypeE.SByte:
                    var = ReadSByte();
                    break;
                case InternalPrimitiveTypeE.Single:
                    var = ReadSingle();
                    break;
                case InternalPrimitiveTypeE.UInt16:
                    var = ReadUInt16();
                    break;
                case InternalPrimitiveTypeE.UInt32:
                    var = ReadUInt32();
                    break;
                case InternalPrimitiveTypeE.UInt64:
                    var = ReadUInt64();
                    break;
                case InternalPrimitiveTypeE.Decimal:
                    var = ReadDecimal();
                    break;
                case InternalPrimitiveTypeE.TimeSpan:
                    var = ReadTimeSpan();
                    break;
                case InternalPrimitiveTypeE.DateTime:
                    var = ReadDateTime();
                    break;
                default:
                    throw new SerializationException("Serialization_TypeCode");
            }
            return var;
        }

        private ObjectProgress GetOp()
        {
            ObjectProgress op = null;

            if (opPool != null && !opPool.IsEmpty())
            {
                op = (ObjectProgress)opPool.Pop();
                op.Init();
            }
            else
                op = new ObjectProgress();

            return op;
        }

        private void PutOp(ObjectProgress op)
        {
            if (opPool == null)
                opPool = new SerStack("opPool");
            opPool.Push(op);
        }

        private void Dump()
        { }
        public void Dispose()
        {
        }
    }
}
