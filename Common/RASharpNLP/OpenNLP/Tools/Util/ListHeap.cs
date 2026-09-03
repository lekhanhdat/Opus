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
	using System.Collections.Generic;
	/// <summary>
	/// This class implements the heap interface using a generic List as the underlying
	/// data structure.  This heap allows values which are equals to be inserted, however
	/// the order in which they are extracted is arbitrary.
	/// </summary>
	public class ListHeap<T> : IHeap<T>, IEnumerable<T>
	{
		private List<T> mList;
		private readonly IComparer<T> mComparer;
		private readonly int mSize;
		private T mMax;
		
		/// <summary>
		/// True if the heap is empty.
		/// </summary>
		public virtual bool IsEmpty
		{
			get
			{
				return (mList.Count == 0);
			}
		}
	
		/// <summary>
		/// Creates a new heap with the specified size using the sorted based on the
		/// specified comparator.
		/// </summary>
		/// <param name="size">
		/// The size of the heap.
		/// </param>
		/// <param name="comparer">
		/// The comparer to be used to sort heap elements.
		/// </param>
		public ListHeap(int size, IComparer<T> comparer)
		{
			mSize = size;
			mComparer = comparer;
			mList = new List<T>(size);
		}
		
		/// <summary>
		/// Createa a new heap of the specified size.
		/// </summary>
		/// <param name="size">
		/// The size of the new heap.
		/// </param>
		public ListHeap(int size) : this(size, null)
		{
		}
		
		private int ParentIndex(int index)
		{
			return (index - 1) / 2;
		}
		
		private int LeftIndex(int index)
		{
			return (index + 1) * 2 - 1;
		}
		
		private int RightIndex(int index)
		{
			return (index + 1) * 2;
		}
		
		/// <summary>
		/// The size of the heap.
		/// </summary>
		public virtual int Size
		{
			get
			{
				return mList.Count;
			}
			set
			{
				if (value > mList.Count)
				{
					return ;
				}
				else
				{
                    var newList = new List<T>(value);
					for (int currentItem = 0; currentItem < value; currentItem++)
					{
						newList.Add(this.Extract());
					}
					mList = newList;
				}
			}
		}
		
		private void Swap(int firstIndex, int secondIndex)
		{
			T firstObject = mList[firstIndex];
			T secondObject = mList[secondIndex];
			
			mList[secondIndex] = firstObject;
			mList[firstIndex] = secondObject;
		}
		
		private bool LessThan(T firstObject, T secondObject)
		{
			if (mComparer != null)
			{
				return (mComparer.Compare(firstObject, secondObject) < 0);
			}
			else
			{
				return (((IComparable) firstObject).CompareTo(secondObject) < 0);
			}
		}
		
		private bool GreaterThan(T firstObject, T secondObject)
		{
			if (mComparer != null)
			{
				return (mComparer.Compare(firstObject, secondObject) > 0);
			}
			else
			{
				return (((IComparable) firstObject).CompareTo(secondObject) > 0);
			}
		}
		
		private void Heapify(int index)
		{
			while (true)
			{
				int left = LeftIndex(index);
				int right = RightIndex(index);
				int smallest;
				
				if (left < mList.Count && LessThan(mList[left], mList[index]))
				{
					smallest = left;
				}
				else
				{
					smallest = index;
				}
				
				if (right < mList.Count && LessThan(mList[right], mList[smallest]))
				{
					smallest = right;
				}
				
				if (smallest != index)
				{
					Swap(smallest, index);
					index = smallest;
				}
				else
				{
					break;
				}
			}
		}
		
		public virtual T Extract()
		{
			if (mList.Count == 0)
			{
				throw new NotSupportedException("Heap Underflow");
			}
			T mMax = mList[0];
			int last = mList.Count - 1;
			if (last != 0)
			{
				mList[0] = mList[last];
				mList.RemoveAt(last);
				Heapify(0);
			}
			else
			{
				mList.RemoveAt(last);
			}
			
			return mMax;
		}
		
		/// <summary>
		/// Resets the heap size to its original value.
		/// </summary>
		public virtual void ResetSize()
		{
			this.Size = mSize;
		}
				
		/// <summary>
		/// Gets the object on top of the heap.
		/// </summary>
		public virtual T Top
		{
			get
			{
				if (mList.Count == 0)
				{
					throw new NotSupportedException("Heap Underflow");
				}
				return (mList[0]);
			}
		}
		
		public virtual void Add(T item)
		{
			/* keep track of min to prevent unnecessary insertion */
			if (object.Equals(mMax, default(T)))
			{
				mMax = item;
			}
			else if (GreaterThan(item, mMax))
			{
				if (mList.Count < mSize)
				{
					mMax = item;
				}
				else
				{
					return;
				}
			}
			mList.Add(item);
			
			int index = mList.Count - 1;
			
			//percolate new node to correct position in heap.
			while (index > 0 && GreaterThan(mList[ParentIndex(index)], item))
			{
				mList[index] = mList[ParentIndex(index)];
				index = ParentIndex(index);
			}
			
			mList[index] = item;
		}
		
		public virtual void Clear()
		{
			mList.Clear();
		}
		
		public virtual System.Collections.IEnumerator GetEnumerator()
		{
			return (mList.GetEnumerator());
		}

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (mList.GetEnumerator());
        }

    }
}