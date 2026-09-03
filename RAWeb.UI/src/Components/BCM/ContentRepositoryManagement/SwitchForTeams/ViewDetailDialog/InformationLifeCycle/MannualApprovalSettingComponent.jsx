import { SelectProcessType } from "../../../ManualApprovalSetting/ManualApprovalSettingPanel";
import StringUtil from "../../../../../../Utilities/StringUtil";

function MannualApprovalSettingComponent({ nodeSetting }) {
    const renderUserSetting = () => {
        let recordOwner = nodeSetting.RecordOwner;
        let newRecordOwner = [];
        if (recordOwner) {
            recordOwner.forEach((user) => {
                newRecordOwner.push({
                    tooltip: user.UserPrincipalName,
                    name: user.DisplayName,
                    id: user.UserId,
                });
            });
        }
        return newRecordOwner;
    };

    return (
        <R.Expander
            title={RMResx.RM_BCM_ManualApproval_Title_ManualApprovalSettings}
            level={2}
        >
            <div>
                <$g.DetailList>
                    {nodeSetting.ApprovalType ==
                        SelectProcessType.SelectNoneApprovalType && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={
                                    RMResx.RM_BCM_ManualApproval_Title_EnableApproval
                                }
                            >
                                <span tabIndex="0">{RMResx.RM_JS_Common_No}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {(nodeSetting.ApprovalType ==
                        SelectProcessType.SelectApprovalProcess ||
                        nodeSetting.ApprovalType ==
                            SelectProcessType.SelectOwnerRecords) && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_JS_MA_IsSendEmail
                                )}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.EMailToRecordOwner
                                        ? RMResx.RM_JS_Common_Yes
                                        : RMResx.RM_JS_Common_No}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {nodeSetting.ApprovalType ==
                        SelectProcessType.SelectApprovalProcess && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_BCM_ManualApproval_Title_Process}
                            >
                                <span tabIndex="0">
                                    {nodeSetting.WorkflowReferenceName}
                                </span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {nodeSetting.ApprovalType ==
                        SelectProcessType.SelectOwnerRecords && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={StringUtil.trimEndColon(
                                    RMResx.RM_SPS_RecordOwners
                                )}
                            >
                                {renderUserSetting().map((item) => {
                                    return (
                                        <span
                                            key={item.id}
                                            className="ra-setting-profile"
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
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                    {nodeSetting.ApprovalType ==
                        SelectProcessType.SelectAutoApprove && (
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={
                                    RMResx.RM_BCM_ManualApproval_Detail_AutoApprove
                                }
                            >
                                <span tabIndex="0">{RMResx.RM_JS_Common_Yes}</span>
                            </$g.DetailCell>
                        </$g.DetailRow>
                    )}
                </$g.DetailList>
            </div>
        </R.Expander>
    );
}

export default MannualApprovalSettingComponent;
