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
using System.Collections.Generic;

namespace SharpEntropy.IO
{
	/// <summary> 
	/// Interface for readers of GIS models.
	/// </summary>
	public interface IGisModelReader
	{
		/// <summary>
		/// Returns the value of the model's correction constant.  This property should
		/// usually only be accessed by GIS model writer classes via the GisModel class.
		/// </summary>
		int CorrectionConstant
		{
			get;
		}

		/// <summary>
		/// Returns the value of the model's correction constant parameter.  This property should
		/// usually only be accessed by GIS model writer classes via the GisModel class.
		/// </summary>
		double CorrectionParameter
		{
			get;
		}

		/// <summary>
		/// Returns the model's outcome labels as a string array.  This method should
		/// usually only be accessed by GIS model writer classes via the GisModel class.
		/// </summary>
		string[] GetOutcomeLabels();

		/// <summary>
		/// Returns the model's outcome patterns.  This method should
		/// usually only be accessed by GIS model writer classes via the GisModel class.
		/// </summary>
		int[][] GetOutcomePatterns();

		/// <summary>
		/// Returns the model's predicates.  This method should
		/// usually only be accessed by GIS model writer classes via the GisModel class.
		/// </summary>
		Dictionary<string, PatternedPredicate> GetPredicates();

		/// <summary>
		/// Returns model information for a predicate, given the predicate label.
		/// </summary>
		/// <param name="predicateLabel">
		/// The predicate label to fetch information for.
		/// </param>
		/// <param name="featureCounts">
		/// Array to be passed in to the method; it should have a length equal to the number of outcomes
		/// in the model.  The method increments the count of each outcome that is active in the specified
		/// predicate.
		/// </param>
		/// <param name="outcomeSums">
		/// Array to be passed in to the method; it should have a length equal to the number of outcomes
		/// in the model.  The method adds the parameter values for each of the active outcomes in the
		/// predicate.
		/// </param>
		void GetPredicateData(string predicateLabel, int[] featureCounts, double[] outcomeSums);

	}
}
