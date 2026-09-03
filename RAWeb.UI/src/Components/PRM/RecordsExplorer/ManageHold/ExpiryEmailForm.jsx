import React, { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
import PeoplePicker from "../../../Common/PeoplePicker";
import { convertPeoplePickerData } from "./utils";
import "./index.less";


export const ExpiryEmailForm = forwardRef(({ data }, ref) => {
    const {IsEnabled, ReminderDurationDays, EmailRecipients} = data || {};
    const [enabled, setEnabled] = useState(IsEnabled || false);
    const [reminderDuration, setReminderDuration] = useState(ReminderDurationDays || 10);
    const [recipients, setRecipients] = useState(EmailRecipients || []);
    const [reminderError, setReminderError] = useState("");
    const peoplePickerContainer = useRef(null);

    const getPayload = () => {
        return enabled ?  {
            IsEnabled: enabled,
            ReminderDurationDays: Number(reminderDuration),
            EmailRecipients: convertPeoplePickerData(recipients || []),
        } : null
    }   

    const validate = () => {  
        return $$.verify("allValidation");  
    };

    const setReminderErrorMessage = (message) => {
        if(!!reminderDuration) {
            setReminderError(message);
        }
    };

    useImperativeHandle(ref, () => ({
        getPayload,
        validate,
        setReminderErrorMessage
    }), [enabled, reminderDuration, recipients]); 

    const handleEnabledChange = (value) => {
        setEnabled(value);
    };
    const handleReminderDurationChange = (value) => {
        setReminderDuration(value);
        if (value === "" || value === null || typeof value === "undefined") {
            setReminderError(RMResx.RM_JS_ExpiryEmail_Duration_Validation);
        } else {
            setReminderError("");
        }
    }; 
 
    const handleRecipientsChange = (items) => {
        const uniqueRecipients = Array.from(
            new Map(items.map((item) => [item.UserId, item])).values(),
        );
        setRecipients(uniqueRecipients);   
    };

    useEffect(() => {
        const input = peoplePickerContainer.current?.querySelector(
            ".aui-richcombobox-input input"
        );

        if (input) {
            input.removeAttribute("aria-label");

            input.setAttribute(
                "aria-labelledby",
                "notificationRecipientsTitle",
            );

            input.setAttribute(
                "aria-describedby",
                "notificationRecipientsTitle",
            );
        }
    }, [enabled]);
 
    return (
        <div className="ra-hold-expiry-email-form" id='allValidation' tabIndex={0}>
            <$g.FormRow
                label={RMResx.RM_JS_ExpiryEmail_EnableNotifications}
                tipMsg={RMResx.RM_JS_ExpiryEmail_NotificationTip}
            >
                <R.Switch
                    checked={enabled}
                    onChange={handleEnabledChange}
                />
            </$g.FormRow>

            {enabled && (
                <>
                    <$g.FormRow id='raHoldExpiryEmailReminderDuration' label={RMResx.RM_JS_ExpiryEmail_ReminderDuration} require>
                        <div className="flex align-start gap-xs">
                            <R.Validation>
                                <R.Input
                                    id="raHoldExpiryEmailReminderDurationIpt"
                                    type="number"
                                    value={reminderDuration}
                                    placeholder={RMResx.RM_JS_ExpiryEmail_Duration_Placeholder}
                                    onChange={handleReminderDurationChange}
                                    hasControl
                                    min={1}
                                    max={365}
                                    width={484}
                                    aria={{
                                        'aria-labelledby': 'raHoldExpiryEmailReminderDuration'
                                    }}
                                />
                                <R.ValidationFaker valid={!reminderError} of="#raHoldExpiryEmailReminderDurationIpt" message={reminderError} />
                            </R.Validation>
                            <div className="reminder-unit">{RMResx.RM_JS_ExpiryEmail_Day_ReminderUnit}</div>
                        </div>
                    </$g.FormRow>

                    <$g.FormRow label={RMResx.RM_JS_ExpiryEmail_NotificationRecipients} require>
                        <div id='notificationRecipientsGroup' tabIndex={0} aria-labelledby='notificationRecipientsLabel' aria-describedby='notificationRecipientsTip'>
                            <span id="notificationRecipientsLabel" className="sr-only">{RMResx.RM_JS_ExpiryEmail_NotificationRecipients}</span>
                            <div id='notificationRecipientsTip' className="margin-bottom-s">{RMResx.RM_JS_ExpiryEmail_NotificationRecipients_Tip}</div>
                        </div>
                        <div id='notificationRecipientsTitle' className='strong margin-bottom-xs notification-recipients-title'>
                            {RMResx.RM_JS_ExpiryEmail_UserOrGroupName_Title}
                        </div>
                        <R.Validation element="RichCombobox" require={RMResx.RM_JS_ExpiryEmail_UserOrGroupName_Validation}>
                            <div ref={peoplePickerContainer}>
                                <PeoplePicker
                                    height={78}
                                    width={"100%"}
                                    items={recipients}
                                    selectionChanged={handleRecipientsChange}
                                    searchUsersByPermissionScope
                                />
                            </div>
                        </R.Validation>
                    </$g.FormRow>
                </>
            )}
        </div>
    );
}); 
 