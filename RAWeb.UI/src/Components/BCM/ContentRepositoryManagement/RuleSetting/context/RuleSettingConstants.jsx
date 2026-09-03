export const RuleLevelType = {
    SiteCollection: 2,
    Site: 4,
    List: 8,
    Folder: 16,
    Item: 32,
    Document: 64,
    Attachment: 128,
    DocumentVersion: 256,
    ItemVersion: 512,
    Teams: 33554432,
};

export const ObjectLevel = {
    SiteCollection: { name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection, value: RuleLevelType.SiteCollection },
    Site: { name: RMResx.RM_JS_Rule_ObjectLevel_Site, value: RuleLevelType.Site },
    List: { name: RMResx.RM_JS_Rule_ObjectLevel_List, value: RuleLevelType.List },
    Folder: { name: RMResx.RM_JS_Rule_ObjectLevel_Folder, value: RuleLevelType.Folder },
    Attachment: { name: RMResx.RM_JS_Rule_ObjectLevel_Attachment, value: RuleLevelType.Attachment },
    ItemVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_ItemVersion, value: RuleLevelType.ItemVersion },
    Item: { name: RMResx.RM_JS_Rule_ObjectLevel_Item, value: RuleLevelType.Item },
    DocumentVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion, value: RuleLevelType.DocumentVersion },
    Document: { name: RMResx.RM_JS_Rule_ObjectLevel_Document, value: RuleLevelType.Document },
};

export const ObjectLevel4Teams = {
    Teams: { name: RMResx.RM_JS_Rule_ObjectLevel_Teams, value: RuleLevelType.Teams },
    SiteCollection: { name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection, value: RuleLevelType.SiteCollection },
    Site: { name: RMResx.RM_JS_Rule_ObjectLevel_Site, value: RuleLevelType.Site },
    List: { name: RMResx.RM_JS_Rule_ObjectLevel_List, value: RuleLevelType.List },
    Folder: { name: RMResx.RM_JS_Rule_ObjectLevel_Folder, value: RuleLevelType.Folder },
    Attachment: { name: RMResx.RM_JS_Rule_ObjectLevel_Attachment, value: RuleLevelType.Attachment },
    ItemVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_ItemVersion, value: RuleLevelType.ItemVersion },
    Item: { name: RMResx.RM_JS_Rule_ObjectLevel_Item, value: RuleLevelType.Item },
    DocumentVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion, value: RuleLevelType.DocumentVersion },
    Document: { name: RMResx.RM_JS_Rule_ObjectLevel_Document, value: RuleLevelType.Document },
};