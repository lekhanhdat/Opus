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
	using System.Collections.Generic;
	/// <summary>
	/// Provides a means of determining which tags are valid for a particular word based on a tag dictionary read from a file.
	/// </summary>
	public class PosLookupList
	{
		private Dictionary<string, string[]> mDictionary;
		private bool mIsCaseSensitive;
		
		//public PosLookupList(string file) : this(file, true)
		//{
		//}
		
		/// <summary>
		/// Create tag dictionary object with contents of specified file and using specified case to determine how to access entries in the tag dictionary.
		/// </summary>
		/// <param name="file">
		/// The file name for the tag dictionary.
		/// </param>
		/// <param name="caseSensitive">
		/// Specifies whether the tag dictionary is case sensitive or not.
		/// </param>
		//public PosLookupList(string file, bool caseSensitive) : this(new System.IO.StreamReader(file, System.Text.Encoding.UTF7), caseSensitive)
		//{
		//}
		
		/// <summary>
		/// Create tag dictionary object with contents of specified file and using specified case to determine how to access entries in the tag dictionary.
		/// </summary>
		/// <param name="reader">
		/// A reader for the tag dictionary.
		/// </param>
		/// <param name="caseSensitive">
		/// Specifies whether the tag dictionary is case sensitive or not.
		/// </param>
		public PosLookupList(System.IO.StreamReader reader, bool caseSensitive)
		{
            mDictionary = new Dictionary<string, string[]>();
			mIsCaseSensitive = caseSensitive;
			for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
			{
				string[] parts = line.Split(' ');
				string[] tags = new string[parts.Length - 1];
				for (int currentTag = 0, tagCount = parts.Length - 1; currentTag < tagCount; currentTag++)
				{
					tags[currentTag] = parts[currentTag + 1];
				}
				mDictionary[parts[0]] = tags;
			}
		}
		
		/// <summary>
		/// Returns a list of valid tags for the specified word. </summary>
		/// <param name="word">
		/// The word.
		/// </param>
		/// <returns>
		/// A list of valid tags for the specified word or null if no information is available for that word.
		/// </returns>
		public virtual string[] GetTags(string word)
		{
			if (!mIsCaseSensitive)
			{
                word = word.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (mDictionary.ContainsKey(word))
            {
			    return mDictionary[word];
            }
            else
            {
                return null;
            }
		}
	}
}
