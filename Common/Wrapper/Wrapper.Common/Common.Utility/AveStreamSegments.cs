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
using System.IO;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveStreamSegments
    {
        private IList<long> mSegmentPositions = new List<long>();
        private IList<long> mSegmentTailPositions = new List<long>();
        private int mCurrentSegmentIndex = 0;
        private Stream mInnerStream;

        public AveStreamSegments(Stream stream)
        {
            mInnerStream = stream;
        }

        public void BeginSegment()
        {
            mSegmentPositions.Add(mInnerStream.Position);
        }

        public void BeginSegmentTail()
        {
            mSegmentTailPositions.Add(mInnerStream.Position);
        }

        public bool NextSegment()
        {
            if (mCurrentSegmentIndex == mSegmentPositions.Count)
            {
                return false;
            }
            else
            {
                mInnerStream.Position = mSegmentPositions[mCurrentSegmentIndex++];
                return true;
            }            
        }

        public void ToSegmentTail()
        {
            mInnerStream.Position = mSegmentTailPositions[mCurrentSegmentIndex - 1];
        }

        public Stream Stream
        {
            get
            {
                return mInnerStream;
            }
        }
    }
}
