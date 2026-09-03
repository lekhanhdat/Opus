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
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
using System;

namespace SharpEntropy
{
	public interface IMaximumEntropyModel
	{
		/// <summary>
		/// Returns the number of outcomes for this model.
		/// </summary>
		/// <returns>
		/// The number of outcomes.
		/// </returns>
		int OutcomeCount
		{
			get;		
		}
			
		/// <summary> 
		/// Evaluates a context.
		/// </summary>
		/// <param name="context">
		/// A list of string names of the contextual predicates
		/// which are to be evaluated together.
		/// </param>
		/// <returns>
		/// An array of the probabilities for each of the different
		/// outcomes, all of which sum to 1.
		/// </returns>
		double[] Evaluate(string[] context);
			
		/// <summary>
		/// Evaluates a context.
		/// </summary>
		/// <param name="context">
		/// A list of string names of the contextual predicates
		/// which are to be evaluated together.
		/// </param>
		/// <param name="probabilities">
		/// An array which is populated with the probabilities for each of the different
		/// outcomes, all of which sum to 1.
		/// </param>
		/// <returns>
		/// an array of the probabilities for each of the different
		/// outcomes, all of which sum to 1.  The <code>probabilities</code> array is returned if it is appropiately sized. 
		/// </returns>
		double[] Evaluate(string[] context, double[] probabilities);
			
		/// <summary>
		/// Simple function to return the outcome associated with the index
		/// containing the highest probability in the double[].
		/// </summary>
		/// <param name="outcomes">
		/// A <code>double[]</code> as returned by the
		/// <code>Evaluate(string[] context)</code>
		/// method.
		/// </param>
		/// <returns> 
		/// the string name of the best outcome
		/// </returns>
		string GetBestOutcome(double[] outcomes);
			
		/// <summary>
		/// Return a string matching all the outcome names with all the
		/// probabilities produced by the <code>eval(string[]
		/// context)</code> method.
		/// </summary>
		/// <param name="outcomes">
		/// A <code>double[]</code> as returned by the
		/// <code>eval(string[] context)</code>
		/// method.
		/// </param>
		/// <returns>
		/// string containing outcome names paired with the normalized
		/// probability (contained in the <code>double[] ocs</code>)
		/// for each one.
		/// </returns>
		string GetAllOutcomes(double[] outcomes);
			
		/// <summary>
		/// Gets the string name of the outcome associated with the supplied index
		/// </summary>
		/// <param name="index">
		/// the index for which the name of the associated outcome is desired.
		/// </param>
		/// <returns> 
		/// the string name of the outcome
		/// </returns>
		string GetOutcomeName(int index);
			
		/// <summary>
		/// Gets the index associated with the string name of the given
		/// outcome.
		/// </summary>
		/// <param name="outcome">
		/// the string name of the outcome for which the
		/// index is desired
		/// </param>
		/// <returns>
		/// the index if the given outcome label exists for this
		/// model, -1 if it does not.
		/// </returns>
		int GetOutcomeIndex(string outcome);
	}
}
