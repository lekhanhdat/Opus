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
	using System.IO;
	/// <summary>
	/// Class that helps generate part-of-speech lookup list files.
	/// </summary>
	public class PosLookupListWriter
	{		
		private string mDictionaryFile;
		private Dictionary<string, Util.Set<string>> mDictionary;
		private Dictionary<string, int> mWordCounts;
		
		/// <summary>
		/// Creates a new part-of-speech lookup list, specifying the location to write it to.
		/// </summary>
		/// <param name="file">
		/// File to write the new list to.
		/// </param>
		public PosLookupListWriter(string file)
		{
			mDictionaryFile = file;
            mDictionary = new Dictionary<string, Util.Set<string>>();
            mWordCounts = new Dictionary<string, int>();
		}
		
		/// <summary>
		/// Adds an entry to the lookup list in memory, ready for writing to file.
		/// </summary>
		/// <param name="word">
		/// The word for which an entry should be added.
		/// </param>
		/// <param name="tag">
		/// The tag that should be marked as valid for this word.
		/// </param>
		public virtual void AddEntry(string word, string tag)
		{
            Util.Set<string> tags;
            if (mDictionary.ContainsKey(word))
            {
                tags = mDictionary[word];
            }
            else
            {
                tags = new Util.Set<string>();
                mDictionary.Add(word, tags);
            }
			tags.Add(tag);
			
			if (!(mWordCounts.ContainsKey(word)))
			{
				mWordCounts.Add(word, 1);
            }
			else
			{
				mWordCounts[word]++;
			}
		}
		
		/// <summary>
		/// Write the lookup list entries to file with a default cutoff of 5.
		/// </summary>
		public virtual void Write()
		{
			Write(5);
		}
		
		/// <summary>
		/// Write the lookup list entries to file.
		/// </summary>
		/// <param name="cutoff">
		/// The number of times a word must have been added to the lookup list for it to be considered important
		/// enough to write to file.
		/// </param>
		public virtual void Write(int cutoff)
		{
			using (StreamWriter writer = new StreamWriter(mDictionaryFile))
			{
                foreach (string word in mDictionary.Keys)
                {
                    if (mWordCounts[word] > cutoff)
                    {
                        writer.Write(word);
                        Util.Set<string> tags = mDictionary[word];
                        foreach (string tag in tags)
                        {
                            writer.Write(" ");
                            writer.Write(tag);
                        }
                        writer.Write(System.Environment.NewLine);
                    }
                }
				writer.Close();
			}
		}
	}
}
