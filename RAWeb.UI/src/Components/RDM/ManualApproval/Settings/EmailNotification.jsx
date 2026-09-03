import React, { useEffect, useState } from "react";
import { EndType, EndTypeI18Ns, IntervalType, IntervalTypeI18Ns, NotificationType } from "../Constants/index";
import _ from "lodash";

const BuildSelectorItems = (
    selectedItem = IntervalType.Days,
    options = [IntervalType.Days, IntervalType.Weeks]
) => {
    const result = [];
    for (const option of options) {
        const optionValue = IntervalTypeI18Ns.get(option);
        result.push({
            key: option,
            value: optionValue,
            checked: selectedItem === option,
        });
    }
    return result;
};

const EmailNotification = ({ emailNotificationSetting, onChange }) => {

    const [selectorItems, setSelectorItems] = useState([]);

    const [showValidateMessage, setShowValidateMessage] = useState(false);

    useEffect(() => {
        const buildedSelectorItems = BuildSelectorItems(emailNotificationSetting.IntervalType);
        setSelectorItems(buildedSelectorItems);

        if (emailNotificationSetting.ManualApprovalSettingType === NotificationType.Interval || (emailNotificationSetting.ManualApprovalSettingType === NotificationType.Advanced && emailNotificationSetting.AdvanceEmailSetting.length <= 10)) {
            setShowValidateMessage(false);
        }
    }, [emailNotificationSetting]);

    const onChangeInterval = (value) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        if(_.isNil(value) || value === "") {
            value = "1";
        }
        clonedSetting.Interval = parseInt(value);
        onChange(clonedSetting);
    };

    const onChangeIntervalType = (args) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        clonedSetting.IntervalType = args.newValue.key;
        onChange(clonedSetting);
    };

    const onChangeEndType = (value) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        clonedSetting.EndType = value;
        onChange(clonedSetting);
    };

    const onChangeOccurrences = (value) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        if(_.isNil(value) || value === "") {
            value = "1";
        }
        clonedSetting.OccurrencesTimes = parseInt(value);
        onChange(clonedSetting);
    };

    const onChangeRadioType = (value) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        clonedSetting.ManualApprovalSettingType = value;
        onChange(clonedSetting);
    };

    const onChangeIndexInterval = (index, value) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        if (_.isNil(value) || value === "") {
            value = "1";
        }
        clonedSetting.AdvanceEmailSetting[index].Interval = parseInt(value);
        onChange(clonedSetting);
    };

    const removeCondition = (index) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        clonedSetting.AdvanceEmailSetting.splice(index, 1);
        onChange(clonedSetting);
        setShowValidateMessage(false);
    };

    const addCondition = (index) => {
        const clonedSetting = _.cloneDeep(emailNotificationSetting);
        if (clonedSetting.AdvanceEmailSetting.length < 10) {
            clonedSetting.AdvanceEmailSetting.splice(index + 1, 0, { Interval: 1, IntervalType: IntervalType.Days });
            onChange(clonedSetting);
            setShowValidateMessage(false);
        } else {
            setShowValidateMessage(true);
            return false;
        }
    };

    const mapAdvanced = (advanced, index) => {
        return <div className="ra-advance-group-popup-row" key={`advanced_${index}`}>
            <div>
                {emailNotificationSetting.AdvanceEmailSetting.length > 1 && <div className="ra-advance-group-text">
                    {`${index + 1}. `}
                </div>}
                <div className="ra-advance-group-text">
                    {index === 0 ? RMResx.RM_MA_Setting_Advanced_After : RMResx.RM_MA_Setting_Advanced_Thereafter}
                </div>
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="number"
                    min={1}
                    width={124}
                    value={advanced.Interval}
                    hasControl
                    onChange={onChangeIndexInterval.bind(this, index)}
                />
            </div>
            <div className="ra-advance-group-text">{RMResx.RM_JS_ScheduleSetting_Days}</div>
            {emailNotificationSetting.AdvanceEmailSetting.length > 1 && <R.Button
                type="bald"
                icon="crm-criteria fia-close"
                tooltip={RMResx.RM_JS_Common_Delete}
                onClick={removeCondition.bind(this, index)}
            />}
            <R.Button
                type="bald"
                icon="crm-criteria fia-plus"
                tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                onClick={addCondition.bind(this, index)}
            />
        </div>;
    };

    const renderInterval = () => {
        return <div className="reco-manual-setting-email-notification">
            <div className="reco-manual-setting-text" style={{ lineHeight: "34px" }}>
                {`${RMResx.RM_TS_IntervalTime}`}
            </div>
            <div className="reco-manual-setting-email-notification-options">
                <div>
                    <R.Input
                        key={Math.random()}
                        type="number"
                        min={1}
                        max={100}
                        width={"100%"}
                        value={emailNotificationSetting.Interval}
                        hasControl
                        onChange={onChangeInterval}
                    />
                </div>
                <div>
                    <R.Combobox
                        checkedField="checked"
                        textField="value"
                        valueField="key"
                        width={"100%"}
                        hasFilter={false}
                        searchable={false}
                        items={selectorItems}
                        onChange={onChangeIntervalType}
                    />
                </div>
            </div>
            <div className="reco-manual-setting-text">
                {`${RMResx.RM_JS_ScheduleSetting_EndTime}:`}
            </div>
            <div className="reco-manual-setting-email-notification-endtype">
                <R.Radio
                    name="endType_noEnd"
                    text={EndTypeI18Ns.get(EndType.NoEnd)}
                    value={EndType.NoEnd}
                    checked={emailNotificationSetting.EndType === EndType.NoEnd}
                    onChange={onChangeEndType}
                />
                <div className="reco-manual-setting-email-notification-occurrences">
                    <R.Radio
                        name="endType_occurrences"
                        text={EndTypeI18Ns.get(EndType.EndOccurrences)}
                        value={EndType.EndOccurrences}
                        checked={emailNotificationSetting.EndType === EndType.EndOccurrences}
                        onChange={onChangeEndType}
                    />
                    <div>
                        <R.Input
                            key={Math.random()}
                            type="number"
                            min={1}
                            max={100}
                            width={100}
                            value={emailNotificationSetting.OccurrencesTimes}
                            hasControl
                            onChange={onChangeOccurrences}
                            disabled={emailNotificationSetting.EndType !== EndType.EndOccurrences}
                        />
                        <span style={{ marginLeft: "8px" }}>{RMResx.RM_JS_ScheduleSetting_Occurrences}</span>
                    </div>
                </div>
            </div>
        </div>;
    };

    const renderAdvanced = () => {
        return <div className="ra-email-notification-advanced">
            <div className="ra-advanced-configure">
                <span>{RMResx.RM_MA_Setting_Advanced_Config}</span>
                <$g.Popover>{RMResx.RM_MA_Setting_Advanced_ConfigDes}</$g.Popover>
            </div>
            <div className={emailNotificationSetting.AdvanceEmailSetting.length === 1 ? "ra-advance-after" : "ra-advance-group"}>
                {emailNotificationSetting.AdvanceEmailSetting.map((advanced, index) => {
                    return mapAdvanced(advanced, index);
                })}
            </div>
            <div className="ra-validation-msg" style={{ marginTop: "5px" }} tabIndex="0" hidden={!showValidateMessage}>{RMResx.RM_MA_Setting_Advanced_AddError}</div>
        </div>;
    };

    return (
        <section className="reco-manual-setting-section">
            <div className="reco-manual-setting-section-title" tabIndex="0">
                {RMResx.RM_MA_Setting_Notification}
            </div>
            <div className="margin-bottom-s">
                <div className="margin-bottom-s">
                    <R.Radio
                        name="notificationSetting"
                        text={RMResx.RM_MA_Setting_Notification_ByInterval}
                        value={NotificationType.Interval}
                        checked={(emailNotificationSetting.ManualApprovalSettingType || 1) === NotificationType.Interval}
                        onChange={onChangeRadioType}
                    />
                </div>
                {(emailNotificationSetting.ManualApprovalSettingType || 1) === NotificationType.Interval && renderInterval()}
            </div>
            <div>
                <div className="margin-bottom-s">
                    <R.Radio
                        name="notificationSetting"
                        text={RMResx.RM_MA_Setting_Notification_Advanced}
                        value={NotificationType.Advanced}
                        checked={emailNotificationSetting.ManualApprovalSettingType === NotificationType.Advanced}
                        onChange={onChangeRadioType}
                    />
                </div>
                {emailNotificationSetting.ManualApprovalSettingType === NotificationType.Advanced && renderAdvanced()}
            </div>
        </section>
    );
};

export default EmailNotification;