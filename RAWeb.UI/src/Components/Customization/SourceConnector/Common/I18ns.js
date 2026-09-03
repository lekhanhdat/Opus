import { ColumnType } from "./Constants";

export const ColumnTypeI18ns = new Map([
    [ColumnType.SingleText, RMResx.RM_PRM_EditTemplate_ColumnType_SingleText],
    [ColumnType.MultipleText, RMResx.RM_PRM_EditTemplate_ColumnType_MultipleText],
    [ColumnType.DateTime, RMResx.RM_PRM_EditTemplate_ColumnType_DateTime],
    [ColumnType.SingleChoice, RMResx.RM_PRM_EditTemplate_ColumnType_SingleChoice],
    [ColumnType.MultipleChoice, RMResx.RM_PRM_EditTemplate_ColumnType_MultipleChoice],
    [ColumnType.Number, RMResx.RM_PRM_EditTemplate_ColumnType_Number],
    [ColumnType.PeopleOrGroup, RMResx.RM_PRM_EditTemplate_ColumnType_PeopleorGroup],
    [ColumnType.Taxonomy, RMResx.RM_PRM_EditTemplate_ColumnType_Taxonomy],
]);