const CustomColumnType = {
    SingleText: 1,
    YesOrNo: 2,
    DateTime: 3,
    Number: 4,
};

const MetadataColumnTypes = [
    {
        name: RMResx.RM_JS_SP_ManageMetadata_ColumnType_Text,
        value: CustomColumnType.SingleText,
        checked: true,
    },
    {
        name: RMResx.RM_JS_SP_ManageMetadata_ColumnType_YesOrNo,
        value: CustomColumnType.YesOrNo,
        checked: false,
    },
    {
        name: RMResx.RM_JS_SP_ManageMetadata_ColumnType_DateTime,
        value: CustomColumnType.DateTime,
        checked: false,
    },
    {
        name: RMResx.RM_JS_SP_ManageMetadata_ColumnType_Number,
        value: CustomColumnType.Number,
        checked: false,
    },
];

export { CustomColumnType, MetadataColumnTypes };
