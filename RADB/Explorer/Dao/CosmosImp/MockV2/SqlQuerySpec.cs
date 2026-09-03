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
	/// Represents a SQL query in the Azure Cosmos DB service.
	/// </summary>
	[DataContract]
	public sealed class SqlQuerySpec
	{
		private string queryText;

		private SqlParameterCollection parameters;

		/// <summary>
		/// Gets or sets the text of the Azure Cosmos DB database query.
		/// </summary>
		/// <value>The text of the database query.</value>
		[DataMember(Name = "query")]
		public string QueryText
		{
			get
			{
				return this.queryText;
			}
			set
			{
				this.queryText = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Microsoft.Azure.Documents.SqlParameterCollection" /> instance, which represents the collection of Azure Cosmos DB query parameters.
		/// </summary>
		/// <value>The <see cref="T:Microsoft.Azure.Documents.SqlParameterCollection" /> instance.</value>
		[DataMember(Name = "parameters")]
		public SqlParameterCollection Parameters
		{
			get
			{
				return this.parameters;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this.parameters = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlQuerySpec" /> class for the Azure Cosmos DB service.</summary>
		/// <remarks> 
		/// The default constructor initializes any fields to their default values.
		/// </remarks>
		public SqlQuerySpec() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlQuerySpec" /> class for the Azure Cosmos DB service.
		/// </summary>
		/// <param name="queryText">The text of the query.</param>
		public SqlQuerySpec(string queryText) : this(queryText, new SqlParameterCollection())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlQuerySpec" /> class for the Azure Cosmos DB service.
		/// </summary>
		/// <param name="queryText">The text of the database query.</param>
		/// <param name="parameters">The <see cref="T:Microsoft.Azure.Documents.SqlParameterCollection" /> instance, which represents the collection of query parameters.</param>
		public SqlQuerySpec(string queryText, SqlParameterCollection parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			this.queryText = queryText;
			this.parameters = parameters;
		}

		/// <summary>
		/// Returns a value that indicates whether the Azure Cosmos DB database <see cref="P:Microsoft.Azure.Documents.SqlQuerySpec.Parameters" /> property should be serialized.
		/// </summary>
		public bool ShouldSerializeParameters()
		{
			return this.parameters.Count > 0;
		}
	}
}
