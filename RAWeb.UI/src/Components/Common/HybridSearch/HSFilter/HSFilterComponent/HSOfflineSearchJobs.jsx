import React from "react";

export default class HSOfflineSearchJobs extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            offlineSearchJobsList: [],
            selectedOfflineJobOption: {},
            isShowPopup: false,
            watermark: RMResx.RM_JS_Common_None
        };
    }

    componentInit() {

    }

    componentReceive(data) {
        let offlineSearchJobs = data.OfflineJobs;
        this.profileId = data.Id;
        if(offlineSearchJobs.length > 0){
            let defaultOfflineSearchJob = offlineSearchJobs[0];
            defaultOfflineSearchJob.Checked = true;
            if(offlineSearchJobs.length > 0){
                for(let item of offlineSearchJobs){
                    item.StartTime = item.StartTime.split("(UTC")[0];
                }
                this.setState({
                    offlineSearchJobsList: offlineSearchJobs,
                    selectedOfflineJobOption: defaultOfflineSearchJob
                });
            }
            this.props.onSearchByOfflineJob(this.profileId, defaultOfflineSearchJob.JobId);
        }else{
            this.setState({
                offlineSearchJobsList: offlineSearchJobs,
                selectedOfflineJobOption: {}
            });
            this.props.onSearchByOfflineJob();
        }
    }

    onSearch = () =>{
        this.props.onSearchByProfileId(this.profileId);
    }

    onClickOfflineJobOption = (item) => {
        for(let jobItem of this.state.offlineSearchJobsList){
            jobItem.Checked = jobItem.JobId == item.JobId;
        }
        this.setState({
            selectedOfflineJobOption: item,
            isShowPopup: false
        });
        this.props.onSearchByOfflineJob(this.profileId, item.JobId);
    }

    renderPopup(){
        return <div className="hs-search-view-offlinejob-popup">
            <div className="search-view-offlinejob-options" role="button">
                {
                    this.state.offlineSearchJobsList.map((item, key)=>{
                        return <div 
                            key={key}
                            tabIndex={0}
                            className="offline-job-option" 
                            style={{background: item.Checked ? "#E6E7E8" : ""}}  
                            onClick={this.onClickOfflineJobOption.bind(this, item)}
                        >
                            {item.StartTime}
                        </div>;
                    })
                }
            </div>
            <div className="search-view-offlinejob-action" onClick={this.onSearch} tabIndex="0" role="button">
                <span className="fia-search"></span>
                <span className="margin-left-xs">{RMResx.RM_HS_Offline_SearchNow}</span>
            </div>
        </div>;
    }

    render() {
        let selectedStartJobTime = this.state.selectedOfflineJobOption.StartTime;
        let comboboxshellText =  selectedStartJobTime || this.state.watermark;
        let comboboxshellColor = selectedStartJobTime ? "" : "#7d848b";
        return <div id="raHsOfflineSearchJobs" tabIndex="0" role="button" onKeyDown={(e) => e.keyCode === 13 && e.target.click()}>
            <div className="aui-comboboxshell" data-tooltip aria-label={this.state.selectedViewText}>
                <div className="aui-comboboxshell-flex">
                    <div className="aui-comboboxshell-content aui-comboboxshell-ellipsis aui-comboboxshell-center">
                        <div className="hs-selected-offline-combobox">
                            <span className="hs-selected-option-icon fia-calendar"></span>
                            <span className="hs-selected-option-text" style={{color: `${comboboxshellColor}`}}>
                                {comboboxshellText}
                            </span>
                        </div>
                    </div>
                    <div className="aui-comboboxshell-icon-box">
                        <div className="aui-comboboxshell-icon fia-triangle-down"></div>
                    </div>
                </div>
            </div>
            <R.Popup
                of="#raHsOfflineSearchJobs"
                triggerEvent="click"
                status={{ show: this.state.isShowPopup }}
            >
                {this.renderPopup()}
            </R.Popup>
        </div>;
    }
}