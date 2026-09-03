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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2
{
	/// <summary>
	/// Represents a parameter associated with <see cref="T:Microsoft.Azure.Documents.SqlQuerySpec" /> in the Azure Cosmos DB service.
	/// </summary> 
	/// <remarks>
	/// Azure Cosmos DB SQL parameters are name-value pairs referenced in parameterized queries. 
	/// Unlike in relation SQL databases, they don't have types associated with them.
	/// </remarks>
	[DataContract]
	public sealed class SqlParameter
	{
		private string name;

		private object value;

		/// <summary>
		/// Gets or sets the name of the parameter for the Azure Cosmos DB service.
		/// </summary>
		/// <value>The name of the parameter.</value>
		/// <remarks>Names of parameters must begin with '@' and be a valid SQL identifier.</remarks>
		[DataMember(Name = "name")]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		/// <summary>
		/// Gets or sets the value of the parameter for the Azure Cosmos DB service.
		/// </summary>
		/// <value>The value of the parameter.</value>
		/// <remarks>The value gets serialized and passed in as JSON to the document query.</remarks>
		[DataMember(Name = "value")]
		public object Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlParameter" /> class for the Azure Cosmos DB service.
		/// </summary>
		public SqlParameter()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlParameter" /> class with the name of the parameter for the Azure Cosmos DB service.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <remarks>Names of parameters must begin with '@' and be a valid SQL identifier.</remarks>
		public SqlParameter(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlParameter" /> class with the name and value of the parameter for the Azure Cosmos DB service.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <remarks>Names of parameters must begin with '@' and be a valid SQL identifier. The value gets serialized and passed in as JSON to the document query.</remarks>
		public SqlParameter(string name, object value)
		{
			this.name = name;
			this.value = value;
		}
	}
}
