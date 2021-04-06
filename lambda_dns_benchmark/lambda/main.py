#!/usr/bin/python
"""
Benchmark the performance / throttling of AWS Lambda,
specifically DNS queries in a VPC.
"""
import json
import os
import time
import socket
import uuid

import boto3


def handler(event, context):
    """
    Sample input event:

    {
      "fanout": 8,
      "test_hostname": "*.somewhere.com",
      "duration_secs": 60
    }

    Total worker lambdas are (2 ** `fanout`), e.g.:
    - 1 fanout = 2 total children
    - 4 fanout = 16 total children
    - 8 fanout = 256 total children

    The `test_hostname` is used for DNS lookups.
    Any '*' character is replaced with a random UUID, to break caching.

    The test runs for `duration_secs` seconds, default 30 seconds.


    To query the logs, run this in cloudwatch logs insights:

        stats sum(num_ok_per_second) as ok_per_second, sum(num_errors) as err by bin(5sec)
    """

    print("event: ", event)
    fanout = event.get("fanout", 0)
    if fanout > 0:
        event["fanout"] -= 1
        launch_fanout(2, event)
    elif event.get("debug", 0):
        """
        Lambda in VPC seems to use a 169.254.x resolver, instead of directly
        querying the VPC / ENI.
        """
        config = open("/etc/resolv.conf", "r").read()
        print("resolv.conf: ", config)
    else:
        duration_secs = event.get("duration_secs", 30)
        test_hostname = event.get("test_hostname", "www.amazon.com")
        run_benchmark(duration_secs, test_hostname)


def run_benchmark(duration_secs, test_hostname):
    end_time = time.time() + duration_secs
    while time.time() < end_time:
        run_benchmark_chunk(5, test_hostname)


def run_benchmark_chunk(duration_secs, test_hostname):
    """
    Run a benchmark for `duration_secs` seconds, then print stats in JSON
    format for cloudwatch logs insights.
    """
    run_start_time = time.time()
    end_time = time.time() + duration_secs
    num_ok = 0
    num_errors = 0
    while True:
        start_time = time.time()
        if start_time > end_time:
            break
        try:
            # NB: Always generating the UUID here, to keep performance consistent
            actual_hostname = test_hostname.replace("*", str(uuid.uuid4()))
            response = socket.gethostbyname(actual_hostname)
            num_ok += 1
        except Exception as e:
            error_secs = time.time() - start_time
            data = dict(benchmark_type="error", error_secs=error_secs, error=str(e))
            print(json.dumps(data))
            num_errors += 1

    elapsed_secs = start_time - run_start_time
    num_ok_per_second = num_ok / elapsed_secs
    data = dict(benchmark_type="stats",
            num_ok=num_ok,
            num_errors=num_errors,
            num_ok_per_second=num_ok_per_second)
    print(json.dumps(data))


def launch_fanout(num_children, event):
    """
    Invoke `num_children` children asynchronously
    """
    client = boto3.client('lambda')
    for i in range(num_children):
        response = client.invoke(
            FunctionName=os.environ['AWS_LAMBDA_FUNCTION_NAME'],
            InvocationType='Event',
            Payload=json.dumps(event).encode("utf-8"),
            )
        print("launch_fanout response", response)
