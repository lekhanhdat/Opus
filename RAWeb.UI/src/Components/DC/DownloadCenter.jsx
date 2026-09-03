import SiteMapLinks from "../../Constants/SiteMapLinks";
import DownloadFile from "./DownloadFile";
import '../../Less/DC/downloadCenter.less';

export default class DC extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            showTip: true,
        };
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    render() {
        return <div id="rmDownloadCenter">
            <$g.SiteMap data={[SiteMapLinks.DC]} />
            <div className="ra-dc-tip">
                <R.Messagebar
                    message={RMResx.RM_JS_DC_RetentionMsg}
                    classify="info"
                    onClose={this.hideMessageTip}
                    status={{ show: this.state.showTip }} />
            </div>

            <div className="ra-page-container">
                <DownloadFile />
            </div>
        </div>;
    }
}