import PropTypes from 'prop-types';

let radioNameIndex = 1;
class RadioGroup extends React.Component {
    constructor(props) {
        super(props);
        let nameIndex = radioNameIndex++;
        this.name = "raRadioGroup" + nameIndex;
    }

    renderChildren(props) {
        let tempIdx = 1;
        return React.Children.map(props.children, child => {
            if (child && child.type === RadioOption) {
                return React.cloneElement(child, {
                    name: this.props.name ? this.props.name : this.name + "_" + tempIdx++,
                    checked: child.props.value == props.value,
                    disabled: props.disabled || child.props.disabled,
                    onChange: props.onChange
                });
            }
            else {
                return child;
            }
        });
    }

    render() {
        return <React.Fragment>
            <div className={"ra-radioGroup " + this.props.className}>
                {this.renderChildren(this.props)}
            </div>
        </React.Fragment>;
    }
}
RadioGroup.propTypes = {
    name: PropTypes.string,
    value: PropTypes.string,
    onChange: PropTypes.func,
    className: PropTypes.string,
};
RadioGroup.defaultProps = {
    name: "",
    value: '',
    onChange: null,
    className: "",
};

class RadioOption extends React.Component {
    constructor(props) {
        super(props);
        this.onChange = this.onChange.bind(this);
    }

    onChange(e) {
        if (this.props.onChange) {
            this.props.onChange(this.props.value);
        }
    }

    render() {
        let { checked, disabled, name, text, title, value, children, isBlock, isFlex } = this.props;
        if (!text) {
            text = "";
        }
        if (!title) {
            title = text;
        }
        let radioClassName = "ra-inline-middle ra-radio-container";
        let radioFlex = "";
        if(isBlock){
            radioClassName = "ra-radio-container";
        }
        if (isFlex) {
            radioFlex = "flex align-start";
        }
        return <div className={`${radioClassName} ${radioFlex}`}>
            <R.Radio
                // attrs={{ className: "ra-radio" }}
                name={name}
                text={text}
                title={title}
                value={value}
                checked={checked}
                disabled={disabled}
                onChange={this.onChange}
            />
            {children}
        </div>;
    }
}
RadioOption.propTypes = {
    checked: PropTypes.bool,
    disabled: PropTypes.bool,
    value: PropTypes.string,
    name: PropTypes.string,
    text: PropTypes.string,
    title: PropTypes.string,
    onChange: PropTypes.func,
    isBlock: PropTypes.bool,
};
RadioOption.defaultProps = {
    checked: false,
    disabled: false,
    value: null,
    name: null,
    text: null,
    title: null,
    onChange: null,
    isBlock: false,
};

export { RadioGroup, RadioOption };