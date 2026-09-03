import React, { useEffect, useState, useImperativeHandle, forwardRef } from "react";
import _ from "lodash";
import PeoplePicker from "../../../Common/PeoplePicker";
import { ApprovalStatus, EscalateSettingType, EscalateSettingTypeI18NS } from "../Constants/index";

const BuildRadioGroupItems = (checkedStatus) => {
    return ([
        {
            text: RMResx.RM_MA_Setting_Escalation_Workflow_Reject,
            value: ApprovalStatus.Rejected,
            checked: checkedStatus === ApprovalStatus.Rejected
        },
        {
            text: RMResx.RM_MA_Setting_Escalation_Workflow_Approve,
            value: ApprovalStatus.Approved,
            checked: checkedStatus === ApprovalStatus.Approved
        }
    ]);
};

const EscalationSetting = ({ escalationSetting, onChange }, ref) => {

    const [radioItems, setRadioItems] = useState([]);

    const [showValidateMessage, setShowValidateMessage] = useState(false);

    useEffect(() => {
        const buildedItems = BuildRadioGroupItems(escalationSetting.ApprovalStatus);
        setRadioItems(buildedItems);
        if (escalationSetting.EscalateSettingType !== EscalateSettingType.ReassignSpecificUsers || escalationSetting.ReassignUsers.length > 0) {
            setShowValidateMessage(false);
        }
    }, [escalationSetting]);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            if (escalationSetting.EscalateSettingType !== EscalateSettingType.ReassignSpecificUsers) {
                return true;
            }

            if (escalationSetting.ReassignUsers.length === 0) {
                setShowValidateMessage(true);
                return false;
            }

            return true;
        }
    }));

    const onChangeApprovalStatus = (value) => {
        const clonedSetting = _.cloneDeep(escalationSetting);
        clonedSetting.ApprovalStatus = value;
        onChange(clonedSetting);
    };

    const onChangeEscalateSettingType = (value) => {
        const clonedSetting = _.cloneDeep(escalationSetting);
        clonedSetting.EscalateSettingType = value;
        onChange(clonedSetting);
    };

    const onChangeReassignUsers = (value) => {
        const clonedSetting = _.cloneDeep(escalationSetting);
        clonedSetting.ReassignUsers = value;
        if (_.isNil(value)) {
            clonedSetting.ReassignUsers = [];
        }

        onChange(clonedSetting);
    };

    return (
        <section className="reco-manual-setting-section">
            <div className="reco-manual-setting-section-title" tabIndex="0">
                {RMResx.RM_MA_Setting_Escalation}
            </div>
            <div className="reco-manual-setting-escalation-setting">
                <R.Radio
                    name="escalationSetting"
                    text={EscalateSettingTypeI18NS.get(EscalateSettingType.NoAction)}
                    value={EscalateSettingType.NoAction}
                    checked={escalationSetting.EscalateSettingType === EscalateSettingType.NoAction}
                    onChange={onChangeEscalateSettingType}
                />
                <div style={{ marginTop: "8px" }}>
                    <R.Radio
                        name="escalationSetting"
                        text={EscalateSettingTypeI18NS.get(EscalateSettingType.WorkflowNextStep)}
                        value={EscalateSettingType.WorkflowNextStep}
                        checked={escalationSetting.EscalateSettingType === EscalateSettingType.WorkflowNextStep}
                        onChange={onChangeEscalateSettingType}
                    />
                    <div style={{ marginTop: "8px", paddingLeft: "24px" }} hidden={escalationSetting.EscalateSettingType !== EscalateSettingType.WorkflowNextStep}>
                        <R.Radio.Group
                            name="escalationSetting_workflow"
                            items={radioItems}
                            block={true}
                            onChange={onChangeApprovalStatus}
                        />
                    </div>
                </div>
                <div style={{ marginTop: "8px" }}>
                    <R.Radio
                        name="escalationSetting"
                        text={EscalateSettingTypeI18NS.get(EscalateSettingType.ReassignSpecificUsers)}
                        value={EscalateSettingType.ReassignSpecificUsers}
                        checked={escalationSetting.EscalateSettingType === EscalateSettingType.ReassignSpecificUsers}
                        onChange={onChangeEscalateSettingType}
                    />
                </div>
                <div style={{ marginTop: "8px", marginLeft: "26px" }} hidden={escalationSetting.EscalateSettingType !== EscalateSettingType.ReassignSpecificUsers}>
                    <PeoplePicker
                        width="100%"
                        items={escalationSetting.ReassignUsers}
                        selectionChanged={onChangeReassignUsers}
                    />
                    <div className="ra-validation-msg" style={{ marginTop: "5px" }} tabIndex="0" hidden={!showValidateMessage}>{RMResx.RM_MA_Setting_Escalation_Reassign_NoAddUser}</div>
                </div>
            </div>
        </section>
    );
};

export default forwardRef(EscalationSetting);