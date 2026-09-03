import CRMCommonUtil from "../../../Common/CRMCommonUtil";
import Enviroments from "../../../../../../Constants/Enviroments";
import { NodeLevel } from "../../../../../../Constants/DAEnums";
import StringUtil from "../../../../../../Utilities/StringUtil";

function ColumnSettingComponent({ nodeSetting, isCSDTenant }) {
    const getKeepSpDefaultValueSettingContent = (setting) => {
        if (
            setting.IsKeepSharePointDefaultValue &&
            setting.SetTermForEmptyDefaultValue
        ) {
            return `${RMResx.RM_JS_Common_Yes}; ${RMResx.RM_SPS_NoSetTermForEmptyDefaultValue_Title}`;
        }
        return setting.IsKeepSharePointDefaultValue
            ? RMResx.RM_JS_Common_Yes
            : RMResx.RM_JS_Common_No;
    };

    const hasConfigColumn = (setting) => {
        return (
            !CRMCommonUtil.guidIsEmpty(setting.ColumnName) ||
            !CRMCommonUtil.guidIsEmpty(setting.IsUsingExistColumnName)
        );
    };

    return (
        <R.Expander title={RMResx.RM_JS_SPS_EditTitle_ColumnSetting} level={2}>
            <div>
                {nodeSetting.IsUsingExistColumnName && (
                    <$g.DetailList>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_EnterColNameDesc
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.ExistColumnName}
                                </span>
                                {nodeSetting.SetDocLevelTermForExistColumn && (
                                    <span tabIndex="0">
                                        {RMResx.RM_JS_SPS_ExistingColumn.replace(
                                            "{0}",
                                            RMResx.RM_JS_SPS_UseTermSettingsDefinedInRecords
                                        )}
                                    </span>
                                )}
                                {!nodeSetting.SetDocLevelTermForExistColumn && (
                                    <span tabIndex="0">
                                        {RMResx.RM_JS_SPS_ExistingColumn.replace(
                                            "{0}",
                                            RMResx.RM_JS_SPS_UseTermSettingsDefinedInSP
                                        )}
                                    </span>
                                )}
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_SPS_EditKey_ShowUniqueID}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.IsShowUniqueId
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {!isCSDTenant && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_JS_SPS_EditKey_KeepSPDefaultValue
                                    }
                                >
                                    <span tabIndex="0">
                                        {getKeepSpDefaultValueSettingContent(
                                            nodeSetting
                                        )}
                                    </span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                        {RM.gData.enviromentName !== Enviroments.ChinaNorth &&
                            nodeSetting.Level == NodeLevel.WebApplication && (
                                <$g.DetailRow>
                                    <$g.DetailCell
                                        label={RMResx.RM_SP_SettingRelatedRecords}
                                    >
                                        <span tabIndex="0">
                                            {nodeSetting.EnableRelatedRecords
                                                ? RMResx.RM_JS_Common_Yes
                                                : RMResx.RM_JS_Common_No}
                                        </span>
                                    </$g.DetailCell>
                                </$g.DetailRow>
                            )}
                    </$g.DetailList>
                )}
                {!nodeSetting.IsUsingExistColumnName && (
                    <$g.DetailList>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_EnterColNameDesc
                                )}
                            >
                                <span tabIndex="0">{nodeSetting.ColumnName}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={
                                    RMResx.RM_JS_SPS_EditKey_ColumnNameDescription
                                }
                            >
                                <span tabIndex="0">{nodeSetting.Description}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {!isCSDTenant && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={RMResx.RM_JS_SPS_HiddenColumn}
                                >
                                    {hasConfigColumn(nodeSetting) && (
                                        <span tabIndex="0">
                                            {nodeSetting.ColumnHidden
                                                ? RMResx.RM_JS_Common_Yes
                                                : RMResx.RM_JS_Common_No}
                                        </span>
                                    )}
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_DisplayColumnRequired
                                )}
                            >
                                {hasConfigColumn(nodeSetting) && (
                                    <span tabIndex="0">
                                        {nodeSetting.ColumnRequired
                                            ? RMResx.RM_JS_Common_Yes
                                            : RMResx.RM_JS_Common_No}
                                    </span>
                                )}
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_SPS_EditKey_ShowUniqueID}
                            >
                                {hasConfigColumn(nodeSetting) && (
                                    <span tabIndex="0">
                                        {nodeSetting.IsShowUniqueId
                                            ? RMResx.RM_JS_Common_Yes
                                            : RMResx.RM_JS_Common_No}
                                    </span>
                                )}
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {!isCSDTenant && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_JS_SPS_EditKey_KeepSPDefaultValue
                                    }
                                >
                                    {hasConfigColumn(nodeSetting) && (
                                        <span tabIndex="0">
                                            {getKeepSpDefaultValueSettingContent(
                                                nodeSetting
                                            )}
                                        </span>
                                    )}
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                        {RM.gData.enviromentName !== Enviroments.ChinaNorth &&
                            nodeSetting.Level == NodeLevel.WebApplication && (
                                <$g.DetailRow>
                                    <$g.DetailCell
                                        label={RMResx.RM_SP_SettingRelatedRecords}
                                    >
                                        {hasConfigColumn(nodeSetting) && (
                                            <span tabIndex="0">
                                                {nodeSetting.EnableRelatedRecords
                                                    ? RMResx.RM_JS_Common_Yes
                                                    : RMResx.RM_JS_Common_No}
                                            </span>
                                        )}
                                    </$g.DetailCell>
                                </$g.DetailRow>
                            )}
                    </$g.DetailList>
                )}
            </div>
        </R.Expander>
    );
}

export default ColumnSettingComponent;
