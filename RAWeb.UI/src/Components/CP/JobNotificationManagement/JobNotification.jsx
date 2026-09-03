import React, { useEffect, useState } from "react";
import _ from "lodash";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import JobNotificationTable from "./JobNotificationTable";
import JobNotificationPanel from "./JobNotificationPanel";
import "./index.less";
import { showToast } from "../../../Utilities/CommonUtil";
import { JobType } from "./Constants/index";

const GetJobTypes = () => {
    let jobTypes = [ JobType.SyncNode ];
    if(RM.gData.hasDiscoveryLicense | RM.gData.hasDiscoverySalesforceLicense | RM.gData.hasDiscoveryGoogleLicense){
        jobTypes.push(JobType.Discovery);
    }
    if(RM.gData.enableRecordsArchiver){
        if(RM.gData.hasArchiverLicense){
            jobTypes.push(JobType.RMArchiverBackup, JobType.SOPreScan, JobType.ArchiverRestore);
            
            if(RM.gData.hasRecordsLicense){
                jobTypes.push(JobType.EnforceRetention, JobType.DataSync, JobType.EnforceRuleAction, JobType.DashboardData, JobType.TermSync);
            }
        }
    }else{
        if(RM.gData.hasRecordsLicense){
            jobTypes.push(JobType.EnforceRetention, JobType.DataSync, JobType.EnforceRuleAction, JobType.DashboardData, JobType.TermSync);
        }
    }
    return jobTypes;
};

const JobNotification = (props) => {

    const [showPanel, setShowPanel] = useState(false);

    const [profiles, setProfiles] = useState([]);

    const [profile, setProfile] = useState(null);

    const [reloadRefreshKey, setReloadRefreshKey] = useState(Math.random());

    const [isShowDeleteButton, setIsShowDeleteButton] = useState(false);

    const [checkedItemIds ,setCheckedItemIds] = useState([]);
    
    const jobTypes = GetJobTypes();

    useEffect(() => {
        onLoadProfiles();
        setIsShowDeleteButton(false);
        setCheckedItemIds([]);
    }, [reloadRefreshKey]);

    const onLoadProfiles = async () => {
        $$.loading(true);
        let result = await fetchUtility({  url: "/api/CPJobNotificationApi/GetAllProfiles"});
        setProfiles(result);
        $$.loading(false);
    };

    const onHide = () => {
        setShowPanel(false);
    };

    const onSave = async (isCreate, data) => {
        if(isCreate){
            let result = await fetchUtility({  url: "/api/CPJobNotificationApi/CreateProfile", data : data});
            if(result.MessageType === 0){
                setShowPanel(false);
            }
            return result;
        }
        data.profileId = profile.profileId;
        let result = await fetchUtility({  url: "/api/CPJobNotificationApi/EditProfile", data : data});
        if(result.MessageType === 0){
            setShowPanel(false);
        }
        return result;
    };

    const onReload = () => {
        setReloadRefreshKey(Math.random());
    };

    const onCreateClick = () => {
        setProfile(null);
        if(profiles.length >= 10){
            showToast.warn(RMResx.RM_JS_JN_CreateLimition);
            return false;
        }
        let result = onCheckAvalibalJobType();
        if(result){
            showToast.warn(RMResx.RM_JS_JN_JobTypeLimition);
            return false;
        }
        setShowPanel(true);
    };

    const onCheckAvalibalJobType = ()=> {
        let jobInfos = profiles.flatMap(profile => profile.profileJobInfos);
        let usedTypes = jobInfos.flatMap(jobInfo => jobInfo.jobType);
        let avalibalJobTypes = jobTypes.filter(jobType => !usedTypes.includes(jobType));
        return avalibalJobTypes.length === 0;
    };

    const onCellClick = async (args) => {
        $$.loading(true);
        let result = await fetchUtility({  url: "/api/CPJobNotificationApi/GetProfile", data : args.profileId});
        setProfile(result);
        setShowPanel(true);
        $$.loading(false);
    };  

    const onChangeChecked = (args) => {
        const willCheckedItems = [];
        args.forEach((value) => {
            if(value.checked){
                willCheckedItems.push(value.profileId);
            }
        });
        setCheckedItemIds(willCheckedItems);
        if(willCheckedItems.length > 0){
            setIsShowDeleteButton(true);
        }else{
            setIsShowDeleteButton(false);
        }
    };

    const onDeleteClick = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_JS_JN_DeleteMsg}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: onDeleteCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: onDeleteSureClick }
            ]
        };
        $$.messagedialog(true, args);
    };

    const onDeleteSureClick = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        let result = await fetchUtility({  url: "/api/CPJobNotificationApi/DeleteProfile", data : checkedItemIds});
        if(result.MessageType === 0){
            showToast.success(RMResx.RM_JS_JN_DeleteSuccessful);
            $$.loading(false);
            onReload();
            return;
        }        
        showToast.error(RMResx.RM_JS_JN_DeleteFailed);
        $$.loading(false);
    };

    const onDeleteCancelClick = () => {
        $$.messagedialog(false);
    };

    return (
        <div className="reco-job-notification">
            <div className="reco-job-notification-header">
                <$g.SiteMap  data={[SiteMapLinks.CP, SiteMapLinks.CP_JobNotification]} />
            </div>
            <div className="reco-job-notification-content">
                <div className="reco-job-notification-action">
                    <div className="reco-job-notification-actions gap-s">
                        <R.Button
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Create}
                            onClick={onCreateClick}
                        />
                        {isShowDeleteButton && 
                            <R.Button
                                text={RMResx.RM_JS_Common_Delete}
                                icon="fia-delete"
                                onClick={onDeleteClick}
                            />
                        }
                    </div>
                    <div className="reco-manual-review-actions-desc">
                        {
                            RMResx.RM_Common_SelectTableItemsCounter.format(checkedItemIds.length, profiles.length)
                        }
                    </div>
                </div>
                <div className="reco-job-notification-table">
                    <JobNotificationTable
                        items={profiles}
                        onCellClick={onCellClick}
                        onChangeChecked={onChangeChecked}
                    />
                </div>
                <div className="reco-job-notification-panel">
                    <JobNotificationPanel
                        show={showPanel}
                        onHide={onHide}
                        profile={profile}
                        profiles={profiles}
                        onSave={onSave}
                        onReload={onReload}
                        jobTypes={jobTypes}
                    />
                </div>
            </div>
        </div>
    );
};

export default JobNotification;