const SFObjectType = {
    StandardObject: 0,
    CustomObject: 1,
    FileObject: 2,
    AttachmentObject: 3,
};

const SFObjectTypeI18ns = new Map([
    [SFObjectType.StandardObject, RMResx.RM_FA_SF_ObjectType_Standard],
    [SFObjectType.CustomObject, RMResx.RM_FA_SF_ObjectType_Custom],
])

export {SFObjectType, SFObjectTypeI18ns};