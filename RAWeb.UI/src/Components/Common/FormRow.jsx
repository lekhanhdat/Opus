import PropTypes from 'prop-types';

class FormRow extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        let { label, require, children, id, tipMsg } = this.props;
        return <div className="ra-form-row">
            <div className={(require ? "require " : "") + "ra-form-label"}>
                <span id={id} aria-label={label}>{label}</span>
                {tipMsg && <$g.Popover style={tipMsg ? { marginBlock: 0 } : "" }>{tipMsg}</$g.Popover>}
            </div>
            <div className="ra-form-content">
                {children}
            </div>
        </div>;
    }
}
FormRow.propTypes = {
    label: PropTypes.string,
    require: PropTypes.bool,
    tipMsg: PropTypes.string
};
FormRow.defaultProps = {
    label: "",
    require: false,
    tipMsg: ""
};

export { FormRow };