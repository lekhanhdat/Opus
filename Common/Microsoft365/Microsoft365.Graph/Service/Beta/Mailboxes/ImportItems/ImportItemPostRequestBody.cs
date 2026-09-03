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
/// Represents a request body for importing mail items into a mailbox.
/// </summary>
internal class ImportItemPostRequestBody : IAdditionalDataHolder, IBackedModel, IParsable
{
    /// <summary>
    /// Gets or sets additional data not described in the OpenAPI description.
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
    /// Gets or sets the backing store that stores model information.
    /// </summary>
    public IBackingStore BackingStore { get; private set; }

    /// <summary>
    /// Gets or sets the folder ID where the item will be imported.
    /// </summary>
    public string? FolderId
    {
        get
        {
            return BackingStore?.Get<string>("FolderId");
        }
        set
        {
            BackingStore?.Set("FolderId", value);
        }
    }

    /// <summary>
    /// Gets or sets the import mode for the mailbox item.
    /// </summary>
    public MailboxItemImportMode? Mode
    {
        get
        {
            return BackingStore?.Get<MailboxItemImportMode?>("Mode");
        }
        set
        {
            BackingStore?.Set("Mode", value);
        }
    }

    /// <summary>
    /// Gets or sets the binary data of the item to be imported.
    /// </summary>
    public byte[]? Data
    {
        get
        {
            return BackingStore?.Get<byte[]>("Data");
        }
        set
        {
            BackingStore?.Set("Data", value);
        }
    }

    public Stream? DataStream
    {
        get
        {
            return BackingStore?.Get<Stream>("DataStream");
        }
        set
        {
            BackingStore?.Set("DataStream", value);
        }
    }

    /// <summary>
    /// Gets or sets the ID of the item to be updated when Mode is set to Update.
    /// </summary>
    public string? ItemId
    {
        get
        {
            return BackingStore?.Get<string>("ItemId");
        }
        set
        {
            BackingStore?.Set("ItemId", value);
        }
    }

    /// <summary>
    /// Gets or sets the change key for the item when Mode is set to Update.
    /// </summary>
    public string? ChangeKey
    {
        get
        {
            return BackingStore?.Get<string>("ChangeKey");
        }
        set
        {
            BackingStore?.Set("ChangeKey", value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportItemPostRequestBody"/> class
    /// and sets the default values.
    /// </summary>
    public ImportItemPostRequestBody()
    {
        BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        AdditionalData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ImportItemPostRequestBody"/> class based on discriminator value.
    /// </summary>
    /// <param name="parseNode">The parse node to use to read the discriminator value and create the object.</param>
    /// <returns>A new instance of <see cref="ImportItemPostRequestBody"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when parseNode is null.</exception>
    public static ImportItemPostRequestBody CreateFromDiscriminatorValue(IParseNode parseNode)
    {
        ArgumentNullException.ThrowIfNull(parseNode);
        return new ImportItemPostRequestBody();
    }

    /// <summary>
    /// Gets the deserialization information for the current model.
    /// </summary>
    /// <returns>A dictionary containing field deserializers.</returns>
    public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>> {
        {
            "FolderId",
            delegate(IParseNode n)
            {
                FolderId = n.GetStringValue();
            }
        },
        {
            "Mode",
            delegate(IParseNode n)
            {
                Mode = n.GetEnumValue<MailboxItemImportMode>();
            }
        },
        {
            "Data",
            delegate(IParseNode n)
            {
                Data = n.GetByteArrayValue();
                DataStream = new MemoryStream(Data??[]);
            }
        },
        {
            "ItemId",
            delegate(IParseNode n)
            {
                ItemId = n.GetStringValue();
            }
        },
        {
            "ChangeKey",
            delegate(IParseNode n)
            {
                ChangeKey = n.GetStringValue();
            }
        }};
    }

    /// <summary>
    /// Serializes information from the current object.
    /// </summary>
    /// <param name="writer">Serialization writer to use to serialize this model.</param>
    /// <exception cref="ArgumentNullException">Thrown when writer is null.</exception>
    public virtual void Serialize(ISerializationWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue("FolderId", FolderId);
        writer.WriteEnumValue("Mode", Mode);
        writer.WriteStringValue("ItemId", ItemId);
        writer.WriteStringValue("ChangeKey", ChangeKey);
        writer.WriteByteArrayValue("Data", Data);
        writer.WriteAdditionalData(AdditionalData);
    }

    public Stream ToStream()
    {
        return new ImportItemPostStream(this);
    }
}

/// <summary>
/// Defines the import modes for mailbox items.
/// </summary>
public enum MailboxItemImportMode
{
    /// <summary>
    /// Create a new mailbox item.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Update an existing mailbox item.
    /// </summary>
    Update = 1
}