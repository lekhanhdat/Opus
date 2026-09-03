import {Component} from "react";
export default class CommonViewDetail extends Component {
    constructor(props) {
        super(props);
        this.state = {
            data: this.props.data,
            labelWidth: this.props.labelWidth
        };
    }

    render() {
        return <div id="raRCReportViewDetail">
            <div className='ra-detail-content'>
                <$g.DetailList className="ra-detail-content-list" labelWidth={this.state.labelWidth}>
                    {this.state.data.map((column, index) => {
                        return <$g.DetailRow key={index}>
                            <$g.DetailCell
                                label={column.columnName}
                                value={column.columnValue}/>
                        </$g.DetailRow>;
                    })}
                </$g.DetailList>
            </div>
        </div>;
    }
}
