import { JobTypeI18N } from "./Constants/index";

const JobNotificationJobInfo = ({jobType, options, disabled, onChange}) => {

    const onProfileJobInfoChanged = (args) => {
        onChange(jobType, args);
    };

    return (
        <div className="reco-job-notification-create-module">
            <div className="reco-job-notification-create-jobTitle" tabIndex="0">
                <$g.I18NProvider msg={JobTypeI18N.get(jobType)} />
            </div>
            <R.Checkbox.Group
                id={'checkbox_group'}
                aria={`${jobType}`}
                block
                disabled={disabled}
                name={`checkbox-group-${jobType}`}
                items={options}
                onChange={onProfileJobInfoChanged}
            />
        </div>
    );
};

export default JobNotificationJobInfo;