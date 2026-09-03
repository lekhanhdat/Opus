export default class FTP extends R.Component {
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

    onFolderChanged = (value) => {
        this.state.params.ftprootfolder = value;
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

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_FTP_Host"]}</div>
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
                                    aria={{ ariaLabel: RMResx["MediaStorage_FTP_Host"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_FTP_Port"]}</div>
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
                                    aria={{ ariaLabel: RMResx["MediaStorage_FTP_Port"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_FTP_Root_Folder"]}</div>
                        <div className="ra-storageType-main">
                            <R.Input
                                id="raStorageSettingsFolderIpt"
                                type="text"
                                placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Storage}
                                value={this.state.params.ftprootfolder}
                                onChange={this.onFolderChanged}
                                aria={{ ariaLabel: RMResx["MediaStorage_FTP_Root_Folder"] }}
                            />
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_FTP_Username"]}</div>
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
                                    aria={{ ariaLabel: RMResx["MediaStorage_FTP_Username"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_FTP_Password"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsPasswordIpt"
                                    type="password"
                                    value={this.state.params.secret}
                                    onChange={this.onPasswordChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_FTP_Password"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}