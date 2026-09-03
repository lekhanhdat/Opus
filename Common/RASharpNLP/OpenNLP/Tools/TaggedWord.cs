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

namespace OpenNLP.Tools
{ 
    using System.Linq;  

    public class TaggedWord
    {
        public string Tag { get; set; }
        public string Word { get; set; }
        public int Index { get; set; }

         
        public TaggedWord(string stringTaggedWord, int indexInGroup)
        {
            if (stringTaggedWord.Contains("/"))
            {
                this.Word = stringTaggedWord.Split('/').First();
                this.Tag = stringTaggedWord.Split('/').Last();
                this.Index = indexInGroup;
            }
        }

        public TaggedWord(string word, string tag, int index)
        {
            this.Word = word;
            this.Tag = tag;
            this.Index = index;
        }
         
        public override string ToString()
        {
            return string.Format("{0}/{1}", this.Word, this.Tag);
        }
         
    }
}
