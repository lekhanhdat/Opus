import PropTypes from 'prop-types';
import {Component} from 'react';

class TextCell extends Component {
    constructor(props) {
        super(props);
        this.onTextChange = this.onTextChange.bind(this);
    }

    componentDidMount() {

    }

    onTextChange(e) {
        let {onChange, name, rowData} = this.props;
        if (onChange) {
            let args = {
                fieldName: name,
                newValue: e.target.value,
                item: rowData
            };
            onChange(e, args);
        }
    }

    renderContent() {
        let {convertFunc, editing, name, showTip, rowData} = this.props;
        let content = !rowData ? "" : rowData[name];
        if (convertFunc) {
            content = convertFunc(content);
        }
        if (editing) {
            return <input
                type="text"
                defaultValue={content}
                className="ra-input"
                onChange={this.onTextChange}
            />;
        } else {
            let textProps = {};
            if (showTip) {
                textProps["data-tooltip"] = true;
                textProps["aria-label"] = content;
            }
            return <React.Fragment>
                <div className='datagrid_text_content' {...textProps}>
                    {content}
                </div>
            </React.Fragment>;
        }
    }

    render() {
        return <div className="ra-grid-cell-text">
            {this.renderContent()}
        </div>;
    }
}

TextCell.propTypes = {
    editing: PropTypes.bool,
    name: PropTypes.string,
    rowData: PropTypes.object,
    onChange: PropTypes.func,
    convertFunc: PropTypes.func,
    isShowTip: PropTypes.bool
};
TextCell.defaultProps = {
    editing: false,
    name: "",
    rowData: {},
    onChange: null,
    convertFunc: null,
    isShowTip: false
};
export {TextCell};