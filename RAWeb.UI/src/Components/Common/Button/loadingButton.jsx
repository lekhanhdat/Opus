import "./index.less";

export const LoadingButton = (props) => {
    return (
        <R.Button
            className={props.isBusy ? "ra-common-loading-button" : ""}
            {...props}
        />
    );
};
