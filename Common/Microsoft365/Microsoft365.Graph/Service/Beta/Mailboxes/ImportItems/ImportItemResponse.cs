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
namespace Microsoft365.Graph.Service.ImportItems;

/// <summary>
/// Represents a response object for a mail item import operation in Microsoft Graph API.
/// </summary>
public class ImportItemResponse : IAdditionalDataHolder, IBackedModel, IParsable
{
    /// <summary>
    /// Gets or sets additional data not described in the OpenAPI description found when deserializing.
    /// Can be used for serialization as well.
    /// </summary>
    public IDictionary<string, object> AdditionalData
    {
        get
        {
            return BackingStore.Get<IDictionary<string, object>>("AdditionalData") ?? new Dictionary<string, object>();
        }
        set
        {
            BackingStore.Set("AdditionalData", value);
        }
    }

    /// <summary>
    /// Gets the backing store that stores model information.
    /// </summary>
    public IBackingStore BackingStore { get; private set; }

    /// <summary>
    /// Gets or sets the version of the imported item.
    /// </summary>
    public string? ChangeKey
    {
        get
        {
            return BackingStore?.Get<string>("changeKey");
        }
        set
        {
            BackingStore?.Set("changeKey", value);
        }
    }
    
    /// <summary>
    /// Gets or sets the unique identifier of the imported item.
    /// </summary>
    public string? ItemId
    {
        get
        {
            return BackingStore?.Get<string>("itemId");
        }
        set
        {
            BackingStore?.Set("itemId", value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportItemResponse"/> class and sets the
    /// default values.
    /// </summary>
    public ImportItemResponse()
    {
        BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        AdditionalData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new instance of the appropriate class based on discriminator value.
    /// </summary>
    /// <param name="parseNode">The parse node to use to read the discriminator value and create the object.</param>
    /// <returns>An <see cref="ImportItemResponse"/> instance.</returns>
    public static ImportItemResponse CreateFromDiscriminatorValue(IParseNode parseNode)
    {
        ArgumentNullException.ThrowIfNull(parseNode);
        return new ImportItemResponse();
    }

    /// <summary>
    /// Gets the deserialization information for the current model.
    /// </summary>
    /// <returns>A dictionary containing field deserializers.</returns>
    public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>>
        {
            {
                "changeKey",
                delegate(IParseNode n)
                {
                    ChangeKey = n.GetStringValue();
                }
            },
            {
                "itemId",
                delegate(IParseNode n)
                {
                    ItemId = n.GetStringValue();
                }
            }
        };
    }

    /// <summary>
    /// Serializes information from the current object.
    /// </summary>
    /// <param name="writer">Serialization writer to use to serialize this model.</param>
    public virtual void Serialize(ISerializationWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue("changeKey", ChangeKey);
        writer.WriteStringValue("itemId", ItemId);
        writer.WriteAdditionalData(AdditionalData);
    }
}
