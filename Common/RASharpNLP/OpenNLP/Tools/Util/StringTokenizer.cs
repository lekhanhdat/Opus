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

namespace OpenNLP.Tools.Util
{
    using System;
     
    public class StringTokenizer
    {
        private const string Delimiters = " \t\n\r";
            //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character	

        private readonly string[] _tokens;
        private int _position;

        /// <summary>
        /// Initializes a new class instance with a specified string to process
        /// </summary>
        /// <param name="input">
        /// string to tokenize
        /// </param>
        public StringTokenizer(string input) : this(input, Delimiters.ToCharArray())
        {
        }

        public StringTokenizer(string input, string separators) : this(input, separators.ToCharArray())
        {
        }

        public StringTokenizer(string input, params char[] separators)
        {
            _tokens = input.Split(separators);
            _position = 0;
        }

        public string NextToken()
        {
            while (_position < _tokens.Length)
            {
                if ((_tokens[_position].Length > 0))
                {
                    return _tokens[_position++];
                }
                _position++;
            }
            return null;
        }

    }
}