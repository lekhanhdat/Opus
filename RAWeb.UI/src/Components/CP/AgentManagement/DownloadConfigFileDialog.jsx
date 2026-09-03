import Download from "../../Common/Download";
export default class DownloadConfigFileDialog extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            downloadConfigFileDiaShow: false,
            downloadConfigFileBtnDis: true,
            installationCode: '',
            agentInfo: {}
        };
    }

    componentReceive(agentInfo) {
        this.setState({
            downloadConfigFileDiaShow: true,
            downloadConfigFileBtnDis: true,
            agentInfo: agentInfo
        }, () => {
            this.setInstallationCode();
        });
    }

    setInstallationCode() {
        $$.loading(true);
        let agentId = this.state.agentInfo.Id;
        let option = {
            url: "/api/CPAgentMgmtApi/GetInstallationCode",
            method: "POST",
            data: agentId
        };
        fetchUtility(option).then((res) => {
            this.setState({
                installationCode: res,
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    closeDownloadConfigFileDialog = () => {
        this.setState({ downloadConfigFileDiaShow: false });
    };

    copyCode = () => {
        let downloadConfigFileCodeInput = document.querySelector(".cpAgentDownloadCodeInput .aui-input-target");
        downloadConfigFileCodeInput.select();
        document.execCommand("copy");
        this.setState({ downloadConfigFileBtnDis: false });
    };

    onDownloadConfigFile = () => {
        this.dispatch('raDownloadConfigFileForm', '/api/CPAgentMgmtApi/DownloadAgentConfig', [{ name: 'agentId', value: this.state.agentInfo.Id }]);
        this.setState({ downloadConfigFileDiaShow: false });
    };

    renderDownloadInstaller() {
        return <Download id="raDownloadConfigFileForm"></Download>;
    }

    render() {
        return <div id={this.props.id}>
            <R.Dialog
                id="raDownloadConfigFile"
                header={RMResx.RM_CP_Agent_DownloadConfigFile}
                width={600}
                status={{ show: this.state.downloadConfigFileDiaShow }}
                destroy={true}
                onClose={this.closeDownloadConfigFileDialog}
            >
                <div>
                    <$g.FormRow label={RMResx.RM_CP_Agent_InstallCode}>
                        <div className='agent-install-intro' tabIndex= "0">{RMResx.RM_CP_Agent_InstallCode_Introduce}</div>
                    </$g.FormRow>
                    <div className="raDownloadConfigFile_InstallationCode">
                        <R.Input
                            id="raCpAgentDownloadConfigFileIpt"
                            type="text"
                            width={448}
                            value={this.state.installationCode}
                            readonly={true}
                            className="cpAgentDownloadCodeInput"
                            aria={{ ariaLabel: RMResx.RM_CP_Agent_InstallCode }}
                        />
                        <div className="downloadConfig-copy ra-cursor-pointer" tabIndex='0' onClick={this.copyCode}>
                            <R.Button id="raCpAgentDownloadBtn" type="link" icon="fia-copy" text={RMResx.RM_CP_Agent_InstallCode_CopyBtn} />
                        </div>
                    </div>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.closeDownloadConfigFileDialog} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_CP_Agent_Download_Btn} disabled={this.state.downloadConfigFileBtnDis} onClick={this.onDownloadConfigFile} />
                </>
            </R.Dialog>
            {this.renderDownloadInstaller()}
        </div>;
    }
}