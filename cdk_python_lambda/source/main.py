"""
Scheduled cron lambda (demo)
"""
import boto3
import requests

#from acme_common import acme_logging


#@acme_logging.lambda_handler
def main(event, context):
    print("Hello, World!")
    print("requests version: %s" % requests.__version__)
