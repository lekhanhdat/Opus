import logging
import os
import json
import datetime
import logging.handlers
os.makedirs("./logs", exist_ok=True)


show_keys = ['name', 'msg', 'levelname', 'pathname', 'lineno', 'funcName']


class JSONFormatter(logging.Formatter):
    def format(self, record):
        extra = self.build_record(record)
        self.set_format_time(extra)  # set time
        extra['Message'] = record.msg  # set message
        if record.exc_info:
            extra['exc_info'] = self.formatException(record.exc_info)
        return json.dumps(extra, ensure_ascii=False)

    def build_record(self, record):
        # all_keys: 'name', 'msg', 'args', 'levelname', 'levelno', 'pathname', 'filename', 'module', 'exc_info', 'exc_text', 'stack_info',
        #           'lineno', 'funcName', 'created', 'msecs', 'relativeCreated', 'thread', 'threadName', 'processName', 'process'
        extra = {k: record.__dict__[k] for k in show_keys if k in record.__dict__}

        now = datetime.datetime.utcnow()
        format_time = now.strftime("%Y-%m-%dT%H:%M:%S" + ".%03d" % (now.microsecond / 1000) + "Z")
        extra['timestamp'] = format_time

        if record.exc_info:
            extra['exc_info'] = str(self.formatException(record.exc_info))
        else:
            extra['exc_info'] = ''
        extra['Message'] = record.msg  # set message
        return extra

    @classmethod
    def set_format_time(cls, extra):
        now = datetime.datetime.utcnow()
        format_time = now.strftime("%Y-%m-%dT%H:%M:%S" + ".%03d" % (now.microsecond / 1000) + "Z")
        extra['timestamp'] = format_time
        return format_time


def get_logger(json_path='', name='root', level=logging.INFO):
    logger = logging.getLogger(f'{json_path}_{name}')
    logger.propagate = False
    logger.setLevel(level)
    stream_handler = logging.StreamHandler()
    stream_handler.setFormatter(logging.Formatter('%(asctime)s %(pathname)s:%(funcName)s[line:%(lineno)d] %(message)s'))
    logger.addHandler(stream_handler)

    if json_path:
        d = os.path.dirname(os.path.abspath(json_path))
        os.makedirs(d, exist_ok=True)
        json_handler = logging.handlers.RotatingFileHandler(json_path, 'a', 50 * 1024 * 1024, 2, encoding='utf8')
        json_formatter = JSONFormatter()
        json_handler.setFormatter(json_formatter)
        json_handler.setLevel(logging.DEBUG)
        logger.addHandler(json_handler)

    logger.info(f'Logger init {logger}  json_path={json_path}')
    return logger


json_log_path = './logs/webapi.log'
logger = get_logger(json_log_path)