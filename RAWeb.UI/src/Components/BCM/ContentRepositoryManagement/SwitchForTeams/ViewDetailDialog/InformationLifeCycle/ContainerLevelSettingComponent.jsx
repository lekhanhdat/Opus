import { LicenseHelper } from "../../../../../../Utilities/CommonUtil";
import StringUtil from "../../../../../../Utilities/StringUtil";

function ContainerLevelSettingComponent({ nodeSetting }) {
    return (
        <R.Expander
            title={RMResx.RM_JS_SPS_EditTitle_ContainerLevelTermSetting}
            level={2}
        >
            <div>
                {nodeSetting.Level == 2 && (
                    <$g.DetailList>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_BCM_Explorer_Details_TermName
                                )}
                            >
                                <span tabIndex="0">
                                    {(nodeSetting.IsClassificationTermDeprecated ||
                                        nodeSetting.IsClassificationTermRemoved) && (
                                        <div className="info-error">
                                            <div className="info-error-icon">
                                                <span className="fia-status-error info-error-tab"></span>
                                            </div>
                                        </div>
                                    )}
                                    <div className="ra-setting-termPath">
                                        <span>
                                            {nodeSetting.ContainerTermFullPath}
                                        </span>
                                    </div>
                                    {nodeSetting.IsClassificationTermRemoved && (
                                        <span className="info-error-font">
                                            {RMResx.RM_JS_SPS_TermDelete}
                                        </span>
                                    )}
                                    {!nodeSetting.IsClassificationTermRemoved &&
                                        nodeSetting.IsClassificationTermDeprecated && (
                                            <span className="info-error-font">
                                                {RMResx.RM_JS_SPS_IsTermRetired}
                                            </span>
                                        )}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_EditKey_ColumnNameDescription
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.DescriptionOfContainer}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {LicenseHelper.EnableRecordsArchiver() && (
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_EnableInheritParentTerm}>
                                    <span tabIndex="0">{nodeSetting.IsInheritParentTerm ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                    </$g.DetailList>
                )}
                {nodeSetting.Level != 2 && (
                    <$g.DetailList>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_EditKey_EnableClassification
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.isEnableClassification
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_BCM_Explorer_Details_TermName
                                )}
                            >
                                <span tabIndex="0">
                                    {(nodeSetting.IsClassificationTermDeprecated ||
                                        nodeSetting.IsClassificationTermRemoved) && (
                                        <div className="info-error">
                                            <div className="info-error-icon">
                                                <span className="fia-status-error info-error-tab"></span>
                                            </div>
                                        </div>
                                    )}
                                    <div className="ra-setting-termPath">
                                        <span>
                                            {nodeSetting.ContainerTermFullPath}
                                        </span>
                                    </div>
                                    {nodeSetting.IsClassificationTermRemoved && (
                                        <span className="info-error-font">
                                            {RMResx.RM_JS_SPS_TermDelete}
                                        </span>
                                    )}
                                    {!nodeSetting.IsClassificationTermRemoved &&
                                        nodeSetting.IsClassificationTermDeprecated && (
                                            <span className="info-error-font">
                                                {RMResx.RM_JS_SPS_IsTermRetired}
                                            </span>
                                        )}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_EditKey_ColumnNameDescription
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.DescriptionOfContainer}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                        {LicenseHelper.EnableRecordsArchiver() && (
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_SPS_EditKey_EnableInheritParentTerm}>
                                    <span tabIndex="0">{nodeSetting.IsInheritParentTerm ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                    </$g.DetailList>
                )}
            </div>
        </R.Expander>
    );
}

export default ContainerLevelSettingComponent;
