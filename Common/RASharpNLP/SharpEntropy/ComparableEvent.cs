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
//Copyright (C) 2005 Richard J. Northedge
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
using System;
using System.Text;

namespace SharpEntropy
{ 
	public class ComparableEvent : IComparable<ComparableEvent>
	{
		private int mOutcome;
		private int[] mPredicateIndexes ;
		private int mSeenCount = 1;

		/// <summary>
		/// The outcome ID of this event.
		/// </summary>
		public int Outcome
		{
			get
			{
				return mOutcome;
			}
			set
			{
				mOutcome = value;
			}
		}

		/// <summary>
		/// Returns an array containing the indexes of the predicates in this event.
		/// </summary>
		/// <returns>
		/// Integer array of predicate indexes.
		/// </returns>
		public int[] GetPredicateIndexes()
		{
			return mPredicateIndexes;
		}

		/// <summary>
		/// Sets the array containing the indices of the predicates in this event.
		/// </summary>
		/// <param name="predicateIndexes">
		/// Integer array of predicate indexes.
		/// </param>
		public void SetPredicateIndexes(int[] predicateIndexes)
		{
			mPredicateIndexes  = predicateIndexes;
		}

		/// <summary>
		/// The number of times this event
		/// has been seen.
		/// </summary>
		public int SeenCount
		{
			get
			{
				return mSeenCount;
			}
			set
			{
				mSeenCount = value;
			}
		}

		/// <summary>
		/// Constructor for the ComparableEvent.
		/// </summary>
		/// <param name="outcome">
		/// The ID of the outcome for this event.
		/// </param>
		/// <param name="predicateIndexes">
		/// Array of indexes for the predicates in this event.
		/// </param>
		public ComparableEvent(int outcome, int[] predicateIndexes)
		{
			mOutcome = outcome;
			System.Array.Sort(predicateIndexes);
			mPredicateIndexes  = predicateIndexes;
		}
		
		/// <summary>
		/// Implementation of the IComparable interface.
		/// </summary>
        /// <param name="eventToCompare">
        /// ComparableEvent to compare this event to.
		/// </param>
		/// <returns>
		/// A value indicating if the compared object is smaller, greater or the same as this event.
		/// </returns>
        public virtual int CompareTo(ComparableEvent eventToCompare)
		{			
			if (mOutcome < eventToCompare.Outcome)
			{
				return - 1;
			}
			else if (mOutcome > eventToCompare.Outcome)
			{
				return 1;
			}
			
			int smallerLength = (mPredicateIndexes .Length > eventToCompare.GetPredicateIndexes().Length ? eventToCompare.GetPredicateIndexes().Length : GetPredicateIndexes().Length);
			
			for (int currentIndex = 0; currentIndex < smallerLength; currentIndex++)
			{
				if (mPredicateIndexes [currentIndex] < eventToCompare.GetPredicateIndexes()[currentIndex])
				{
					return - 1;
				}
				else if (mPredicateIndexes [currentIndex] > eventToCompare.GetPredicateIndexes()[currentIndex])
				{
					return 1;
				}
			}
			
			if (mPredicateIndexes .Length < eventToCompare.GetPredicateIndexes().Length)
			{
				return - 1;
			}
			else if (mPredicateIndexes .Length > eventToCompare.GetPredicateIndexes().Length)
			{
				return 1;
			}
			
			return 0;
		}
		
		/// <summary>
		/// Tests if this event is equal to another object.
		/// </summary>
		/// <param name="o">
		/// Object to test against.
		/// </param>
		/// <returns>
		/// True if the objects are equal.
		/// </returns>
		public override bool Equals (object o)
		{
			if (!(o is ComparableEvent))
			{
				return false;
			}
			return (this.CompareTo(o as ComparableEvent)== 0);
		}  

		/// <summary>
		/// Provides a hashcode for storing events in a dictionary or hashtable.
		/// </summary>
		/// <returns>
		/// A hashcode value.
		/// </returns>
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}  

		/// <summary>
		/// Override to provide a succint summary of the ComparableEvent object.
		/// </summary>
		/// <returns>
		/// string representation of the ComparableEvent object.
		/// </returns>
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int currentIndex = 0; currentIndex < mPredicateIndexes.Length; currentIndex++)
			{
				stringBuilder.Append(" ").Append(mPredicateIndexes [currentIndex]);
			}
			return stringBuilder.ToString();
		}
	}
}
