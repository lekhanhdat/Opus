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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2
{
	/// <summary>
	/// Represents a collection of parameters associated with <see cref="T:Microsoft.Azure.Documents.SqlQuerySpec" /> for use in the Azure Cosmos DB service.
	/// </summary>
	public sealed class SqlParameterCollection : IList<SqlParameter>, ICollection<SqlParameter>, IEnumerable<SqlParameter>, IEnumerable
	{
		private readonly List<SqlParameter> parameters;

		/// <summary>
		/// Gets or sets the element at the specified index in the Azure Cosmos DB collection.
		/// </summary>
		/// <param name="index">The location in the index.</param>
		/// <value>The element at the specified index.</value>
		public SqlParameter this[int index]
		{
			get
			{
				return this.parameters[index];
			}
			set
			{
				this.parameters[index] = value;
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the Azure Cosmos DB collection.
		/// </summary>
		/// <value>The number of elements contained in the collection.</value>
		public int Count
		{
			get
			{
				return this.parameters.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Azure Cosmos DB collection is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false.</value>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Initialize a new instance of the SqlParameterCollection class for the Azure Cosmos DB service.
		/// </summary>
		public SqlParameterCollection()
		{
			this.parameters = new List<SqlParameter>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.Azure.Documents.SqlParameterCollection" /> class for the Azure Cosmos DB service.
		/// </summary>
		/// <param name="parameters">The collection of parameters.</param>
		public SqlParameterCollection(IEnumerable<SqlParameter> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			this.parameters = new List<SqlParameter>(parameters);
		}

		/// <summary>
		/// Determines the index of a specific item in the Azure Cosmos DB collection.
		/// </summary> 
		/// <param name="item">The item to find.</param>
		/// <returns>The index value for the item.</returns>
		public int IndexOf(SqlParameter item)
		{
			return this.parameters.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item at the specified index in the Azure Cosmos DB collection.
		/// </summary>
		/// <param name="index">The location in the index array in which to start inserting elements.</param>
		/// <param name="item">The item to copy into the index.</param>
		public void Insert(int index, SqlParameter item)
		{
			this.parameters.Insert(index, item);
		}

		/// <summary>
		/// Removes the item at the specified index from the Azure Cosmos DB collection.
		/// </summary>
		/// <param name="index">The location in the index where the item will be removed from.</param>
		public void RemoveAt(int index)
		{
			this.parameters.RemoveAt(index);
		}

		/// <summary>
		/// Adds an item to the Azure Cosmos DB collection.
		/// </summary>
		/// <param name="item">The item to add to the collection.</param>
		public void Add(SqlParameter item)
		{
			this.parameters.Add(item);
		}

		/// <summary>
		/// Removes all items from the Azure Cosmos DB collection.
		/// </summary>
		public void Clear()
		{
			this.parameters.Clear();
		}

		/// <summary>
		/// Determines whether the Azure Cosmos DB collection contains a specific value.
		/// </summary>
		/// <param name="item">The value to search for.</param>
		/// <returns>true if the collection contains a specific value; otherwise, false.</returns>
		public bool Contains(SqlParameter item)
		{
			return this.parameters.Contains(item);
		}

		/// <summary>
		/// Copies the elements of the Azure Cosmos DB collection to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The array to copy into.</param>
		/// <param name="arrayIndex">The location in the index array in which to start adding elements.</param>
		public void CopyTo(SqlParameter[] array, int arrayIndex)
		{
			this.parameters.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// /// Removes the first occurrence of a specific object from the Azure Cosmos DB collection.
		/// </summary>
		/// <param name="item">
		/// The item to remove from the collection.
		/// </param>
		/// <returns>true if the first item was removed; otherwise, false.</returns>
		public bool Remove(SqlParameter item)
		{
			return this.parameters.Remove(item);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the Azure Cosmos DB collection.
		/// </summary>
		/// <returns>An enumerator for the collection.</returns>
		public IEnumerator<SqlParameter> GetEnumerator()
		{
			return this.parameters.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the Azure Cosmos DB collection.
		/// </summary>
		/// <returns>An enumerator to iterate through the collection. </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.parameters.GetEnumerator();
		}
	}
}
