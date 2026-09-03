import PropTypes from 'prop-types';

function convert(child) {
    if (typeof child === 'string') {
        return <span>{child}</span>;
    }
    return child;
}

class I18NProvider extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        const {
            msg,
            children,
            ...others
        } = this.props;

        const reg = /\{\d+\}/g;
        const args = msg.match(reg); //["{1}","{0}"]
        const msgArr = msg.split(reg); //["a, b", " cc"]
        const reAr = React.Children.toArray(children);

        let reactArr = [];
        //["a, b", <div></div> " cc", "undefined"]
        msgArr.map((v, k) => {
            reactArr.push(v);
            if (k != msgArr.length - 1) {
                const index = args[k].replace(/\{(\d+)\}/g, function (m, n) {
                    return n;
                });
                reactArr.push(reAr[index]);
            }
        });

        const temp = React.Children.map(reactArr, convert);

        return (
            <span {...others}>{temp}</span>
        );
    }
}

const propTypes = {
    msg: PropTypes.string
};

const defaultProps = {
    msg: ""
};

I18NProvider.propTypes = propTypes;
I18NProvider.defaultProps = defaultProps;

export { I18NProvider };