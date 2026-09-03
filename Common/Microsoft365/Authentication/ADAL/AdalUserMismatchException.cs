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
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// The exception type thrown when user returned by service does not match user in the request.
	/// </summary>
	[Serializable]
	public class AdalUserMismatchException : AdalException
	{
		/// <summary>
		/// Gets the user requested from service.
		/// </summary>
		public string RequestedUser
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the user returned by service.
		/// </summary>
		public string ReturnedUser
		{
			get;
			private set;
		}

		/// <summary>
		///  Initializes a new instance of the exception class.
		/// </summary>
		public AdalUserMismatchException(string requestedUser, string returnedUser)
			: base("user_mismatch", string.Format(CultureInfo.InvariantCulture, "User '{0}' returned by service does not match user '{1}' in the request", new object[2]
			{
				returnedUser,
				requestedUser
			}))
		{
			RequestedUser = requestedUser;
			ReturnedUser = returnedUser;
		}

		/// <summary>
		/// Creates and returns a string representation of the current exception.
		/// </summary>
		/// <returns>A string representation of the current exception.</returns>
		public override string ToString()
		{
			return base.ToString() + string.Format(CultureInfo.InvariantCulture, "\n\tRequestedUser: {0}\n\tReturnedUser: {1}", new object[2]
			{
				RequestedUser,
				ReturnedUser
			});
		}

		/// <summary>
		/// Initializes a new instance of the exception class with serialized data.
		/// </summary>
		/// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
		protected AdalUserMismatchException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			RequestedUser = info.GetString("RequestedUser");
			ReturnedUser = info.GetString("ReturnedUser");
		}

		/// <summary>
		/// Sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
		/// </summary>
		/// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("RequestedUser", RequestedUser);
			info.AddValue("ReturnedUser", ReturnedUser);
			base.GetObjectData(info, context);
		}
	}
}