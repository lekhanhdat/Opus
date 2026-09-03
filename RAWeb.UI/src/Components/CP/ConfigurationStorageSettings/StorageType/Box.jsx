export default class Box extends R.Component {
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
            $("#raStorageSettingsRefreshIpt").focus(function () {
                that.state.params.boxrefreshtokensecret = "";
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

    onFolderNameChanged = (value) => {
        this.state.params.boxrootfoldername = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onEmailChanged = (value) => {
        this.state.params.boxemailaddress = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onRefreshTokenChanged = (value) => {
        this.state.params.boxrefreshtokensecret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Box_RootFolderName"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsFolderNameIpt"
                                    type="text"
                                    value={this.state.params.boxrootfoldername}
                                    onChange={this.onFolderNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Box_RootFolderName"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["Common.GuiControls_E-mail Address"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
                                rules={{
                                    isEmail: true,
                                }}
                            >
                                <R.Input
                                    id="raStorageSettingsEmailIpt"
                                    type="text"
                                    value={this.state.params.boxemailaddress}
                                    onChange={this.onEmailChanged}
                                    aria={{ ariaLabel: RMResx["Common.GuiControls_E-mail Address"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Box_BoxRefreshToken"]}</div>
                        <div className="ra-storageType-file">
                            <div className="ra-storageType-input">
                                <R.Validation
                                    element="Input"
                                    require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                    <R.Input
                                        id="raStorageSettingsRefreshIpt"
                                        type="password"
                                        value={this.state.params.boxrefreshtokensecret}
                                        onChange={this.onRefreshTokenChanged}
                                        aria={{ ariaLabel: RMResx["MediaStorage_Box_BoxRefreshToken"] }}
                                    />
                                </R.Validation>
                            </div>
                            <div id="boxTokenBtn">
                                <a href="https://www.box.com/api/oauth2/authorize?response_type=code&client_id=6wlvcp6l8tujowomdwrbjtqlwhdxzqfq" target="_blank" rel="noopener noreferrer" style={{ textDecoration: "none" }}>
                                    <R.Button
                                        id="raStorageSettingsTokenBtn"
                                        text={RMResx["Gui.Common_266D203A-4123-4E50-835F-82AE27C88B9F"]}
                                    />
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}