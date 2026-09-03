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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2
{
	/// <summary>
	/// Represents the connection mode to be used by the client when connecting to the Azure Cosmos DB service.
	/// </summary>
	/// <remarks>
	/// Direct and Gateway connectivity modes are supported. Gateway is the default. 
	/// </remarks>
	/// <example>
	/// <code language="c#">
	/// <![CDATA[
	/// DocumentClient client = new DocumentClient(endpointUri, masterKey, new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct });
	/// ]]>
	/// </code>
	/// </example>
	/// <seealso cref="T:Microsoft.Azure.Documents.Client.ConnectionPolicy" />
	/// <seealso cref="T:Microsoft.Azure.Documents.Client.Protocol" />
	public enum ConnectionMode
	{
		/// <summary>
		/// Use the Azure Cosmos DB gateway to route all requests to the Azure Cosmos DB service. The gateway proxies requests to the right data partition.
		/// </summary>
		/// <remarks>
		/// Use Gateway connectivity when within firewall settings do not allow Direct connectivity. All connections 
		/// are made to the database account's endpoint through the standard HTTPS port (443).
		/// </remarks>
		Gateway,
		/// <summary>
		/// Uses direct connectivity to connect to the data nodes in the Azure Cosmos DB service. Use gateway only to initialize and cache logical addresses and refresh on updates
		/// </summary>
		/// <remarks>
		/// Use Direct connectivity for best performance. Connections are made to the data nodes on Azure Cosmos DB's clusters 
		/// on a range of port numbers either using HTTPS or TCP/SSL.
		/// </remarks>
		Direct
	}


	/// <summary>
	/// Specifies the protocol to be used by DocumentClient for communicating to the Azure Cosmos DB service.
	/// </summary>
	/// <example>
	/// <code language="c#">
	/// <![CDATA[
	/// DocumentClient client = new DocumentClient(endpointUri, masterKey, new ConnectionPolicy 
	/// { 
	///     ConnectionMode = ConnectionMode.Direct,
	///     ConnectionProtocol = Protocol.Tcp
	/// }); 
	/// ]]>
	/// </code>
	/// </example>
	/// <seealso cref="T:Microsoft.Azure.Documents.Client.ConnectionMode" />
	/// <seealso cref="T:Microsoft.Azure.Documents.Client.ConnectionPolicy" />
	/// <seealso cref="T:Microsoft.Azure.Documents.Client.DocumentClient" />
	public enum Protocol
	{
		/// <summary>
		/// Specifies the HTTPS protocol.
		/// </summary>
		/// <remarks>Default connectivity.</remarks>
		Https,
		/// <summary>
		/// Specifies a custom binary protocol on TCP.
		/// </summary>
		/// <remarks>Better for performance.</remarks>
		Tcp
	}
}
