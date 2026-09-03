import StringUtil from '../../Utilities/StringUtil';
export default class JMDetailList extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            detailList: this.props.data || [],
        };
    }

    componentDidUpdate(prevProps) {
        if (prevProps.data !== this.props.data) {
            this.setState({ detailList: this.props.data || [] });
        }
    }

    render() {
        return <div className="ra-main-section jm-detail-list" id={this.props.id}>
            {this.props.title && <div className="jm-detail-title">{this.props.title}</div>}
            <div className='wrapper'>
                <div className="row">
                    {!this.props.isShowJobSetting ?
                        <>
                            <div className="col-md-10">
                                {
                                    this.state.detailList.map((item, key) => {
                                        return <div className="col-md-6 jm-detail-cell" key={key}>
                                            <div className="col-md-5 jm-detail-label ra-ellipsis" data-tooltip="ifneed" tabIndex="0">{StringUtil.trimEndColon(item[this.props.textField])}</div>
                                            <div className="col-md-7 jm-detail-value ra-ellipsis" data-tooltip="ifneed" tabIndex="0" data-tooltip-wrap="force">{item[this.props.valueField]}</div>
                                        </div>;
                                    })
                                }
                            </div>
                            <div className="col-md-2"></div>
                        </>
                        :
                            !!this.state.detailList.length && <div className='col-md-12 jm-setting-section'>
                            <div className={`col-md-2 jm-setting-label ra-ellipsis`} data-tooltip="ifneed" tabIndex="0">{StringUtil.trimEndColon(RMResx["StorageOptimization.Service_76708d46-2c6b-44a2-a717-cc05e89daba9"])}</div>
                            <div className='col-md-10 jm-setting-list-value'>
                                {
                                    this.state.detailList.map((item, key) => {
                                        return <div key={key} className="jm-setting-value ra-ellipsis" data-tooltip="ifneed" tabIndex="0" data-tooltip-wrap="force">{item[this.props.valueField]}</div>
                                    })
                                }
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>;
    }
}