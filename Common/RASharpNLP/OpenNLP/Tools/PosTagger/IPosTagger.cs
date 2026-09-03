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

namespace OpenNLP.Tools.PosTagger
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	/// <summary> 
	/// The interface for part of speech taggers.
	/// </summary>
	public interface IPosTagger : IDisposable
	{
			
		/// <summary>Assigns the sentence of tokens pos tags</summary>
		/// <param name="tokens">The sentence of tokens to be tagged</param>
		/// <returns>An array of pos tags for each token provided in sentence</returns>
		string[] Tag(string[] tokens);
			
		/// <summary> Assigns pos tags to the sentence of space-delimited tokens</summary>
		/// <param name="sentence">The sentence of space-delimited tokens to be tagged</param>
		/// <returns>A collection of tagged words (word + pos tag + index in sentence)</returns>
		List<TaggedWord> TagSentence(string sentence);
	}
}
