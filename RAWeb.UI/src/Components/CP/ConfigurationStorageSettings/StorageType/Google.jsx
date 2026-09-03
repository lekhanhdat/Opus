export default class Google extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            params: { cdn: false },
        };
    }

    componentInit() {
        let that = this;
        $(document).ready(function () {
            $("#raStorageSettingsPrivateIDIpt").focus(function () {
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

    onValueChanged = (value, key) => {
        this.state.params[key] = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx.MediaStorage_Google_ClientEmail}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsClientEmailIpt"
                                    type="text"
                                    value={this.state.params.name}
                                    onChange={(value) => this.onValueChanged(value, "name")}
                                    aria={{ ariaLabel: RMResx.MediaStorage_Google_ClientEmail }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx.MediaStorage_Google_PrivateID}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsPrivateIDIpt"
                                    type="password"
                                    maxlength={1000000}
                                    value={this.state.params.secret}
                                    onChange={(value) => this.onValueChanged(value, "secret")}
                                    aria={{ ariaLabel: RMResx.MediaStorage_Google_PrivateID }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx.MediaStorage_Google_ProjectID}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsProjectIDIpt"
                                    type="text"
                                    value={this.state.params.accesspoint}
                                    onChange={(value) => this.onValueChanged(value, "accesspoint")}
                                    aria={{ ariaLabel: RMResx.MediaStorage_Google_ProjectID }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx.MediaStorage_Google_BucketName}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsBucketNameIpt"
                                    type="text"
                                    value={this.state.params.containername}
                                    onChange={(value) => this.onValueChanged(value, "containername")}
                                    aria={{ ariaLabel: RMResx.MediaStorage_Google_BucketName }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}