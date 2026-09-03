export default class Dropbox extends R.Component {
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
            $("#raStorageSettingsTokenIpt").focus(function () {
                that.state.params.dropboxaccesstokensecret = "";
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
        this.state.params.containername = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onTokenSecretChanged = (value) => {
        this.state.params.dropboxaccesstokensecret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Dropbox_Root_folder"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsFolderNameIpt"
                                    type="text"
                                    value={this.state.params.containername}
                                    onChange={this.onFolderNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Dropbox_Root_folder"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Dropbox_TokenSecret"]}</div>
                        <div className="ra-storageType-file">
                            <div className="ra-storageType-input">
                                <R.Validation
                                    element="Input"
                                    require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                    <R.Input
                                        id="raStorageSettingsTokenIpt"
                                        type="password"
                                        value={this.state.params.dropboxaccesstokensecret}
                                        onChange={this.onTokenSecretChanged}
                                        aria={{ ariaLabel: RMResx["MediaStorage_Dropbox_TokenSecret"] }}
                                    />
                                </R.Validation>
                            </div>
                            <div id="dropboxTokenBtn">
                                <a href="https://www.dropbox.com/oauth2/authorize?redirect_uri=https://www.avepointonlineservices.com/getcloudtoken/dropbox&client_id=p9kxswndtb7f6gp&response_type=code" target="_blank" rel="noopener noreferrer" style={{ textDecoration: "none" }}>
                                    <R.Button
                                        id="raStorageSettingsTokenBtn"
                                        text={RMResx["Gui.Common_6502D7F3-2B8C-4193-B00A-7BB07147D4BD"]}
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