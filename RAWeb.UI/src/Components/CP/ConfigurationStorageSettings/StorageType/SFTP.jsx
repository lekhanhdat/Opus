export default class SFTP extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            params: {},
        };
    }

    componentInit() {
        let that = this;
        $(document).ready(function () {
            $("#raStorageSettingsPasswordIpt").focus(function () {
                that.state.params.secret = "";
                that.setState({ params: RM.deepcopy(that.state.params) });
            });
            $("#raStorageSettingsPrivateKeyIpt").focus(function () {
                that.state.params.privatekeypasswordsecret = "";
                that.setState({ params: RM.deepcopy(that.state.params) });
            });
        });
    }

    getParams = () => {
        return this.state.params;
    }

    setData = (args) => {
        this.setState({ params: args.mCurrentXRI.Params });
    }

    onHostChanged = (value) => {
        this.state.params.host = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onPortChanged = (value) => {
        this.state.params.port = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onRootFolderChanged = (value) => {
        this.state.params.sftprootfolder = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onUsernameChanged = (value) => {
        this.state.params.name = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onPasswordChanged = (value) => {
        this.state.params.secret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onBrowseClick = (args) => {
        this.importChooseInput.value = "";
        this.importChooseInput.click();
    }

    onChooseFileChange = (e) => {
        let filePath = this.importChooseInput.value;
        if (!filePath) {
            return;
        }

        let fileInfo = this.importChooseInput.files[0];
        let reader = new FileReader();
        reader.readAsText(fileInfo, "UTF-8");
        let that = this;
        reader.onload = function (e) {
            const val = e.target.result;
            that.state.params.privatekeysecret = val;
        };

        this.state.params.privatekeyfile = this.importChooseInput.files[0].name;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onPrivateKeyChanged = (value) => {
        this.state.params.privatekeypasswordsecret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_Host"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsHostIpt"
                                    type="text"
                                    placeholder="10.0.0.1"
                                    value={this.state.params.host}
                                    onChange={this.onHostChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_SFTP_Host"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_Port"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsPortIpt"
                                    type="text"
                                    placeholder="21"
                                    value={this.state.params.port}
                                    onChange={this.onPortChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_SFTP_Port"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_Root_Folder"]}</div>
                        <div className="ra-storageType-main">
                            <R.Input
                                id="raStorageSettingsRootFolderIpt"
                                type="text"
                                placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Storage}
                                value={this.state.params.sftprootfolder}
                                onChange={this.onRootFolderChanged}
                                aria={{ ariaLabel: RMResx["MediaStorage_SFTP_Root_Folder"] }}
                            />
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_Username"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsUsernameIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Admin}
                                    value={this.state.params.name}
                                    onChange={this.onUsernameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_SFTP_Username"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_Password"]}</div>
                        <div className="ra-storageType-main">
                            <R.Input
                                id="raStorageSettingsPasswordIpt"
                                type="password"
                                value={this.state.params.secret}
                                onChange={this.onPasswordChanged}
                                aria={{ ariaLabel: RMResx["MediaStorage_SFTP_Password"] }}
                            />
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_PrivateKeyFile"]}</div>
                        <div className="ra-storageType-file">
                            <div className="ra-storageType-input">
                                <R.Input
                                    id="raStorageSettingsFileIpt"
                                    type="text"
                                    value={this.state.params.privatekeyfile}
                                    // tooltip={this.state.params.privatekeyfile}
                                    readonly={true}
                                    aria={{ ariaLabel: RMResx["MediaStorage_SFTP_PrivateKeyFile"] }}
                                />
                            </div>
                            <div id="sftpBrowseBtn">
                                <R.Button
                                    id="raStorageSettingsBrowseBtn"
                                    text={RMResx["Gui.Common_a2a87c22-cacd-449b-a6a0-9c0b46f86887"]}
                                    onClick={this.onBrowseClick}
                                />
                            </div>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_SFTP_PrivateKeyPassword"]}</div>
                        <div className="ra-storageType-main">
                            <R.Input
                                id="raStorageSettingsPrivateKeyIpt"
                                type="password"
                                value={this.state.params.privatekeypasswordsecret}
                                onChange={this.onPrivateKeyChanged}
                                aria={{ ariaLabel: RMResx["MediaStorage_SFTP_PrivateKeyPassword"] }}
                            />
                        </div>
                    </div>
                </div>
            </R.Validation>
            <input type="file" id="choosefile" name="fileUp"
                style={{ display: "none" }} ref={r => this.importChooseInput = r}
                onChange={this.onChooseFileChange} />
        </div>;
    }
}