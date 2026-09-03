import PropTypes from 'prop-types';
import { Component } from 'react';

class ProcessCell extends Component {
    constructor(props) {
        super(props);

    }
     
    renderContent() {
        let { name, rowData, width, type } = this.props;
        let percent = !rowData ? 0 : rowData[name];
        return <div className="grid-cell-progressbar ra-inline-middle">
            <R.Progressbar  data={[percent]} animation={false} template={" "} classify={type}/>
            <span className="margin-left-8">{`${percent}%`}</span>
        </div>;
    }
    render() {
        return <div className="ra-grid-cell-text">
            {this.renderContent()}
        </div>;
    }
}
ProcessCell.propTypes = {
    name: PropTypes.string,
    rowData: PropTypes.object,
    width: PropTypes.number
};
ProcessCell.defaultProps = { 
    name: 0,
    rowData: {},
    width: 160
};
export { ProcessCell };