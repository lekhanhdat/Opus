import AddRelatedRecord from "./AddRelatedRecord";
import ManageRelatedRecord from "./ManageRelatedRecord";
import "./index.less";

export default class RelatedRecords extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            tabIndex: 0,
            showManageRelatedRcords: false,
            relatedInfos: null
        };
    }

    componentInit() {
        this.loadRelatedRecordsInfos();
    }

    onClickRelatedrecordLink = () =>{
        let redirectHomeUrl = RM.RedirectUrl;
        window.open(redirectHomeUrl, "_blank");
    }

    handleSelectedIndexChanged = (index)=> {
        this.setState({ 
            tabIndex: index,
            showManageRelatedRcords: true
        });
    }

    loadRelatedRecordsInfos(){
        $$.loading(true);
        let option = {
            url: "/api/RelatedRecordsApi/GetRelatedRecordsInfos",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            this.setState({relatedInfos: res || []});
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    render() {
        const backgroundImageUrl = `${RM.gData.resCdnURL}/cloud%20records/logo_24x24.png`;
        return <div id="raRelatedRecord">
            <div className="ra-page-container">
                <div className="ra-relatedrecord-link">
                    <div className="ra-relatedrecord-link-content" onClick={this.onClickRelatedrecordLink}>
                        <div
                            className="ra-cloud-records-icon"
                            style={{ backgroundImage: `url(${backgroundImageUrl})` }}
                        ></div>
                        <div>{RMResx.RM_BCM_RelatedRecords_RecordsHomeLink}</div>
                    </div>
                </div>
                <R.Tabcontrol
                    active={this.state.tabIndex}
                    onChange={this.handleSelectedIndexChanged}
                    destroy={false}
                    flex
                >
                    <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_ManageRelatedRecordsAddTitle}>
                        {
                            this.state.relatedInfos && <AddRelatedRecord relatedInfos={this.state.relatedInfos}/>
                        }
                    </R.TabPanel>
                    <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_ManageRelatedRecordsTitle}>
                        {this.state.showManageRelatedRcords &&
                         <ManageRelatedRecord relatedInfos={this.state.relatedInfos}/>
                        }
                    </R.TabPanel>
                </R.Tabcontrol>
            </div>
        </div>; 
    }
}