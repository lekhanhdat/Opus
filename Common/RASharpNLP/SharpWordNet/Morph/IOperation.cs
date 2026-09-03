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
//Copyright (C) 2006 Richard J. Northedge
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

//This file is based on the Operation.java source file found in
//the Java WordNet Library (JWNL).  That source file is licensed under BSD.

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpWordNet.Morph
{
    public interface IOperation
    {
        /// <summary>
        /// Execute the operation.
        /// </summary>
        /// <param name="lemma">
        /// input lemma to look up
        /// </param>
        ///<param name="partOfSpeech">
        /// part of speech of the lemma to look up
        /// </param>
        /// <param name="baseForms">
        /// List to which all discovered base forms should be added.
        /// </param>
        /// <returns>
        /// True if at least one base form was discovered by the operation and
        /// added to baseForms.
        /// </returns>
        bool Execute(string lemma, string partOfSpeech, List<string> baseForms);
    }
}
