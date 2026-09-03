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
	/// <summary>
	/// Inteface for interacting with a Heap data structure.
	/// This implementation extract objects from smallest to largest based on either
	/// their natural ordering or the comparator provided to an implementation.
	/// While this is a typical of a heap it allows this objects natural ordering to
	/// match that of other sorted collections.
	/// </summary>
	public interface IHeap<T>
	{
			
		/// <summary>
		/// Removes the smallest element from the heap and returns it.
		/// </summary>
		/// <returns>
		/// The smallest element from the heap.
		/// </returns>
		T Extract();
			
		/// <summary>
		/// Returns the smallest element of the heap.
		/// </summary>
		/// <returns>
		/// The top element of the heap.
		/// </returns>
		T Top
		{
			get;
		}
			
		/// <summary>
		/// Adds the specified object to the heap.
		/// </summary>
		/// <param name="input">
		/// The object to add to the heap.
		/// </param>
		void Add(T input);
			
		/// <summary>
		/// Returns the size of the heap.
		/// </summary>
		/// <returns>
		/// The size of the heap.
		/// </returns>
		int Size
		{
			get;
		}
	
		/// <summary>
		/// Returns whether the heap is empty.
		/// </summary>
		/// <returns> 
		/// true if the heap is empty; false otherwise.
		///</returns>
		bool IsEmpty
		{
			get;
		}

		/// <summary>
		/// Clears the contents of the heap.
		/// </summary>
		void Clear();
	}
}