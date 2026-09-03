import { useLayoutEffect, useState } from "react";
import useEvent from "../../Common/Util/CommonHooks/UseEvent";

export default function ApiKeyForm({item, showPanel, onHidePanel}) {
    const dateTimeFormat = RM.TimeUtil.getGlobalAuiFormat();
    const [keyName, setKeyName] = useState(item.Name || "");
    const [keyOperator, setKeyOperator] = useState(item.OperatorLoginName || "");
    const [keyExpired, setKeyExpired] = useState(item.Expired ? new Date(item.Expired) : "");
    const [invalidInfo, setInvalidInfo] = useState({});

    useLayoutEffect(initForm, [item]);

    const isNewKeyForm = !item.Id;
    const panelTitle = isNewKeyForm ? RMResx.RM_JS_CP_CSDAK_Title_AddKey : RMResx.RM_JS_CP_CSDAK_Title_EditKey;


    function initForm() {
        setKeyName(item.Name || "");
        setKeyOperator(item.OperatorLoginName || "");
        setKeyExpired(item.Expired ? new Date(item.Expired) : "");
        setInvalidInfo({});
    }

    let onSaveClick = useEvent(() => {
        let invalidInfo = {};
        if (!keyName || !keyName.trim()) {
            invalidInfo.invalidKeyName = true;
            invalidInfo.invalidKeyNameMsg = RMResx.RM_Common_FillOut;
        } else if (keyName.length > 255) {
            invalidInfo.invalidKeyName = true;
            invalidInfo.invalidKeyNameMsg = RMResx.RM_CP_CSDAK_NameTooLong;
        }
        if (!keyOperator || !keyOperator.trim()) {
            invalidInfo.invalidKeyOperator = true;
            invalidInfo.invalidKeyOperatorMsg = RMResx.RM_Common_FillOut;
        } else if (keyOperator.length > 255) {
            invalidInfo.invalidKeyOperator = true;
            invalidInfo.invalidKeyOperatorMsg = RMResx.RM_CP_CSDAK_OperatorTooLong;
        }
        if (!keyExpired) {
            invalidInfo.invalidKeyExpired = true;
            invalidInfo.invalidKeyExpiredMsg = RMResx.RM_Common_FillOut;
        }
        if(invalidInfo.invalidKeyName || invalidInfo.invalidKeyOperator || invalidInfo.invalidKeyExpired) {
            setInvalidInfo(invalidInfo);
            return false;
        }

        let param = {
            Name: keyName,
            OperatorLoginName: keyOperator,
            Expired: RM.TimeUtil.toISOString(keyExpired)
        };
        let url = "/api/CPApi/AddCSDKey";
        if (item.Id) {
            param.id = item.Id;
            url = "/api/CPApi/EditCSDKey";
        }
        $$.loading(true);
        let option = {
            data: param,
            url: url,
            method: "POST"
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res == "true") {
                onHidePanel(true, isNewKeyForm);
            } else if (res == "duplicateKeyName") {
                invalidInfo.invalidKeyName = true;
                invalidInfo.invalidKeyNameMsg = RMResx.RM_CP_CSDAK_NameDuplicate;
                setInvalidInfo(invalidInfo);
            } else {
                onHidePanel(false, isNewKeyForm);
            }
        });
        return false;
    });

    function onHideFormPanel() {
        onHidePanel(false);
    }

    function onChangeKeyName(value) {
        let inputValue = value.trim();
        setInvalidInfo(Object.assign(invalidInfo, {invalidKeyName: false}));
        setKeyName(inputValue);
    }

    function onChangeKeyOperator(value) {
        let inputValue = value.trim();
        setInvalidInfo(Object.assign(invalidInfo, {invalidKeyOperator: false}));
        setKeyOperator(inputValue);
    }

    function onChangeExpiredTime(value) {
        setInvalidInfo(Object.assign(invalidInfo, {invalidKeyExpired: false}));
        setKeyExpired(value.newValue);
    }

    function getFormPanelBtns() {
        return <>
            <R.Button
                slot="buttons"
                text={RMResx.RM_JS_Common_Cancel}
                onClick={onHideFormPanel}
            />
            <R.Button
                slot="buttons"
                primary
                classify="theme"
                text={RMResx.RM_JS_Common_Save}
                onClick={onSaveClick}
            />
        </>
    }

    return <R.Panel
        header={panelTitle}
        size={600}
        status={{ show: showPanel }}
        onHide={onHideFormPanel}
        destroy={true}
    >
        <div id='csdApiKeyForm'>
            <$g.FormRow label={RMResx.RM_JS_CP_CSDAK_KeyName} require={true}>
                <R.Input
                    type="text"
                    width={500}
                    value={keyName}
                    onChange={onChangeKeyName}
                    placeholder=""
                    aria={{ ariaLabel: RMResx.RM_JS_CP_CSDAK_KeyName }}
                />
                <$g.ValidationMsg show={invalidInfo.invalidKeyName}>
                    {invalidInfo.invalidKeyNameMsg}
                </$g.ValidationMsg>
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_CP_CSDAK_KeyOperator} require={true}>
                <R.Input
                    type="text"
                    width={500}
                    value={keyOperator}
                    onChange={onChangeKeyOperator}
                    placeholder=""
                    aria={{ ariaLabel: RMResx.RM_JS_CP_CSDAK_KeyOperator }}
                />
                <$g.ValidationMsg show={invalidInfo.invalidKeyOperator}>
                    {invalidInfo.invalidKeyOperatorMsg}
                </$g.ValidationMsg>
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_CP_CSDAK_KeyExpired} require={true}>
                <R.Datepicker
                    width={500}
                    dateTimeFormat={dateTimeFormat}
                    selectedDate={keyExpired}
                    hasTimePicker={true}
                    onChange={onChangeExpiredTime} />
                <$g.ValidationMsg show={invalidInfo.invalidKeyExpired}>
                    {invalidInfo.invalidKeyExpiredMsg}
                </$g.ValidationMsg>
            </$g.FormRow>

        </div>
        {getFormPanelBtns()}
    </R.Panel>;
}