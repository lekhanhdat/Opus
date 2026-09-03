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
using System.Text;

namespace AvePoint.RA.Common.Retrying
{
    public interface IRMRetryPredicate<T>
    {
        bool Predicate(T attempt);
    }
    
    public class RMRetryPredicate<T, U> where T : IRMRetryPredicate<U>
    {
        public List<IRMRetryPredicate<U>> Predicates { get; private set; } = new List<IRMRetryPredicate<U>>();

        public void Add(T predicate)
        {
            Predicates.Add(predicate);
        }

        public bool Predicate(U parameter)
        {
            foreach(var predicate in Predicates)
            {
                if(!predicate.Predicate(parameter))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class RMExceptionPredicate : IRMRetryPredicate<RMRetryAttemptInfo>
    {

        public Type PredicateExceptionType { get; private set; }

        public bool IsStrictMatch { get; private set; }

        public RMExceptionPredicate(Type exception) : this(exception, false) { }

        public RMExceptionPredicate(Type exception, bool isStrictMatch)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (!exception.IsSubclassOf(typeof(Exception)) && exception != typeof(Exception))
            {
                throw new ArgumentException("Parameter type is not exception.");
            }

            PredicateExceptionType = exception;
            IsStrictMatch = isStrictMatch;
        }

        public bool Predicate(RMRetryAttemptInfo attemptInfo)
        {
            return true;
            //var exception = attemptInfo.E.GetType();
            //return (!IsStrictMatch && exception.IsSubclassOf(PredicateExceptionType)) || exception == PredicateExceptionType;
        }
    }
}
