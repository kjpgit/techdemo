#!/usr/bin/env python3
"""
Cloudformation generation
"""
from aws_cdk import core

import my_stack


app = core.App()

# Best practice is to explicitly set account ids and region
ACME_ENV_WEST = dict(account="123412341234", region="us-west-2")

my_stack.MyStack(app,
        "acme-prod-DemoLambda",
        env=ACME_ENV_WEST,
        )

app.synth()
