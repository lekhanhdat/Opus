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

namespace SharpEntropy
{
	public class PatternedPredicate
	{
		private int mOutcomePattern;
		private double[] mParameters;
		private string mName;

		/// <summary>
		/// Creates a PatternedPredicate object.
		/// </summary>
		/// <param name="outcomePattern">
		/// Index into the outcome pattern array, specifying which outcome pattern relates to
		/// this predicate.
		/// </param>
		/// <param name="parameters">
		/// Array of parameters for this predicate.
		/// </param>
		protected internal PatternedPredicate(int outcomePattern, double[] parameters)
		{
			mOutcomePattern = outcomePattern;
			mParameters = parameters;
		}

		/// <summary>
		/// Creates a PatternedPredicate object.
		/// </summary>
		/// <param name="name">
		/// The predicate name.
		/// </param>
		/// <param name="parameters">
		/// Array of parameters for this predicate.
		/// </param>
		protected internal PatternedPredicate(string name, double[] parameters)
		{
			mName = name;
			mParameters = parameters;
		}

		/// <summary>
		/// Index into array of outcome patterns.
		/// </summary>
		public int OutcomePattern
		{
			get
			{
				return mOutcomePattern;
			}
			set // for trainer
			{
				mOutcomePattern = value;
			}
		}

		/// <summary>
		/// Gets the value of a parameter from this predicate.
		/// </summary>
		/// <param name="index">
		/// index into the parameter array.
		/// </param>
		/// <returns></returns>
		public double GetParameter(int index)
		{
			return mParameters[index];
		}

		/// <summary>
		/// Number of parameters associated with this predicate.
		/// </summary>
		public int ParameterCount
		{
			get
			{
				return mParameters.Length;
			}
		}

		/// <summary>
		/// Name of the predicate.
		/// </summary>
		public string Name
		{
			get
			{
				return mName;
			}
			set
			{
				mName = value;
			}
		}
	}
}
