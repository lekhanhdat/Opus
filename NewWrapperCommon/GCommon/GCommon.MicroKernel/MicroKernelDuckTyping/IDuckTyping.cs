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



namespace AvePoint.GCommon.MicroKernel.DuckTyping
{
    #region using directives

    using System;

    #endregion using directives

    /// <summary>
    /// Class for casting objects using "duck typing".  Casting will succeed if a given duck type implements
    /// all the members of an interface even though it does not explicitly implement said interface at
    /// compile time.  Hence, implementation of interfaces is moved to runtime.  Also supports delegate
    /// casting.
    /// </summary>
    public interface IDuckTyping
    {
        /// <summary>
        /// Casts an object using duck typing.
        /// </summary>
        /// <remarks>
        /// This method will use a normal cast if one is possible.
        /// </remarks>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="duck">Object to cast.</param>
        /// <returns>A T casting of the given duck object.</returns>
        T Cast<T>(Object duck);

        /// <summary>
        /// Casts an object using duck typing.
        /// </summary>
        /// <remarks>
        /// This method will use a normal cast if one is possible.
        /// </remarks>
        /// <param name="toType">Type to cast to.</param>
        /// <param name="duck">Object to cast.</param>
        /// <returns>A casting of the given duck object to the given type.</returns>
        Object Cast(Type toType, Object duck);

        /// <summary>
        /// Casts a static type to an object using duck typing.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="staticType">Static type to cast.</param>
        /// <returns>A casting of the given static type.</returns>
        T StaticCast<T>(Type staticType);

        /// <summary>
        /// Casts a static type to an object using duck typing.
        /// </summary>
        /// <param name="toType">Type to cast to.</param>
        /// <param name="staticType">Static type to cast.</param>
        /// <returns>A casting of the given static type.</returns>
        Object StaticCast(Type toType, Type staticType);

        /// <summary>
        /// If the given object is a duck casted object, uncasts the object to retrieve the original duck object.
        /// </summary>
        /// <param name="duck">Object that may be duck casted.</param>
        /// <returns>If the given object is duck casted, the original duck object; otherwise, the same object that was given.</returns>
        Object Uncast(Object duck);

        /// <summary>
        /// Determines whether a given object can be casted to a given type.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="duck">The object to cast.</param>
        /// <returns>If the given object can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanCast<T>(object duck);

        /// <summary>
        /// Determines whether a given object can be casted to a given type.
        /// </summary>
        /// <param name="toType">Type to cast to.</param>
        /// <param name="duck">The object to cast.</param>
        /// <returns>If the given object can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanCast(Type toType, object duck);

        /// <summary>
        /// Determines whether a type can be casted to another type.
        /// </summary>
        /// <typeparam name="TTo">Type to cast to.</typeparam>
        /// <typeparam name="TFrom">Type of object to be casted.</typeparam>
        /// <returns>If an object of the given from type can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanCast<TTo, TFrom>();

        /// <summary>
        /// Determines whether a type can be casted to type T.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="duckType">Type of object to be casted.</param>
        /// <returns>If an object of the given type can be casted to T, true; otherwise, false.</returns>
        Boolean CanCast<T>(Type duckType);

        /// <summary>
        /// Determines whether a type can be casted to another type.
        /// </summary>
        /// <param name="toType">Type to cast to.</param>
        /// <param name="fromType">Type of object to be casted.</param>
        /// <returns>If an object of the given from type can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanCast(Type toType, Type fromType);

        /// <summary>
        /// Determines whether a static type can be casted to another type.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="staticType">Static type to be casted.</param>
        /// <returns>If the given static type can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanStaticCast<T>(Type staticType);

        /// <summary>
        /// Determines whether a static type can be casted to another type.
        /// </summary>
        /// <param name="toType">Type to cast to.</param>
        /// <param name="staticType">Static type to be casted.</param>
        /// <returns>If the given static type can be casted to the given to type, true; otherwise, false.</returns>
        Boolean CanStaticCast(Type toType, Type staticType);
    }
}