export default class CompatibleStorage extends R.Component {
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
            $("#raStorageSettingsSecretKeyIpt").focus(function () {
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

    onBucketNameChanged = (value) => {
        this.state.params.bucketname = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onAccessIdChanged = (value) => {
        this.state.params.name = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onSecretKeyChanged = (value) => {
        this.state.params.secret = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    onEndpointChanged = (value) => {
        this.state.params.endpoint = value;
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_S3Compatible_Bucket_Name"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsBucketNameIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Storage}
                                    value={this.state.params.bucketname}
                                    onChange={this.onBucketNameChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_S3Compatible_Bucket_Name"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_S3Compatible_Access_Key_ID"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsAccessIdIpt"
                                    type="text"
                                    placeholder={RMResx.RM_AR_CP_GSS_Placeholder_Accessid}
                                    value={this.state.params.name}
                                    onChange={this.onAccessIdChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_S3Compatible_Access_Key_ID"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_S3Compatible_Secret_Access_Key"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsSecretKeyIpt"
                                    type="password"
                                    value={this.state.params.secret}
                                    onChange={this.onSecretKeyChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_S3Compatible_Secret_Access_Key"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_S3Compatible_Endpoint"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsEndpointIpt"
                                    type="text"
                                    value={this.state.params.endpoint}
                                    onChange={this.onEndpointChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_S3Compatible_Endpoint"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}