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

namespace LS.BinarySerialization
{
    public sealed class ParseRecord
    {
        // Fields
        internal static int parseRecordIdCount;
        internal Type PRarrayElementType;
        internal InternalPrimitiveTypeE PRarrayElementTypeCode;
        internal string PRarrayElementTypeString;
        internal InternalArrayTypeE PRarrayTypeEnum;
        internal Type PRdtType;
        internal InternalPrimitiveTypeE PRdtTypeCode;
        internal long PRheaderId;
        internal long PRidRef;
        internal int[] PRindexMap;
        internal bool PRisArrayVariant;
        internal bool PRisEnum;
        internal bool PRisLowerBound;
        internal bool PRisPrimitiveArray;
        internal bool PRisRegistered;
        internal bool PRisValueTypeFixup;
        internal bool PRisVariant;
        internal string PRkeyDt;
        internal int[] PRlengthA;
        internal int PRlinearlength;
        internal int[] PRlowerBoundA;
        internal object[] PRmemberData;
        internal int PRmemberIndex;
        internal InternalMemberTypeE PRmemberTypeEnum;
        internal InternalMemberValueE PRmemberValueEnum;
        internal string PRname;
        internal object PRnewObj;
        internal int PRnullCount;
        internal object[] PRobjectA;
        internal long PRobjectId;
        internal ReadObjectInfo PRobjectInfo;
        internal InternalObjectPositionE PRobjectPositionEnum;
        internal InternalObjectTypeE PRobjectTypeEnum;
        internal int PRparseRecordId;
        internal InternalParseTypeE PRparseTypeEnum;
        internal int[] PRpositionA;
        internal PrimitiveArray PRprimitiveArray;
        internal int PRrank;
        internal int[] PRrectangularMap;
        internal SerializationInfo PRsi;
        internal long PRtopId;
        internal int[] PRupperBoundA;
        internal string PRvalue;
        internal object PRvarValue;

        //for analyze
        internal bool PRhasMember;
        internal long PRobjectPosition;
        internal long PRobjectValuePosition;
        internal Type[] PRmemberTypes;
        internal InternalPrimitiveTypeEx PRobjectInternalType;
        internal object PRobjectArrayType;

        static ParseRecord()
        {
            parseRecordIdCount = 1;
        }

        internal void Init()
        {
            this.PRparseTypeEnum = InternalParseTypeE.Empty;
            this.PRobjectTypeEnum = InternalObjectTypeE.Empty;
            this.PRarrayTypeEnum = InternalArrayTypeE.Empty;
            this.PRmemberTypeEnum = InternalMemberTypeE.Empty;
            this.PRmemberValueEnum = InternalMemberValueE.Empty;
            this.PRobjectPositionEnum = InternalObjectPositionE.Empty;
            this.PRname = null;
            this.PRvalue = null;
            this.PRkeyDt = null;
            this.PRdtType = null;
            this.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRisEnum = false;
            this.PRobjectId = 0L;
            this.PRidRef = 0L;
            this.PRarrayElementTypeString = null;
            this.PRarrayElementType = null;
            this.PRisArrayVariant = false;
            this.PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRrank = 0;
            this.PRlengthA = null;
            this.PRpositionA = null;
            this.PRlowerBoundA = null;
            this.PRupperBoundA = null;
            this.PRindexMap = null;
            this.PRmemberIndex = 0;
            this.PRlinearlength = 0;
            this.PRrectangularMap = null;
            this.PRisLowerBound = false;
            this.PRtopId = 0L;
            this.PRheaderId = 0L;
            this.PRisValueTypeFixup = false;
            this.PRnewObj = null;
            this.PRobjectA = null;
            this.PRprimitiveArray = null;
            this.PRobjectInfo = null;
            this.PRisRegistered = false;
            this.PRmemberData = null;
            this.PRsi = null;
            this.PRnullCount = 0;
        }
    }
}
