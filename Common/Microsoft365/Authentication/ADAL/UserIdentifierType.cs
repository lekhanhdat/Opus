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
namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Indicates the type of <see cref="T:Portal.ADAL.UserIdentifier" />
	/// </summary>
	public enum UserIdentifierType
	{
		/// <summary>
		/// When a <see cref="T:Portal.ADAL.UserIdentifier" /> of this type is passed in a token acquisition operation,
		/// the operation is guaranteed to return a token issued for the user with corresponding <see cref="P:Portal.ADAL.UserIdentifier.UniqueId" /> or fail.
		/// </summary>
		UniqueId,
		/// <summary>
		/// When a <see cref="T:Portal.ADAL.UserIdentifier" /> of this type is passed in a token acquisition operation,
		/// the operation restricts cache matches to the value provided and injects it as a hint in the authentication experience. However the end user could overwrite that value, resulting in a token issued to a different account than the one specified in the <see cref="T:Portal.ADAL.UserIdentifier" /> in input.
		/// </summary>
		OptionalDisplayableId,
		/// <summary>
		/// When a <see cref="T:Portal.ADAL.UserIdentifier" /> of this type is passed in a token acquisition operation,
		/// the operation is guaranteed to return a token issued for the user with corresponding <see cref="P:Portal.ADAL.UserIdentifier.DisplayableId" /> (UPN or email) or fail
		/// </summary>
		RequiredDisplayableId
	}
}