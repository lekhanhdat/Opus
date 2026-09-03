import React, { useImperativeHandle, forwardRef, useState, useRef } from "react";
import { useDidUpdateEffect } from "../Hooks/index";
import { ActionCompletedStatus, ExtendType, ExtendTypeI18Ns } from "../Constants/index";
import StringUtil from "../../../../Utilities/StringUtil";
import { showToast } from "../../../../Utilities/CommonUtil";


const BuildDefaultOptionsSelectItems = (
    latestExtendType,
    options = [ExtendType.After3Month, ExtendType.After6Month, ExtendType.After1Year, ExtendType.Custom],
    selectedItems = [ExtendType.After3Month]
) => {
    const result = [];
    for (const option of options) {
        if (option > latestExtendType) {
            break;
        }
        const optionValue = ExtendTypeI18Ns.get(option);
        result.push({
            key: option,
            value: optionValue,
            checked: selectedItems.some(item => item === option),
        });
    }
    result.push({
        key: ExtendType.Custom,
        value: ExtendTypeI18Ns.get(ExtendType.Custom),
        checked: false
    });
    return result;
};

const MaxCustomExtendTime = (latestExtendType) => {
    const now = new Date();
    if (latestExtendType === ExtendType.After1Year) {
        const year = now.getFullYear() + 1;
        now.setFullYear(year);
    }
    else if (latestExtendType === ExtendType.After3Month) {
        const month = now.getMonth() + 3;
        now.setMonth(month);
    }
    else if (latestExtendType === ExtendType.After6Month) {
        const month = now.getMonth() + 6;
        now.setMonth(month);
    }
    now.setHours(now.getHours());
    now.setMinutes(now.getMinutes());
    return now;
};

const ExtendAction = forwardRef(({ onReload, latestExtendType }, ref) => {

    const valueRef = useRef({});

    const [isShow, setIsShow] = useState(false);

    const [selectorItems, setSelectorItems] = useState([]);

    const [selectedItem, setSelectedItem] = useState(ExtendType.After3Month);

    const [comment, setComment] = useState("");

    const [customeExtendDate, setCustomExtendDate] = useState(new Date());

    const [maxCustomeExtendDate, setMaxCustomExtendDate] = useState(MaxCustomExtendTime(latestExtendType));

    const [showValidateMessage, setShowValidateMessage] = useState(false);

    const [showExceedMaxDateValidateMessage, setShowExceedMaxDateValidateMessage] = useState(false);

    useDidUpdateEffect(() => {
        valueRef.current.ExtendType = selectedItem;
        valueRef.current.Comment = comment;
        valueRef.current.CustomeExtendDate = RM.TimeUtil.getCommonDateStr(customeExtendDate);
        valueRef.current.LastExtendType = latestExtendType;
    }, [selectedItem, comment, customeExtendDate]);

    useImperativeHandle(ref, () => ({
        onShow: (itemIds) => {
            setSelectorItems(BuildDefaultOptionsSelectItems(latestExtendType));
            valueRef.current = {
                ItemIds: Array.from(itemIds),
                ExtendType: ExtendType.After3Month,
                CustomeExtendDate: RM.TimeUtil.getCommonDateStr(new Date()),
                Comment: "",
            };
            setSelectedItem(ExtendType.After3Month);
            setComment("");
            setCustomExtendDate(new Date());
            setIsShow(true);
            setShowValidateMessage(false);
            setShowExceedMaxDateValidateMessage(false);
            setMaxCustomExtendDate(MaxCustomExtendTime(latestExtendType));
        }
    }));

    const onCancel = () => {
        setIsShow(false);
    };

    const onExecuteAction = async () => {
        if (valueRef.current.ExtendType === ExtendType.Custom) {
            if(new Date(valueRef.current.CustomeExtendDate).getTime() <= new Date().getTime()){
                setShowValidateMessage(true);
                return false;
            }
            if(new Date(valueRef.current.CustomeExtendDate).getTime() >= MaxCustomExtendTime(valueRef.current.LastExtendType).getTime()){
                setShowExceedMaxDateValidateMessage(true);
                return false;
            }
        }

        $$.loading(true);
        setIsShow(false);
        const requestOptions = {
            url: "/api/ManualApproval/Extend",
            data: valueRef.current
        };
        const result = await fetchUtility(requestOptions);

        $$.loading(false);

        if (result.completedStatus == ActionCompletedStatus.Failed) {
            showToast.error(RMResx.RM_JS_MA_ExtendFailed);
            return;
        }

        showToast.success(RMResx.RM_JS_MA_ExtendSucceed);
        if (onReload) {
            onReload();
        }
    };

    const onSelectDisposalTimeRangeType = (args) =>{
        setSelectedItem(args.newValue.key);
        setShowValidateMessage(false);
        setShowExceedMaxDateValidateMessage(false);
    };

    return (
        <R.Panel
            id="raMAExtendDisposalTimePanel"
            header={RMResx.RM_MA_EntendDisposalTime}
            size={600}
            status={{ show: isShow }}
            destroy={true}
            onHide={onCancel}
        >
            <div>
                <$g.FormRow label={StringUtil.trimEndColon(RMResx.RM_MA_SelectEntendDisposalTime)} require={true}>
                    <R.Combobox
                        checkedField="checked"
                        textField="value"
                        valueField="key"
                        width={"100%"}
                        hasFilter={false}
                        searchable={false}
                        items={selectorItems}
                        onChange={onSelectDisposalTimeRangeType}
                    />
                </$g.FormRow>
                <$g.FormRow label={StringUtil.trimEndColon(RMResx.RM_MA_EntendDisposition)}>
                    <R.Datepicker
                        selectedDate={customeExtendDate}
                        dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                        hasTimePicker={true}
                        disabled={selectedItem !== ExtendType.Custom}
                        onChange={(args) => setCustomExtendDate(args.newValue)}
                        enableDates={{ start: new Date(), end: maxCustomeExtendDate }}
                    />
                    <$g.ValidationMsg show={showExceedMaxDateValidateMessage}>
                        {RMResx.RM_MA_ExtendDisposalTime_Valid_ExceedMaxDate}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={showValidateMessage}>
                        {RMResx.RM_MA_ExtendDisposalTime_Valid_EarlierThanNow}
                    </$g.ValidationMsg>
                </$g.FormRow>
                <$g.FormRow label={RMResx.RM_MA_Extend_Reason}>
                    <R.Input
                        type='textarea'
                        value={comment}
                        onChange={value => setComment(value)}
                        aria={{ ariaLabel: RMResx.RM_MA_Extend_Reason }}
                    />
                </$g.FormRow>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCancel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onExecuteAction} />
            </>
        </R.Panel>
    );
});

ExtendAction.displayName = "ExtendAction";

export default ExtendAction;