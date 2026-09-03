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
using System.Diagnostics;

namespace LS.BinarySerialization
{
    internal sealed class ObjectProgress
    {
        internal static int opRecordIdCount = 1;
        internal int opRecordId;


        // Control
        internal bool isInitial;
        internal int count; //Progress count
        internal BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        internal Object expectedTypeInformation = null;

        internal String name;
        internal InternalObjectTypeE objectTypeEnum = InternalObjectTypeE.Empty;
        internal InternalMemberTypeE memberTypeEnum;
        internal InternalMemberValueE memberValueEnum;
        internal Type dtType;

        // Array Information 
        internal int numItems;
        internal BinaryTypeEnum binaryTypeEnum;
        internal Object typeInformation;
        internal int nullCount;

        // Member Information
        internal int memberLength;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal Object[] typeInformationA;
        internal String[] memberNames;
        internal Type[] memberTypes;

        // ParseRecord
        internal ParseRecord pr = new ParseRecord();

        static ObjectProgress()
        {
            opRecordIdCount = 1;
        }

        internal ObjectProgress()
        {
            this.expectedType = BinaryTypeEnum.ObjectUrt;
            this.pr = new ParseRecord();

            Counter();
        }

        [Conditional("SER_LOGGING")]
        private void Counter()
        {
            lock (this)
            {
                opRecordId = opRecordIdCount++;
                if (opRecordIdCount > 1000)
                    opRecordIdCount = 1;
            }
        }

        internal void Init()
        {
            isInitial = false;
            count = 0;
            expectedType = BinaryTypeEnum.ObjectUrt;
            expectedTypeInformation = null;

            name = null;
            objectTypeEnum = InternalObjectTypeE.Empty;
            memberTypeEnum = InternalMemberTypeE.Empty;
            memberValueEnum = InternalMemberValueE.Empty;
            dtType = null;

            // Array Information 
            numItems = 0;
            nullCount = 0;
            //binaryTypeEnum 
            typeInformation = null;

            // Member Information
            memberLength = 0;
            binaryTypeEnumA = null;
            typeInformationA = null;
            memberNames = null;
            memberTypes = null;

            pr.Init();
        }

        //Array item entry of nulls has a count of nulls represented by that item. The first null has been 
        // incremented by GetNext, the rest of the null counts are incremented here
        internal void ArrayCountIncrement(int value)
        {
            count += value;
        }

        // Specifies what is to parsed next from the wire.
        internal bool GetNext(out BinaryTypeEnum outBinaryTypeEnum, out Object outTypeInformation)
        {
            //Initialize the out params up here.
            //< 
            outBinaryTypeEnum = BinaryTypeEnum.Primitive;
            outTypeInformation = null;


            if (objectTypeEnum == InternalObjectTypeE.Array)
            {
                //SerTrace.Log(this, "GetNext Array");
                // Array 
                if (count == numItems)
                    return false;
                else
                {
                    outBinaryTypeEnum = binaryTypeEnum;
                    outTypeInformation = typeInformation;
                    if (count == 0)
                        isInitial = false;
                    count++;
                    //SerTrace.Log(this, "GetNext Array Exit ", ((Enum)outBinaryTypeEnum).ToString(), " ", outTypeInformation);
                    return true;
                }
            }
            else
            {
                // Member 
                //SerTrace.Log(this, "GetNext Member");
                if ((count == memberLength) && (!isInitial))
                    return false;
                else
                {
                    outBinaryTypeEnum = binaryTypeEnumA[count];
                    outTypeInformation = typeInformationA[count];
                    if (count == 0)
                        isInitial = false;
                    name = memberNames[count];
                    if (memberTypes == null)
                    {
                        //SerTrace.Log(this, "GetNext memberTypes = null");
                    }
                    dtType = memberTypes[count];
                    count++;
                    //SerTrace.Log(this, "GetNext Member Exit ", ((Enum)outBinaryTypeEnum).ToString(), " ", outTypeInformation, " memberName ", name);
                    return true;
                }
            }
        }
    }
}
