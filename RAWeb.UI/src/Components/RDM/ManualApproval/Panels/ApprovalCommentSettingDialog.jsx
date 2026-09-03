import React, { useEffect, useState } from "react";
import { ApprovalStatus } from "../Constants/ApprovalStatus";
import { useStableCallback } from "../../../Common/Hooks";
import { ConfigOptions } from "../Constants/ConfigOptions";
import Utility from "../Utility";
import { ExtendType, ExtendTypeI18Ns } from "../Constants/index";
import StringUtil from "../../../../Utilities/StringUtil";

const BuildDefaultOptionsSelectItems = (
    latestExtendType,
    latestExtendNumber,
    selectedItems,
    options = [ExtendType.After1Month,ExtendType.After3Month, ExtendType.After6Month, ExtendType.After1Year],   //选择的ExtendType
   
) => {
    const result = [];
    for (const option of options) {
        if(latestExtendType === ExtendType.Month)
        {
            if(latestExtendNumber < 3 )
            {
                if ([ExtendType.After3Month, ExtendType.After6Month,ExtendType.After1Year].includes(option)) {
                    continue;
                }
            }else if(latestExtendNumber >= 3 && latestExtendNumber < 6)
            {
                if ([ExtendType.After6Month,ExtendType.After1Year].includes(option)) {
                    continue;
                }
            }else if(latestExtendNumber >= 6  && latestExtendNumber < 12)
            {
                if ([ExtendType.After1Year].includes(option)) {
                    continue;
                }
            }
        }
        result.push({
            key: option,
            value: ExtendTypeI18Ns.get(option),
            checked: selectedItems.some(item => item === option),
        });
    } 

    result.push({
        key: ExtendType.Custom,
        value: ExtendTypeI18Ns.get(ExtendType.Custom),
        checked: selectedItems.includes(ExtendType.Custom)
    });

    //如果都没有被checked，则选定的最大值，和设置的最大值不符，需要降一个档次。
    if(!result.find(item=>item.checked == true))
    {
        result[result.length - 2].checked = true;
    }

    return result;
};

//最大的时间
const MaxCustomExtendTime = (latestExtendType,latestExtendNumber) => {
    const now = new Date();
    if (latestExtendType === ExtendType.After1Month) {
        const month = now.getMonth() + 1;
        now.setMonth(month);
    }else if(latestExtendType === ExtendType.Month)
    {
        const month = now.getMonth() + latestExtendNumber;
        now.setMonth(month);
    }
    else if(latestExtendType === ExtendType.Year)
    {
        const year = now.getFullYear() + latestExtendNumber;
        now.setFullYear(year);
    }
    now.setHours(now.getHours());
    now.setMinutes(now.getMinutes());
    return now;
};


const ApprovalCommentSettingDialog = ({ show, onHide, onApprove, onReject, action, checkedCommentOption, NeedQuickReason, ApprovalCommentQuickReason, InactiveRejects, CheckQuickReason, onChange, LatestExtendType, LatestExtendNumber, checkedItems, needCustomButton, customButtonNames}) => {
    const [approvalCommentQuickReasons, setApprovalCommentQuickReasons] = useState([]);

    const [approvalComment, setApprovalComment] = useState("");

    const [requiredComment, setRequiredComment] = useState(false);

    const [requiredQuickReason, setRequiredQuickReason] = useState(false);

    //校验时间
    const [showValidateMessage, setShowValidateMessage] = useState(false);

    const [showExceedMaxDateValidateMessage, setShowExceedMaxDateValidateMessage] = useState(false);
    // 用户选择的自定义延期日期的状态变量。如果选择了自定义延期时间，会记录用户选择的具体日期和时间。
    const [customeExtendDate, setCustomExtendDate] = useState(new Date());
    // 存储最大的自定义延期日期的状态变量
    const [maxCustomeExtendDate, setMaxCustomExtendDate] = useState(MaxCustomExtendTime(ExtendType.After1Month,LatestExtendType));
    //页面显示的变量
    const [selectorItems, setSelectorItems] = useState([]);
    //用来存储用户当前选择的延期时间选项的状态变量
    const [selectedItem, setSelectedItem] = useState(ExtendType.After1Month);

    useEffect(() => {
        !show && resetDialog();
        let currentSelectedItems = [ExtendType.After1Month];    //默认是一个月
 
        if(checkedItems)
        {
            if(checkedItems.length > 1)   //多个默认为1个月
            {   
                setSelectorItems(BuildDefaultOptionsSelectItems(LatestExtendType,LatestExtendNumber,currentSelectedItems));
                setSelectedItem(ExtendType.After1Month);
                setCustomExtendDate(new Date());
            }
            else if(checkedItems.length == 1)
            {
                let manualLastExtendType = checkedItems.map(item => item.manualLastExtendType)[0];           
                if(manualLastExtendType === ExtendType.Custom)
                {
                    currentSelectedItems = [ExtendType.Custom];
                    let manualLastCustomeExtendDate = checkedItems.map(item => item.manualLastCustomeExtendDate)[0];
                    setSelectorItems(BuildDefaultOptionsSelectItems(LatestExtendType,LatestExtendNumber,currentSelectedItems));
                    setSelectedItem(manualLastExtendType);
                    setCustomExtendDate(new Date(manualLastCustomeExtendDate));
                  
                }
                else if(manualLastExtendType === ExtendType.None)
                {
                    setSelectorItems(BuildDefaultOptionsSelectItems(LatestExtendType,LatestExtendNumber,currentSelectedItems));
                    setSelectedItem(ExtendType.After1Month);
                    setCustomExtendDate(new Date());
                }
                else 
                {                  
                    currentSelectedItems = [manualLastExtendType];
                    const isSelectorType = BuildDefaultOptionsSelectItems(LatestExtendType,LatestExtendNumber,currentSelectedItems);
                    setSelectorItems(isSelectorType);
                    const checkedItem = isSelectorType.find(item => item.checked == true);
                    const isSelectedItem = currentSelectedItems.includes(checkedItem.key) ? manualLastExtendType  : checkedItem.key;
                    setSelectedItem(isSelectedItem);
                    setCustomExtendDate(new Date());
                }

            }
            else   //select all 情况
            {
                setSelectorItems(BuildDefaultOptionsSelectItems(LatestExtendType,LatestExtendNumber,currentSelectedItems));
                setSelectedItem(ExtendType.After1Month);
                setCustomExtendDate(new Date());
            }
        }
       
        setShowValidateMessage(false);
        setShowExceedMaxDateValidateMessage(false);
        setMaxCustomExtendDate(MaxCustomExtendTime(LatestExtendType,LatestExtendNumber));
    }, [show]);

    useEffect(() => {
        if (ApprovalCommentQuickReason && ApprovalCommentQuickReason.length) {
            setApprovalCommentQuickReasons(ApprovalCommentQuickReason);
        }
    }, [ApprovalCommentQuickReason])

    useEffect(() => {
        if (InactiveRejects && InactiveRejects.length) {
            setApprovalCommentQuickReasons(prev => prev.filter((_, index) => !InactiveRejects[index]));
        }
    }, [InactiveRejects])

    const resetDialog = () => {
        setApprovalComment("");
        setRequiredComment(false);
        setRequiredQuickReason(false);
    };

    const onApprovalCommentChanged = (args) =>{
        if(args.trim() !== ""){        
            setRequiredComment(false);
        }
        setApprovalComment(args);
    };

    const PreCheckIsRequestComment = useStableCallback(() => {
        let isRequired = false;
        if(approvalComment.trim() === ""){
            if(checkedCommentOption === ConfigOptions.BothApproveAndReject){
                isRequired = true;
            }
            else if(checkedCommentOption === ConfigOptions.ApproveOnly  && action === ApprovalStatus.Approved){
                isRequired = true;
            }
            else if(checkedCommentOption === ConfigOptions.RejectOnly && action === ApprovalStatus.Rejected){
                isRequired = true;
            }
            setRequiredComment(isRequired);
        }

        if(NeedQuickReason && CheckQuickReason === "" && action === ApprovalStatus.Rejected ){
            isRequired = true;
            setRequiredQuickReason(true);
        }

        return isRequired;
    });

    const onApproveOrReject = useStableCallback(() =>{

        if(PreCheckIsRequestComment()){
            return false;
        }
        
        if(action === ApprovalStatus.Approved){
            onApprove(approvalComment);
            onHide();
            return;
        }

        if (selectedItem === ExtendType.Custom) {
            if(new Date(customeExtendDate).getTime() <= new Date().getTime()){
                setShowValidateMessage(true);
                return false;
            }
            if(new Date(customeExtendDate).getTime() >= MaxCustomExtendTime(LatestExtendType,LatestExtendNumber).getTime()){
                setShowExceedMaxDateValidateMessage(true);
                return false;
            }
        }
        
        onReject(approvalComment,selectedItem,customeExtendDate);
        onHide();
    });

    
    const onChangeChecked = (args) => {
        onChange(args.newValue.value);
        setRequiredQuickReason(false);
    };

    const onSelectDisposalTimeRangeType = (args) =>{
        setSelectedItem(args.newValue.key);
        setShowValidateMessage(false);
        setShowExceedMaxDateValidateMessage(false);
    };
    
    return (
        <R.Dialog
            id="ManualConfigDialog"
            header={
                action === ApprovalStatus.Approved
                    ? RMResx.RM_MA_Approve
                    : RMResx.RM_MA_Reject
            }
            width={480}
            height={ action === ApprovalStatus.Approved ?  330 : (NeedQuickReason ?  580 : 500 ) }
            status={{ show: show }}
            struct={{ foot: true }}
            onHide={onHide}
            destroy={true}
        >
            <div className="ra-comment-dialog">
                {NeedQuickReason && action === ApprovalStatus.Rejected && <div className="ra-comment-dialog-term">
                    <div className="ra-comment-dialog-term-title require" tabIndex="0">
                        <$g.I18NProvider msg={RMResx.RM_MA_ApprovalComment_TermTitle} />
                    </div>
                    <div>
                        <R.Combobox
                            id="option"
                            textField="text"
                            valueField="value"
                            checkedField="checked"
                            tooltipField="tooltip"
                            width="100%"
                            items={Utility.setCheckedOption(Utility.convertToComboxItems(approvalCommentQuickReasons), CheckQuickReason)}
                            onChange={onChangeChecked}
                            aria={{
                                ariaLabelledby: "ariaUrl",
                                ariaRequired: true
                            }}
                            searchable={false}
                        />
                    </div>
                    <$g.ValidationMsg show={requiredQuickReason}>
                        {RMResx.RM_MA_ApprovalComment_QuickReasonRequire}
                    </$g.ValidationMsg>
                </div>}
                <div className="ra-comment-dialog-title" tabIndex="0">
                    <$g.I18NProvider
                        msg={
                            action === ApprovalStatus.Approved
                                ? RMResx.RM_MA_ApprovalComment_ApproveWhy
                                : RMResx.RM_MA_ApprovalComment_RejectWhy
                        }
                    />
                </div>
                <div>
                    <R.Input
                        width={416}
                        height={80}
                        type="textarea"
                        value={approvalComment}
                        onChange={onApprovalCommentChanged}
                    />
                    <$g.ValidationMsg show={requiredComment}>
                        {RMResx.RM_MA_ApprovalComment_InputRequire}
                    </$g.ValidationMsg>
                </div>
                <br/>
                {
                    action === ApprovalStatus.Rejected && (
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
                            <$g.FormRow label={StringUtil.trimEndColon(RMResx.RM_MA_NextReviewEntendTime)}>
                                <R.Datepicker
                                    selectedDate={customeExtendDate}
                                    dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                                    hasTimePicker={true}
                                    disabled={selectedItem !== ExtendType.Custom}
                                    onChange={(args) => setCustomExtendDate(args.newValue)}
                                    enableDates={{start: new Date(), end: maxCustomeExtendDate}}
                                />
                                <$g.ValidationMsg show={showExceedMaxDateValidateMessage}>
                                    {RMResx.RM_MA_ExtendDisposalTime_Valid_ExceedMaxDate}
                                </$g.ValidationMsg>
                                <$g.ValidationMsg show={showValidateMessage}>
                                    {RMResx.RM_MA_ExtendDisposalTime_Valid_EarlierThanNow}
                                </$g.ValidationMsg>
                            </$g.FormRow>
                        </div>
                    )
                }
            </div>
            {show && <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={action === ApprovalStatus.Approved
                        ? Utility.getCustomButtonNames(needCustomButton, customButtonNames).approveButtonName
                        : Utility.getCustomButtonNames(needCustomButton, customButtonNames).rejectButtonName}
                    onClick={onApproveOrReject}
                />
            </>}
        </R.Dialog>
    );
};

export default ApprovalCommentSettingDialog;
