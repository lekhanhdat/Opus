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
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;

namespace LS.BinarySerialization
{
    internal class LSObjectNodeValueExtension
    {
        private InternalPrimitiveTypeEx type;
        private long valuePosition;
        private long valuePhysicalPosition;

        internal InternalPrimitiveTypeEx ValueType
        {
            get { return type; }
            set { type = value; }
        }

        internal long Position
        {
            get { return valuePosition; }
            set { valuePosition = value; }
        }

        internal long PhysicalPosition
        {
            get { return valuePhysicalPosition; }
            set { valuePhysicalPosition = value; }
        }
    }
    internal class LSObjectNode
    {
        private long objectId;
        private string objectName;
        private InternalObjectTypeE objectType;
        private InternalMemberTypeE fieldType;
        private object[] Values;
        private int valueIndex = 0;
        //private int memberIndex = 0;


        private Dictionary<string, object> members;
        private Dictionary<string, string> memberRefs;
        private Dictionary<string, LSObjectNodeValueExtension> types;

        //private int arrayLength = 0;
        //private bool isPrimitiveArray = false;
        //private int position = 0;

        private Guid guidInternalValue = Guid.Empty;
        private object arrayType;

        internal long ObjectId
        {
            get { return objectId; }
            set { objectId = value; }
        }

        internal string Name
        {
            get { return objectName; }
            set { objectName = value; }
        }

        internal InternalObjectTypeE Type
        {
            get { return objectType; }
            set { objectType = value; }
        }

        internal InternalMemberTypeE FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }

        internal object[] ArrayValues
        {
            get { return Values; }
            set { Values = value; }
        }

        internal Dictionary<string, object> Members
        {
            get { return members; }
            set { members = value; }
        }

        internal Dictionary<string, string> MemberRefs
        {
            get { return memberRefs; }
            set { memberRefs = value; }
        }

        internal Guid GuidInternalValue
        {
            get { return guidInternalValue; }
            set { guidInternalValue = value; }
        }

        internal object ArrayType
        {
            get { return arrayType; }
            set { arrayType = value; }
        }

        internal Dictionary<string, LSObjectNodeValueExtension> ValueTypes
        {
            get { return types; }
            set { types = value; }
        }

        internal int ValueIndex
        {
            get { return valueIndex; }
            set { valueIndex = value; }
        }
    }

    internal class LSObjectNodeCollection
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SerStack stack = new SerStack("ProcessTree");

        private List<LSObjectNode> mObjectNodes;
        internal List<LSObjectNode> ObjectNodes
        {
            get
            {
                if (mObjectNodes == null)
                    mObjectNodes = new List<LSObjectNode>();
                return mObjectNodes;
            }
            set { mObjectNodes = value; }
        }

        private Dictionary<long, LSObjectNode> mFieldNodes;
        internal Dictionary<long, LSObjectNode> FieldNodes
        {
            get
            {
                if (mFieldNodes == null)
                    mFieldNodes = new Dictionary<long, LSObjectNode>();
                return mFieldNodes;
            }
            set { mFieldNodes = value; }
        }

        public void Clear()
        {
            ObjectNodes.Clear();
            FieldNodes.Clear();
        }

        private LSObjectNode CreateNode(ParseRecord pr)
        {
            LSObjectNode node = null;
            switch (pr.PRparseTypeEnum)
            {
                case InternalParseTypeE.Member:
                case InternalParseTypeE.Object:
                    switch (pr.PRobjectTypeEnum)
                    {
                        case InternalObjectTypeE.Array:

                            node = new LSObjectNode();
                            node.Type = pr.PRobjectTypeEnum;
                            node.ObjectId = pr.PRobjectId;
                            node.Name = pr.PRkeyDt;
                            node.ArrayType = pr.PRobjectArrayType;

                            node.MemberRefs = new Dictionary<string, string>();

                            //node.Values = new object[pr.PRlengthA[0]];
                            if (pr.PRisPrimitiveArray && pr.PRnewObj != null)
                            {
                                node.ArrayValues = new object[1];
                                node.ArrayValues[node.ValueIndex] = pr.PRnewObj;

                                LSObjectNodeValueExtension info = new LSObjectNodeValueExtension();
                                info.Position = pr.PRobjectPosition;
                                info.PhysicalPosition = pr.PRobjectValuePosition;
                                info.ValueType = InternalPrimitiveTypeEx.ByteArray;
                                node.ValueTypes = new Dictionary<string, LSObjectNodeValueExtension>(1);
                                node.ValueTypes.Add("0", info);
                            }
                            else
                            {
                                node.ArrayValues = new object[pr.PRlengthA[0]];
                                node.ValueTypes = new Dictionary<string, LSObjectNodeValueExtension>(pr.PRlengthA[0]);
                            }
                            break;
                        case InternalObjectTypeE.Object:
                            node = new LSObjectNode();
                            node.Type = pr.PRobjectTypeEnum;
                            node.ObjectId = pr.PRobjectId;
                            node.Name = pr.PRkeyDt;
                            node.Members = new Dictionary<string, object>();
                            foreach (string s in pr.PRobjectInfo.wireMemberNames)
                            {
                                node.Members.Add(s, null);
                            }
                            node.MemberRefs = new Dictionary<string, string>();

                            node.ValueTypes = new Dictionary<string, LSObjectNodeValueExtension>(node.Members.Count);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            ObjectNodes.Add(node);
            return node;
        }

        private void CreateAddtionalNode(ParseRecord pr)
        {
            try
            {
                if (pr.PRobjectId == 0)
                    return;
                if (pr.PRmemberValueEnum != InternalMemberValueE.InlineValue)
                    return;
                LSObjectNode node = null;
                switch (pr.PRparseTypeEnum)
                {
                    case InternalParseTypeE.Member:
                    case InternalParseTypeE.Object:
                        switch (pr.PRobjectTypeEnum)
                        {
                            case InternalObjectTypeE.Object:
                                node = new LSObjectNode();
                                node.Type = pr.PRobjectTypeEnum;
                                node.FieldType = pr.PRmemberTypeEnum;
                                node.ObjectId = pr.PRobjectId;
                                node.Name = pr.PRobjectInternalType.ToString();

                                if (pr.PRmemberTypeEnum == InternalMemberTypeE.Field)
                                {
                                    node.Members = new Dictionary<string, object>();
                                    if (string.IsNullOrEmpty(pr.PRvalue))
                                        node.Members.Add(pr.PRname, pr.PRvarValue);
                                    else
                                        node.Members.Add(pr.PRname, pr.PRvalue);
                                }
                                else if (pr.PRmemberTypeEnum == InternalMemberTypeE.Item)
                                {
                                    node.ArrayValues = new object[1];
                                    if (string.IsNullOrEmpty(pr.PRvalue))
                                        node.ArrayValues[0] = pr.PRvarValue;
                                    else
                                        node.ArrayValues[0] = pr.PRvarValue;
                                }
                                else
                                    return;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                if (node != null)
                {
                    if (FieldNodes.ContainsKey(node.ObjectId))
                        FieldNodes[node.ObjectId] = node;
                    else
                        FieldNodes.Add(node.ObjectId, node);
                }
            }
            catch(Exception ex) 
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.AddtionalNodeCreateError, ex);
            }
        }

        internal void AddMemberNode(ParseRecord pr)
        {
            LSObjectNodeValueExtension info = null;
            //try
            //{
            LSObjectNode node = null;
            switch (pr.PRparseTypeEnum)
            {
                case InternalParseTypeE.Object:
                    stack.Push(CreateNode(pr));
                    break;
                case InternalParseTypeE.ObjectEnd:
                case InternalParseTypeE.MemberEnd:
                    stack.Pop();
                    break;
                case InternalParseTypeE.Member:
                    node = (LSObjectNode)stack.Peek();
                    switch (pr.PRmemberTypeEnum)
                    {
                        case InternalMemberTypeE.Field:
                            switch (pr.PRmemberValueEnum)
                            {
                                case InternalMemberValueE.InlineValue:
                                    if (!string.IsNullOrEmpty(pr.PRname))
                                    {
                                        if (pr.PRvalue != null)
                                        {
                                            node.Members[pr.PRname] = pr.PRvalue;
                                        }
                                        else
                                            node.Members[pr.PRname] = pr.PRvarValue;




                                        info = new LSObjectNodeValueExtension();
                                        info.ValueType = pr.PRobjectInternalType;
                                        info.Position = pr.PRobjectPosition;
                                        info.PhysicalPosition = pr.PRobjectValuePosition;
                                        node.ValueTypes.Add(pr.PRname, info);


                                        CreateAddtionalNode(pr);
                                    }
                                    break;
                                case InternalMemberValueE.Reference:
                                    node.Members[pr.PRname] = InternalMemberValueE.Reference;
                                    node.MemberRefs.Add(pr.PRname, pr.PRidRef.ToString());




                                    info = new LSObjectNodeValueExtension();
                                    info.ValueType = InternalPrimitiveTypeEx.MemberReference;
                                    info.Position = pr.PRobjectPosition;
                                    info.PhysicalPosition = pr.PRobjectValuePosition;
                                    node.ValueTypes.Add(pr.PRname, info);
                                    break;
                                case InternalMemberValueE.Nested:
                                    node.Members[pr.PRname] = InternalMemberValueE.Nested;
                                    node.MemberRefs.Add(pr.PRname, pr.PRobjectId.ToString());
                                    stack.Push(CreateNode(pr));




                                    info = new LSObjectNodeValueExtension();
                                    info.ValueType = InternalPrimitiveTypeEx.MemberNested;
                                    info.Position = pr.PRobjectPosition;
                                    info.PhysicalPosition = pr.PRobjectValuePosition;
                                    node.ValueTypes.Add(pr.PRname, info);
                                    break;
                                case InternalMemberValueE.Null:
                                    node.ValueIndex = node.ValueIndex + (pr.PRnullCount - 1);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case InternalMemberTypeE.Item:
                            node = (LSObjectNode)stack.Peek();
                            switch (pr.PRmemberValueEnum)
                            {
                                case InternalMemberValueE.InlineValue:
                                    if (pr.PRvalue != null)
                                        node.ArrayValues[node.ValueIndex] = pr.PRvalue;
                                    else
                                        node.ArrayValues[node.ValueIndex] = pr.PRvarValue;




                                    info = new LSObjectNodeValueExtension();
                                    info.ValueType = pr.PRobjectInternalType;
                                    info.Position = pr.PRobjectPosition;
                                    info.PhysicalPosition = pr.PRobjectValuePosition;
                                    node.ValueTypes.Add(node.ValueIndex.ToString(), info);


                                    CreateAddtionalNode(pr);
                                    break;
                                case InternalMemberValueE.Reference:
                                    node.ArrayValues[node.ValueIndex] = InternalMemberValueE.Reference;
                                    node.MemberRefs.Add(node.ValueIndex.ToString(), pr.PRidRef.ToString());




                                    info = new LSObjectNodeValueExtension();
                                    info.ValueType = InternalPrimitiveTypeEx.MemberReference;
                                    info.Position = pr.PRobjectPosition;
                                    info.PhysicalPosition = pr.PRobjectValuePosition;
                                    node.ValueTypes.Add(node.ValueIndex.ToString(), info);
                                    break;
                                case InternalMemberValueE.Nested:
                                    node.ArrayValues[node.ValueIndex] = InternalMemberValueE.Nested;
                                    node.MemberRefs.Add(node.ValueIndex.ToString(), pr.PRobjectId.ToString());
                                    stack.Push(CreateNode(pr));




                                    info = new LSObjectNodeValueExtension();
                                    info.ValueType = InternalPrimitiveTypeEx.MemberNested;
                                    info.Position = pr.PRobjectPosition;
                                    info.PhysicalPosition = pr.PRobjectValuePosition;
                                    node.ValueTypes.Add(node.ValueIndex.ToString(), info);
                                    break;
                                case InternalMemberValueE.Null:
                                    node.ValueIndex = node.ValueIndex + (pr.PRnullCount - 1);
                                    break;
                                default:
                                    break;
                            }
                            node.ValueIndex++;
                            break;
                        default:
                            break;

                    }
                    break;
                default:
                    break;
            }
            //}
            //catch (Exception e)
            //{ }
        }
    }
}
