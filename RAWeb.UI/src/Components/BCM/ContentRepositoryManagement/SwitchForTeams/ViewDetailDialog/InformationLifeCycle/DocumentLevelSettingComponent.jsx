import CRMCommonUtil from "../../../Common/CRMCommonUtil";
import StringUtil from "../../../../../../Utilities/StringUtil";
import {
    ApplyExistType,
    ArtificialIntelligenceTermUseType,
    AutoJobOption,
    DeployTermMethod,
} from "../../../DocumentTermSetting/DocumentTermSettingPanel";
import { SelectProcessType } from "../../../ManualApprovalSetting/ManualApprovalSettingPanel";
import Enviroments from "../../../../../../Constants/Enviroments";

function DocumentLevelSettingComponent({ nodeSetting, isCSDTenant }) {
    const hasConfigDocumentLevel = () => {
        return (
            !CRMCommonUtil.guidIsEmpty(nodeSetting.TermId) ||
            !CRMCommonUtil.guidIsEmpty(nodeSetting.TermSetId)
        );
    };

    const getDeployTermMethod = () => {
        let result = "";
        if (nodeSetting.TermSetName) {
            switch (nodeSetting.DeployTermMethod) {
                case DeployTermMethod.UseDefaultValue:
                    result = RMResx.RM_JS_SPS_AutoClassification_UseDefault;
                    break;
                case DeployTermMethod.NoDefaultValue:
                    result = RMResx.RM_JS_SPS_AutoClassification_NoDefaultValue;
                    break;
                case DeployTermMethod.UseAutoClassification:
                    result = RMResx.RM_JS_SPS_AutoClassification_UseRule;
                    break;
                case DeployTermMethod.UseIntelligenceClassification:
                    result =
                        RMResx.RM_MachineLearning_DeployTermMethodIntelligence;
                    break;
                default:
                    break;
            }
        }
        return result;
    };

    const isDefaultTermConfigured = () => {
        return nodeSetting.DeployTermMethod == DeployTermMethod.UseDefaultValue;
    };

    const applyActionString = () => {
        if (nodeSetting.NeedCheckDefaultValue) {
            var includeString = nodeSetting.IncludeDeclaredRecords
                ? "; " + RMResx.RM_JS_SPS_IncludeDeclaredRecords
                : "";
            if (nodeSetting.ApplyExistType == ApplyExistType.OverWrite) {
                return (
                    RMResx.RM_JS_Common_Yes +
                    "; " +
                    RMResx.RM_JS_SPS_AutoClassification_ApplyOverwirteTerm +
                    includeString
                );
            } else if (
                nodeSetting.ApplyExistType == ApplyExistType.SkipAndKeep
            ) {
                return (
                    RMResx.RM_JS_Common_Yes +
                    "; " +
                    RMResx.RM_JS_SPS_AutoClassification_ApplySkipTerm +
                    includeString
                );
            }
        } else {
            return RMResx.RM_JS_Common_No;
        }
    };

    const isAutoClassificationConfigured = () => {
        return (
            nodeSetting.DeployTermMethod ==
            DeployTermMethod.UseAutoClassification
        );
    };

    const getFiltersInGroup = (resultFilters, filterGroups) => {
        filterGroups.forEach((filterGroup) => {
            resultFilters.push(...filterGroup.Filters);
            getFiltersInGroup(resultFilters, filterGroup.FilterGroups);
        });
    };

    const getAllFilters = (rule) => {
        let resultFilters = [];
        getFiltersInGroup(resultFilters, rule.FilterGroups);
        return resultFilters;
    };

    const renderConditionsCriteria = () => {
        let defaultRule = null;
        const autoClassificationRules = nodeSetting.AutoClassificationRules;
        return (
            <div>
                {autoClassificationRules &&
                    autoClassificationRules.length &&
                    autoClassificationRules.map((rule) => {
                        if (rule.IsDefaultRule) {
                            if (!rule.NoDefaultTerm) {
                                defaultRule = rule;
                            }
                        } else {
                            let filters = getAllFilters(rule);
                            let displayAndOrStr = rule.AndOrExpression;
                            if (filters.length == 1) {
                                displayAndOrStr = "(" + displayAndOrStr + ")";
                            }
                            return (
                                <div key={StringUtil.newGuid()}>
                                    {filters.map((f) => {
                                        return (
                                            <div
                                                key={StringUtil.newGuid()}
                                                className="margin-top-xs"
                                            >
                                                {f.FilterCretia}
                                            </div>
                                        );
                                    })}
                                    <div>{displayAndOrStr}</div>
                                    <div className="margin-bottom-l">
                                        <span>{`${RMResx.RM_JS_SPS_AutoClassification_DisplayPolicyApplyTerm} `}</span>
                                        {(rule.TermIsRemoved ||
                                            rule.TermIsDeprecated) && (
                                            <span className="fia-status-error info-error-tab"></span>
                                        )}
                                        <span>{rule.TermName}</span>
                                        {rule.TermIsRemoved && (
                                            <span className="info-error-font">
                                                {RMResx.RM_JS_SPS_TermDelete}
                                            </span>
                                        )}
                                        {!rule.TermIsRemoved &&
                                            rule.TermIsDeprecated && (
                                                <span className="info-error-font">
                                                    {
                                                        RMResx.RM_JS_SPS_IsTermRetired
                                                    }
                                                </span>
                                            )}
                                    </div>
                                </div>
                            );
                        }
                    })}
                {defaultRule && (
                    <div key={StringUtil.newGuid()} className="margin-top-l">
                        <span>{`${RMResx.RM_JS_SPS_AutoClassification_DisplayPolicyDefaultTerm} `}</span>
                        {(defaultRule.TermIsRemoved ||
                            defaultRule.TermIsDeprecated) && (
                            <span className="fia-status-error info-error-tab"></span>
                        )}
                        <span>{defaultRule.TermName}</span>
                        {defaultRule.TermIsRemoved && (
                            <span className="info-error-font">
                                {RMResx.RM_JS_SPS_TermDelete}
                            </span>
                        )}
                        {!defaultRule.TermIsRemoved &&
                            defaultRule.TermIsDeprecated && (
                                <span className="info-error-font">
                                    {RMResx.RM_JS_SPS_IsTermRetired}
                                </span>
                            )}
                    </div>
                )}
            </div>
        );
    };

    const isAIClassificationConfigured = () => {
        return (
            nodeSetting.DeployTermMethod ==
            DeployTermMethod.UseIntelligenceClassification
        );
    };

    const isAIClassificationConfiguredOrAiInAuto = () => {
        return (
            isAIClassificationConfigured() ||
            nodeSetting.AITermUseType ==
                ArtificialIntelligenceTermUseType.AutoDefault
        );
    };

    const isAIApprovalTypeSelectOwner = () => {
        return (
            nodeSetting.AIApprovalType == SelectProcessType.SelectOwnerRecords
        );
    };

    const renderUserSetting = () => {
        const aiOwner = nodeSetting.AIReviewers;
        const newAIOwner = [];
        if (aiOwner) {
            aiOwner.forEach((user) => {
                newAIOwner.push({
                    tooltip: user.UserPrincipalName,
                    name: user.DisplayName,
                    id: user.UserId,
                });
            });
        }
        return newAIOwner;
    };

    const getSkipOverrideStr = () => {
        let result = "";
        switch (nodeSetting.AutoJobOption) {
            case AutoJobOption.Skip:
                result =
                    RMResx.RM_JS_SPS_AutoClassification_SkipOverrideOption_Skip;
                break;
            case AutoJobOption.Override:
                result =
                    RMResx.RM_JS_SPS_AutoClassification_SkipOverrideOption_Override;
                break;
            default:
                break;
        }
        return result;
    };

    const applyDocumentSetsAndFoldersStringForDefault = () => {
        if (nodeSetting.ApplyTermIncludeFolder) {
            if (nodeSetting.ApplyExistType == ApplyExistType.OverWrite) {
                return (
                    RMResx.RM_JS_Common_Yes +
                    "; " +
                    RMResx.RM_JS_SPS_AutoClassification_ApplySetsOverwirteTerm
                );
            } else if (
                nodeSetting.ApplyExistType == ApplyExistType.SkipAndKeep
            ) {
                return (
                    RMResx.RM_JS_Common_Yes +
                    "; " +
                    RMResx.RM_JS_SPS_AutoClassification_ApplySetsSkipTerm
                );
            }
        } else {
            return RMResx.RM_JS_Common_No;
        }
    };

    const applyDocumentSetsAndFoldersStringForAuto = () => {
        return nodeSetting.ApplyTermIncludeFolder
            ? RMResx.RM_JS_Common_Yes
            : RMResx.RM_JS_Common_No;
    };

    return (
        <R.Expander
            title={RMResx.RM_JS_SPS_EditTitle_DocumentLevelSetting}
            level={2}
        >
            <div>
                <$g.DetailList>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={RMResx.RM_JS_SPS_EditKey_TermScope}
                        >
                            <span tabIndex="0">
                                {(nodeSetting.IsTermDeprecated ||
                                    nodeSetting.IsTermRemoved) && (
                                    <div className="info-error">
                                        <div className="info-error-icon">
                                            <span className="fia-status-error info-error-tab"></span>
                                        </div>
                                    </div>
                                )}
                                <div className="ra-setting-termPath">
                                    <span>{nodeSetting.TermScopeFullPath}</span>
                                </div>
                                {nodeSetting.IsTermRemoved && (
                                    <span className="info-error-font">
                                        {RMResx.RM_JS_SPS_TermDelete}
                                    </span>
                                )}
                                {!nodeSetting.IsTermRemoved &&
                                    nodeSetting.IsTermDeprecated && (
                                        <span className="info-error-font">
                                            {RMResx.RM_JS_SPS_IsTermRetired}
                                        </span>
                                    )}
                            </span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    {!CRMCommonUtil.isFolder(nodeSetting) && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_SPS_EditKey_TermDisplayForm}
                            >
                                {hasConfigDocumentLevel() && (
                                    <span tabIndex="0">
                                        {nodeSetting.IsDisplyaTermPath
                                            ? RMResx.RM_SPS_DisplayTerm_EntirePath
                                            : RMResx.RM_SPS_DisplayTerm_TermLabel}
                                    </span>
                                )}
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(
                                RMResx.RM_SPS_AutoClassification_DeployTermMethod
                            )}
                        >
                            <span tabIndex="0">{getDeployTermMethod()}</span>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    {isDefaultTermConfigured() && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_SPS_EditKey_DefaultValue}
                            >
                                <span tabIndex="0">
                                    {(nodeSetting.IsDefaultTermDeprecated ||
                                        nodeSetting.IsDefaultTermRemoved) && (
                                        <div className="info-error">
                                            <div className="info-error-icon">
                                                <span className="fia-status-error info-error-tab"></span>
                                            </div>
                                        </div>
                                    )}
                                    <div className="ra-setting-termPath">
                                        <span>
                                            {nodeSetting.DefaultTermFullPath}
                                        </span>
                                    </div>
                                    {nodeSetting.IsDefaultTermRemoved && (
                                        <span className="info-error-font">
                                            {RMResx.RM_JS_SPS_TermDelete}
                                        </span>
                                    )}
                                    {!nodeSetting.IsDefaultTermRemoved &&
                                        nodeSetting.IsDefaultTermDeprecated && (
                                            <span className="info-error-font">
                                                {RMResx.RM_JS_SPS_IsTermRetired}
                                            </span>
                                        )}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {isDefaultTermConfigured() && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_SPS_EditKey_Action}
                            >
                                {hasConfigDocumentLevel() && (
                                    <span tabIndex="0">
                                        {applyActionString()}
                                    </span>
                                )}
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {isAutoClassificationConfigured() && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_SPS_AutoClassification_ApplyPolicy
                                )}
                            >
                                <div tabIndex="0">
                                    {renderConditionsCriteria()}
                                </div>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}

                    {isAIClassificationConfiguredOrAiInAuto() &&
                        isAIApprovalTypeSelectOwner() && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_MachineLearning_IntelligenceReviewers
                                    }
                                >
                                    <div tabIndex="0">
                                        {renderUserSetting().map((item) => {
                                            return (
                                                <span
                                                    key={item.id}
                                                    className="ra-setting-profile margin-left-s"
                                                    data-tooltip
                                                    aria-label={item.tooltip}
                                                    tabIndex="0"
                                                >
                                                    <R.Profile
                                                        tooltip={item.tooltip}
                                                        name={item.name}
                                                        invalid="false"
                                                    ></R.Profile>
                                                </span>
                                            );
                                        })}
                                    </div>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                    {isAIClassificationConfiguredOrAiInAuto() && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_MA_IsSendEmail
                                )}
                            >
                                <div tabIndex="0">
                                    {nodeSetting.AISendEMail
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </div>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {isAIClassificationConfiguredOrAiInAuto() &&
                        nodeSetting.AIThenIsDefaultTermMethod && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_MachineLearning_IntelligenceDefaultTerm
                                    }
                                >
                                    <div tabIndex="0">
                                        {nodeSetting.AIThenDefaultTermName}
                                    </div>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}

                    {(isAutoClassificationConfigured() ||
                        isAIClassificationConfigured()) && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_SPS_AutoClassification_SkipOverrideOption
                                )}
                            >
                                <span tabIndex="0">{getSkipOverrideStr()}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {(isAutoClassificationConfigured() ||
                        isAIClassificationConfigured()) && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_SPS_AutoClassification_FullJobDescription
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.RunAutoFullJob
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {!isCSDTenant &&
                        (isDefaultTermConfigured() ||
                            isAutoClassificationConfigured()) && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={
                                        RMResx.RM_JS_SPS_Expander_IncludeDSetAndFolder
                                    }
                                >
                                    {nodeSetting.TermScopeFullPath != "" &&
                                        nodeSetting.TermScopeFullPath !=
                                            null && (
                                            <span tabIndex="0">
                                                {isAutoClassificationConfigured() ||
                                                isAIClassificationConfigured()
                                                    ? applyDocumentSetsAndFoldersStringForAuto()
                                                    : applyDocumentSetsAndFoldersStringForDefault()}
                                            </span>
                                        )}
                                </$g.DetailCell>
                            </$g.DetailRow>
                        )}
                    {isAutoClassificationConfigured() && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={
                                    RMResx.RM_JS_SPS_EditKey_IncludeDeclaredRecords
                                }
                            >
                                <span tabIndex="0">
                                    {nodeSetting.IncludeDeclaredRecords
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {RM.gData.enviromentName !== Enviroments.ChinaNorth &&
                        (CRMCommonUtil.isSiteCollection(nodeSetting) ||
                            CRMCommonUtil.isSite(nodeSetting) ||
                            CRMCommonUtil.isTeams(nodeSetting)) && (
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={RMResx.RM_SP_SettingRelatedRecords}
                                >
                                    {nodeSetting.TermScopeFullPath != "" &&
                                        nodeSetting.TermScopeFullPath !=
                                            null && (
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
            </div>
        </R.Expander>
    );
}

export default DocumentLevelSettingComponent;
