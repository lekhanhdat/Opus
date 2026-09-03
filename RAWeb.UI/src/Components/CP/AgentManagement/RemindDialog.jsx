import Download from "../../Common/Download";
const processesInfo = [
    {
        id: 0,
        processName: RMResx.RM_CP_Agent_RemindDia_Certificate,
        processIntroduce: RMResx.RM_CP_Agent_RemindDia_Certificate_Intro,
        class: 'process-img1',
        actionName: RMResx.RM_CP_Agent_RemindDia_Certificate_Register_Action,
    },
    {
        id: 1,
        processName: RMResx.RM_CP_Agent_RemindDia_ClientID,
        processIntroduce: RMResx.RM_CP_Agent_RemindDia_ClientID_Intro,
        class: 'process-img2',
        actionName: RMResx.RM_CP_Agent_RemindDia_ClientID_Action,
    },
    {
        id: 2,
        processName: RMResx.RM_CP_Agent_Register,
        processIntroduce: RMResx.RM_CP_Agent_Register_Introduce,
        class: 'process-img3'
    },
    {
        id: 3,
        processName: RMResx.RM_CP_Agent_RemindDia_DownloadConfigFile,
        processIntroduce: RMResx.RM_CP_Agent_RemindDia_DownloadConfigFile_Intro,
        class: 'process-img4'
    },
    {
        id: 4,
        processName: RMResx.RM_CP_Agent_RemindDia_DownloadInstaller,
        processIntroduce: RMResx.RM_CP_Agent_RemindDia_DownloadInstaller_Intro,
        class: 'process-img5'
    }
];

export default class RemindDialog extends R.Component {
    idAttr = true;
    componentCreate () {
        this.state = {
            remindDiaShow: false,
            selectedProcess: RM.deepcopy(processesInfo)[0],
            processesInfo: RM.deepcopy(processesInfo)
        };
    }

    componentReceive () {
        this.setState({ remindDiaShow: true });
    }

    hideRemindDialog = () => {
        this.setState({ remindDiaShow: false });
    };


    downloadCertificate = () => {
        this.dispatch('raCPAgentDownloadCertificate', '/api/CPAgentMgmtApi/DownloadCert');
        this.setState({ remindDiaShow: false });
    };

    renderDownloadCertificate () {
        return <Download id="raCPAgentDownloadCertificate"></Download>;
    }

    onClickProcessIcon = (item) => {
        this.setState({
            selectedProcess: item,
        });
    };

    clientIdIntroActClick = () => {
        this.props.actionClick();
        this.setState({ remindDiaShow: false });
    };

    onBack = () => {
        let processesInfo = RM.deepcopy(this.state.processesInfo);
        let selectedProcessIdx = this.state.selectedProcess.id;
        this.setState({ selectedProcess: processesInfo[selectedProcessIdx - 1] });
    };

    onNext = () => {
        let processesInfo = RM.deepcopy(this.state.processesInfo);
        let selectedProcessIdx = this.state.selectedProcess.id;
        this.setState({ selectedProcess: processesInfo[selectedProcessIdx + 1] });
    }
    
    onNextKeyDown = (e) =>{
        if(e.keyCode == "13"){
            this.onNext();
        }           
    }

    onBackKeyDown = (e) =>{
        if(e.keyCode == "13"){
            this.onBack();
        }           
    }

    onSwicthProcessByKey = (e) =>{
        let selectedProcessId = this.state.selectedProcess.id;
        if(e.keyCode == "37" && selectedProcessId > 0){
            this.onBack();
        }
        if(e.keyCode == "39" && selectedProcessId < processesInfo.length - 1){
            this.onNext();
        }
        setTimeout(()=>{
            $(".selected-process").focus();
        },100);
    }

    renderProcess (processesInfo, selectedProcess) {
        return <div className='process-area' role="tablist">
            <div className='process-icon'>
                {
                    processesInfo.map((item, index) => {
                        return <div
                            role="tab"
                            key={index}
                            onKeyDown={this.onSwicthProcessByKey}
                            aria-selected={index == selectedProcess.id}
                            className={index == selectedProcess.id ? (`${item.class} selected-process`) : item.class}
                            onClick={this.onClickProcessIcon.bind(this, item, index)}
                            tabIndex={index == selectedProcess.id ? "0": "-1"}
                            aria-label={item.processName}>       
                        </div>;
                    })
                }
            </div>
            <div className='process-name' tabIndex= {0} >{selectedProcess.processName}</div>
        </div >;
    }

    renderIntroduce (selectedProcess) {
        let isCertIntro = selectedProcess.id == 0;
        let isClientIdIntro = selectedProcess.id == 1;
        let actionName = selectedProcess.actionName;
        let appRegisterURL = this.props.appRegisterURL;
        return <div className='introduce-area' tabIndex={0}>
            {
                isCertIntro &&
                <$g.I18NProvider msg={selectedProcess.processIntroduce}>
                    {isCertIntro && <a className="ra-link-a" href={appRegisterURL} target="blank" tabIndex={0}>{actionName}</a>}
                </$g.I18NProvider>
            }
            {
                !isCertIntro && selectedProcess.processIntroduce
            }
            {
                isClientIdIntro && <div>
                    <a className="ra-link-a" href={selectedProcess.url} onClick={this.clientIdIntroActClick} tabIndex={0}>{actionName}</a>
                </div>
            }
        </div>;
    }

    renderFoot () {
        let processesInfo = this.state.processesInfo;
        let selectedProcess = this.state.selectedProcess;
        let isShowBackBtn = selectedProcess.id != 0;
        let isShowNextBtn = selectedProcess.id != processesInfo.length - 1;
        let isShowLetGoBtn = selectedProcess.id == processesInfo.length - 1;
        let isCertNode = selectedProcess.id == 0;
        return <div className='foot-btn'>
            {isShowBackBtn && <div className='back' onClick={this.onBack} onKeyDown={this.onBackKeyDown} tabIndex={0}>{RMResx.RM_CP_Agent_Remind_Back}</div>}
            {isShowLetGoBtn && <div className='letgo'>
                <R.Button
                    primary={true}
                    classify="theme"
                    width="100"
                    onClick={this.hideRemindDialog}
                    className='btn-style'
                    text={RMResx.RM_CP_Agent_Remind_Letgo} />
            </div>
            }
            {isCertNode && <div className='download'>
                <R.Button
                    primary={true}
                    classify="theme"
                    width="100"
                    onClick={this.downloadCertificate}
                    className='btn-style'
                    text={RMResx.RM_CP_Agent_Remind_DownloadBtn} />
            </div>
            }
            {isShowNextBtn && <div className='next' onClick={this.onNext} onKeyDown={this.onNextKeyDown} tabIndex={0}>{RMResx.RM_CP_Agent_Remind_Next}</div>}
        </div>;
    }

    render () {
        let processesInfo = this.state.processesInfo;
        let selectedProcess = this.state.selectedProcess;
        return <div>
            <R.Dialog
                id="raRemindDialog"
                width={600}
                height={460}
                status={{ show: this.state.remindDiaShow }}
                destroy={true}
                onClose={this.hideRemindDialog}
            >
                <div className="ra-remind-content">
                    {this.renderProcess(processesInfo, selectedProcess)}
                    {this.renderIntroduce(selectedProcess)}
                    {this.renderFoot()}
                </div>
            </R.Dialog>
            {this.renderDownloadCertificate()}
        </div>;
    }
}