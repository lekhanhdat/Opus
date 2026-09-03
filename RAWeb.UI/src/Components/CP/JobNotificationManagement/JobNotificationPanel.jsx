import React, { useEffect, useState } from "react";
import _ from "lodash";
import { IntervalType, IntervalTypeI18N, JobType, JobTypes, WeeklyTypes, WeeklyTypesI18N,
    RMArchiverBackupStatus, SOPreScanStatus, EnforceRetentionStatus, ArchiverRestoreStatus, DiscoveryStatus,
    DataSyncStatus, EnforceRuleActionStatus, SyncNodeStatus, DashboardDataStatus, TermSyncStatus } from "./Constants/index";
import JobNotificationJobInfo from './JobNotificationJobInfo';
import { useStableCallback } from "../../../Components/Common/Hooks";
import { showToast } from "../../../Utilities/CommonUtil";

const RadioOptions = [
    { text: IntervalTypeI18N.get(IntervalType.Daily), value: IntervalType.Daily, checked: true },
    { text: IntervalTypeI18N.get(IntervalType.Weekly), value: IntervalType.Weekly, checked: false },
];

const GetJobStatus = () => {
    return new Map([
        [JobType.RMArchiverBackup, RMArchiverBackupStatus],
        [JobType.SOPreScan, SOPreScanStatus],
        [JobType.EnforceRetention, EnforceRetentionStatus],
        [JobType.ArchiverRestore, ArchiverRestoreStatus],
        [JobType.Discovery, DiscoveryStatus],
        [JobType.DataSync, DataSyncStatus],
        [JobType.EnforceRuleAction, EnforceRuleActionStatus],
        [JobType.SyncNode, SyncNodeStatus],
        [JobType.DashboardData, DashboardDataStatus],
        [JobType.TermSync, TermSyncStatus],
    ]);
};

const BuildWeelyItems = (
    selectedItem = WeeklyTypes.Monday,
    options = [WeeklyTypes.Monday, WeeklyTypes.Tuesday, WeeklyTypes.Wednesday, WeeklyTypes.Thursday, WeeklyTypes.Friday, WeeklyTypes.Saturday, WeeklyTypes.Sunday]
) => {
    const result = [];
    for (const option of options) {
        const optionValue = WeeklyTypesI18N.get(option);
        result.push({
            key: option,
            value: optionValue,
            checked: selectedItem === option,
        });
    }
    return result;
};

const JobNotificationPanel = (
    { show, onHide, profile, profiles, onSave, onReload, jobTypes }) => {

    const [radioOptions, setRadioOptions] = new useState(RadioOptions);

    const [jobStatus, setJobStatus] = new useState(GetJobStatus);
    
    const [intervalType, setIntervalType] = new useState(IntervalType.Daily);

    const [seletedWeeklyType, setSeletedWeeklyType] = new useState(WeeklyTypes.Monday);

    const [weeklyType, setWeeklyType] = new useState(BuildWeelyItems);

    const [profileName, setProfileName] = new useState("");

    const [profileDes, setProfileDes] = new useState("");

    const [profileUseList, setProfileUseList] = new useState([]);

    const [requiredName, setRequiredName] = useState(false);

    const [requiredNameTooLong, setRequiredNameTooLong] = useState(false);

    const [requiredDesTooLong, setRequiredDesTooLong] = useState(false);

    const [requiredUsers, setRequiredUsers] = useState(false);

    const [requiredJobStatus, setRequiredJobStatus] = useState(false);

    const [busyJobTypes, setBusyJobTypes] = useState([]);

    useEffect(() => {
        if(!_.isNil(profile)){
            setProfileName(profile.profileName);
            setProfileDes(profile.profileDes);
            setUserList();
            setIntervalOptions();
            setJobStatusInfo();
        }else{
            resetProfile();
        }
        !show && setRequiredName(false);  
        !show && setRequiredUsers(false);
        !show && setRequiredJobStatus(false);
        !show && setRequiredNameTooLong(false);
        !show && setRequiredDesTooLong(false);
        show && setAvalibalJobTypes();
    }, [show]);

    const setAvalibalJobTypes = () => {
        
        let jobInfos = profiles.flatMap(profile => profile.profileJobInfos);

        let usedTypes = jobInfos.flatMap(jobInfo => jobInfo.jobType);

        let avalibalJobTypes = jobTypes.filter(jobType => !usedTypes.includes(jobType));

        if (profile && profile.profileJobInfos) {
            let currentTypes = profile.profileJobInfos.flatMap(jobInfo => jobInfo.jobType);
            avalibalJobTypes.push(...currentTypes);
        }

        setBusyJobTypes(avalibalJobTypes);
    };

    const resetProfile = () => {
        const clonedJobStatus = _.cloneDeep(jobStatus);
        const clonedJobTypes = _.cloneDeep(jobTypes);
        clonedJobTypes.forEach((jobType) => {
            clonedJobStatus.get(jobType).forEach((value) => {
                value.checked = false;
            });
        });

        const intervalOptions = _.cloneDeep(radioOptions);
        intervalOptions.forEach((option) => {
            if(option.value === IntervalType.Daily){
                option.checked = true;
            }else{
                option.checked = false;
            }
        });

        setJobStatus(clonedJobStatus);
        setRadioOptions(intervalOptions);
        setIntervalType(IntervalType.Daily);
        setSeletedWeeklyType(WeeklyTypes.Monday);
        setWeeklyType(BuildWeelyItems);
        setProfileUseList([]);
        setProfileName("");
        setProfileDes("");
        setRequiredName(false);  
        setRequiredUsers(false);
        setRequiredJobStatus(false);  
    };

    const setJobStatusInfo = () => {
        if(_.isNil(profile)){
            return;
        }
        const clonedJobStatus = _.cloneDeep(jobStatus);
        const clonedJobTypes = _.cloneDeep(jobTypes);
        clonedJobTypes.forEach((jobType) => {
            clonedJobStatus.get(jobType).forEach((value) => {
                value.checked = false;
            });
            profile.profileJobInfos.forEach((item) => {
                if(item.jobType === jobType){
                    let selectedJobStatus = item.jobStatuses;
                    clonedJobStatus.get(jobType).map((value) => {
                        value.checked = selectedJobStatus.includes(value.value);
                        return value;
                    });
                }
            });
        });
        setJobStatus(clonedJobStatus);
    };

    const setIntervalOptions = () => {
        if(_.isNil(profile)){
            return;
        }

        let intervalOptions = _.cloneDeep(radioOptions);
        intervalOptions.map((item)=> {
            if(item.value === profile.profileInterval.intervalType){
                item.checked = true;
                return item;
            }else{
                item.checked = false;
                return item;
            }
        });
        setRadioOptions(intervalOptions);
        if(profile.profileInterval.intervalType === IntervalType.Weekly){
            setIntervalType(IntervalType.Weekly);
            setWeeklyType(BuildWeelyItems(profile.profileInterval.weeklyType));
            return;
        }
        setIntervalType(IntervalType.Daily);
        setWeeklyType(BuildWeelyItems(WeeklyTypes.Monday));
    };

    const setUserList = () => {
        if(_.isNil(profile)){
            return;
        }

        let newUsers = [];
        if (profile.profileEmailReceivers) {
            const clonedEmailReceivers = _.cloneDeep(profile.profileEmailReceivers);
            clonedEmailReceivers.map(user => {
                newUsers.push({
                    name: user.DisplayName,
                    value: user.UserId,
                    disabled: false,
                    tooltip: user.UserPrincipalName,
                    readonly: false,
                    invalid: false,
                    conflict: false,
                    data: user,
                });
            });
            setProfileUseList(newUsers);
        }
    };

    const perCheckProfile = (e) =>{
        let result = true;
        if(profileName === ""){
            setRequiredName(true);
            result = false;
        }

        if(profileName.length >= 250){
            setRequiredNameTooLong(true);
            result = false;
        }

        if(profileDes.length >= 250){
            setRequiredDesTooLong(true);
            result = false;
        }

        if(!$$.verify(e.target)){
            result = false;
        }

        const profileJobInfo = preCheckJobStatus();
        if(profileJobInfo && !profileJobInfo.hasSelectedStatus){
            setRequiredJobStatus(true);
            result = false;
        }
        
        return {result : result, profileJobInfo : profileJobInfo.profileJobInfo};
    };

    const preCheckJobStatus = () => {
        const clonedJobTypes = _.cloneDeep(jobTypes);
        let hasSelectedStatus = false;
        const profileJobInfo = clonedJobTypes.reduce((acc, item) => {
            const statuses = jobStatus.get(item) || [];
            const selectedStatus = statuses
                .filter(status => status.checked)
                .map(status => status.value);
            
            if (selectedStatus.length > 0) {
                hasSelectedStatus = true;
                acc.push({ jobType: item, jobStatuses: selectedStatus });
            }
            
            return acc;
        }, []);
        
        if(!hasSelectedStatus){
            return [];
        }

        return { hasSelectedStatus : hasSelectedStatus, profileJobInfo : profileJobInfo};
    };

    const onSaveProfile = useStableCallback(async (e) =>{
        
        const checkResult = perCheckProfile(e);
        if(checkResult && !checkResult.result){
            return false;
        }

        const profileJobInfo = checkResult.profileJobInfo;
        
        if(profileJobInfo && profileJobInfo.length === 0){
            setRequiredJobStatus(true);
            return false;
        }
        
        let profileUserList = [];
        profileUseList.forEach((user) => profileUserList.push(user.data));

        let data = {
            profileName: profileName,
            profileDes: profileDes,
            ProfileEmailReceivers: profileUserList,
            profileInterval: {
                intervalType: intervalType,
                weeklyType: seletedWeeklyType,
            },
            profileJobInfos: profileJobInfo,
        };
        
        const isCreate = _.isNil(profile);
        $$.loading(true);
        let result = await onSave(isCreate ,data);
        $$.loading(false);
        if(result){
            if(result.MessageType !== 0){
                showToast.error(result.ErrorMessage);
                return false;
            }
            showToast.success( isCreate ? RMResx.RM_JS_JN_CreateSuccessful : RMResx.RM_JS_JN_EditSuccessful);
            onReload();
        }
    });

    const onSearch = (args) => {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = _.cloneDeep(res.Users);
                let newUsers = [];
                users.forEach(user => {
                    newUsers.push({
                        name: user.DisplayName,
                        value: user.UserId,
                        disabled: false,
                        tooltip: user.UserPrincipalName,
                        readonly: false,
                        invalid: false,
                        conflict: false,
                        data: user,
                    });
                });
                return newUsers;
            }).catch((e) => {

            });
        }
    };

    const onProfileNameChanged = useStableCallback((args) => {
        setRequiredName(false);
        setRequiredNameTooLong(false);
        setProfileName(args);
    });

    const onProfileDesChanged = useStableCallback((args) => {
        setRequiredDesTooLong(false);
        setProfileDes(args);
    });

    const onPorfileUseListChanged = useStableCallback((args) => {
        if(args.newValue.length === 0){
            setRequiredUsers(true);
        }else{
            setRequiredUsers(false);
        }
        setProfileUseList(args.newValue);
    });

    const onProfileIntervalChanged = useStableCallback((args) => {
        setIntervalType(args);        
        setWeeklyType(BuildWeelyItems(WeeklyTypes.Monday));
    });

    const onProfileWeeklyChanged = useStableCallback((args) => {
        setSeletedWeeklyType(args.newValue.key);
        setWeeklyType(BuildWeelyItems(args.newValue.key));
    });

    const onProfileJobInfoChanged = (jobType, value) => {
        setRequiredJobStatus(false);
        const clonedJobStatus = _.cloneDeep(jobStatus);
        const clonedJobTypes = _.cloneDeep(jobTypes);
        clonedJobTypes.forEach((type) => {
            if(type === jobType){
                let JobStatus = clonedJobStatus.get(jobType);
                if (JobStatus) {
                    clonedJobStatus.get(jobType).map((status) => {
                        status.checked = value.includes(status.value);
                        return status;
                    });
                }
            }
        });
        setJobStatus(clonedJobStatus);
    };

    const renderNameInput = () => {
        return <div className="reco-job-notification-create-module">
            <div className="reco-job-notification-create-title require">
                <$g.I18NProvider msg={RMResx.RM_JS_JN_Name} />
            </div>
            <R.Input
                key={Math.random()}
                type="text"
                min={1}
                width={"100%"}
                value={profileName}
                hasControl
                onChange={onProfileNameChanged}
                aria={{ ariaLabel: RMResx.RM_JS_JN_Name }}
            />
            <$g.ValidationMsg show={requiredName}>
                {RMResx.RM_MA_ApprovalComment_TermInputRequire}
            </$g.ValidationMsg>                            
            <$g.ValidationMsg show={requiredNameTooLong}>
                {RMResx.RM_RC_DueDisposal_ProfileNameTooLong}
            </$g.ValidationMsg>
        </div>;
    };

    const renderDescritionInput = () => {
        return <div className="reco-job-notification-create-module">
            <div className="reco-job-notification-create-title">
                <$g.I18NProvider msg={RMResx.RM_JS_JN_Description} />
            </div>
            <R.Input
                key={Math.random()}
                type="textarea"
                min={1}
                width={"100%"}
                value={profileDes}
                hasControl
                onChange={onProfileDesChanged}
                aria={{ ariaLabel: RMResx.RM_JS_JN_Description }}
            />                            
            <$g.ValidationMsg show={requiredDesTooLong}>
                {RMResx.RM_RC_DueDisposal_DescriptionTooLong}
            </$g.ValidationMsg>
        </div>;
    };

    const renderEmailReceiver = () => {
        return <div className="reco-job-notification-create-module">
            <div className="reco-job-notification-create-title require">
                <span>
                    <$g.I18NProvider msg={RMResx.RM_JS_JN_Receiver} />
                </span>
            </div>
            <R.Validation
                element="RichCombobox"
                require={RMResx.RM_JS_CP_AM_Owner_Require} >
                <R.RichCombobox
                    asyncSearch
                    id={"raCrmUserManualApproval"}
                    height={80}
                    value={profileUseList}
                    searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                    disabled={false}
                    textField="name"
                    valueField="value"
                    template="profile"
                    itemTemplate="profile"
                    checkedField="checked"
                    tooltipField="tooltip"
                    disabledField="disabled"
                    readonlyField="readonly"
                    invalidField="invalid"
                    aria={{ ariaLabel: RMResx.RM_JS_JN_Receiver }}
                    groupField={null}
                    matchFields={{ 'name': false }}
                    searchable={true}
                    singleMode={false}
                    silence={false}
                    excludeChecked={true}
                    doLoad={onSearch}
                    onChange={onPorfileUseListChanged}
                />
                <R.ValidationMessage/>
            </R.Validation>
        </div>;
    };

    const renderInterval = () => {
        return <div className="reco-job-notification-create-module">
            <div className="reco-job-notification-interval">
                <div className="reco-job-notification-interval-title require">
                    <span id="ariaRadioInterval">
                        <$g.I18NProvider msg={RMResx.RM_JS_JN_Interval} />
                    </span>
                </div>
                <$g.Popover>{RMResx.RM_JS_JN_Interval_Description}</$g.Popover>
            </div>
            <R.Radio.Group 
                block={true}
                name="radioInterval"
                items={radioOptions}
                onChange={onProfileIntervalChanged}
                aria="#ariaRadioInterval"
            />
            {intervalType === IntervalType.Weekly && <div className="reco-job-notification-create-weekly">
                <div id="ariaEvery" className="reco-job-notification-create-weekly-title">
                    <$g.I18NProvider msg={RMResx.RM_JS_JN_Every} />
                </div>
                <div className="reco-job-notification-create-weekly-options">
                    <R.Combobox
                        checkedField="checked"
                        textField="value"
                        valueField="key"
                        width={"100%"}
                        hasFilter={false}
                        searchable={false}
                        items={weeklyType}
                        onChange={onProfileWeeklyChanged}
                        aria="#ariaEvery"
                    />
                </div>
            </div>}
        </div>;
    };

    const renderJobTypeAndStatues = () => {
        return <div className="reco-job-notification-create-jobType">
            <div className="reco-job-notification-create-title require" tabIndex="0">
                <$g.I18NProvider msg={RMResx.RM_JS_JN_JobStatus} />
            </div>
            {
                jobTypes.map((value, index) => {
                    return <JobNotificationJobInfo 
                        key={index}
                        jobType={value}
                        options={jobStatus.get(value)}
                        onChange={onProfileJobInfoChanged}
                        disabled={!busyJobTypes.includes(value)}
                    />;
                })
            }            
            <$g.ValidationMsg show={requiredJobStatus}>
                {RMResx.RM_MA_ApprovalComment_TermInputRequire}
            </$g.ValidationMsg>
        </div>;
    };

    return (
        <R.Panel
            header={_.isNil(profile) ? RMResx.RM_JS_JN_Panel_Create : RMResx.RM_JS_JN_Panel_Edit}
            size={670}
            status={{ show : show }}
            destroy={true}
            onHide={onHide}
        >
            <div className="reco-job-notification-create">
                {renderNameInput()}
                {renderDescritionInput()}
                {renderEmailReceiver()}
                {renderInterval()}
                {renderJobTypeAndStatues()}
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={onHide}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSaveProfile}
                />
            </>
        </R.Panel>
    );
};

export default JobNotificationPanel;