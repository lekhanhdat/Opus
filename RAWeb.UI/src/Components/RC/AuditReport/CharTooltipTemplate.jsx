export default class LineCharTooltipTemplate extends React.PureComponent {
    constructor(props) {
        super(props);
    }

    render() {
        let templateData = this.props.data;
        return (
            <div className="reco-chart-tooltip-wrapper">
                <div className="reco-chart-tooltip-item">
                    <div className="reco-chart-tooltip-title">{templateData.filterName}:</div>
                    <div className="reco-chart-tooltip-value">{templateData.LabelStr}</div>
                </div>
                <div className="reco-chart-tooltip-item">
                    <div className="reco-chart-tooltip-title">{RMResx.RM_JS_RC_Audit_Activity_X}:</div>
                    <div className="reco-chart-tooltip-value">{templateData.valueCount}</div>
                </div>
            </div>
        );
    }
}