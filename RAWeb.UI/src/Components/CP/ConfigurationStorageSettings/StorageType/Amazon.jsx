import { StorageRegion, CustomizedRegion } from "../../CPConstants";

export default class Amazon extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        let storageRegionList = this.sortStorageRegionList(StorageRegion);
        storageRegionList.push(CustomizedRegion);

        this.state = {
            params: { region: "usstandard" },
            storageRegionList: storageRegionList,
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

    sortStorageRegionList = (arr) => {
        return RM.deepcopy(arr).sort((a, b) => a.name.localeCompare(b.name));
    }

    getParams = () => {
        return this.state.params;
    }

    setData = (args) => {
        if (args.Id) {
            let currentStorageRegionList = this.sortStorageRegionList(StorageRegion);
            currentStorageRegionList.push(CustomizedRegion);
            currentStorageRegionList.forEach(item => {
                item.checked = (item.value == args.mCurrentXRI.Params.region) ? true : false;
            });
            this.setState({ storageRegionList: currentStorageRegionList });
        } else {
            Object.assign(args.mCurrentXRI.Params, this.state.params);
        }
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

    onStorageRegionChanged = (args) => {
        this.state.params.region = args.newValue.value;
        if (args.newValue.value === CustomizedRegion.value) {
            this.state.params.advanced = true;
            if (this.props.onSelectCustomizedRegion) {
                this.props.onSelectCustomizedRegion("CustomizedRegion=");
            }
        } else {
            if (this.props.onSelectCustomizedRegion) {
                this.props.onSelectCustomizedRegion("");
            }
        }
        this.setState({ params: RM.deepcopy(this.state.params) });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Amazon_Bucket_Name"]}</div>
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
                                    aria={{ ariaLabel: RMResx["MediaStorage_Amazon_Bucket_Name"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Amazon_Access_Key_ID"]}</div>
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
                                    aria={{ ariaLabel: RMResx["MediaStorage_Amazon_Access_Key_ID"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Amazon_Secret_Access_Key"]}</div>
                        <div className="ra-storageType-main">
                            <R.Validation
                                element="Input"
                                require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                                <R.Input
                                    id="raStorageSettingsSecretKeyIpt"
                                    type="password"
                                    value={this.state.params.secret}
                                    onChange={this.onSecretKeyChanged}
                                    aria={{ ariaLabel: RMResx["MediaStorage_Amazon_Secret_Access_Key"] }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="ra-storageType-content">
                        <div className="ra-storageType-title">{RMResx["MediaStorage_Amazon_Storage_Region"]}</div>
                        <div className="ra-storageType-main">
                            <R.Combobox
                                id="raStorageSettingsRegionCom"
                                tooltipField="name"
                                width='100%'
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={this.state.storageRegionList}
                                onChange={this.onStorageRegionChanged}
                                aria={{ ariaLabel: RMResx["MediaStorage_Amazon_Storage_Region"] }}
                            />
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}