import os

from aws_cdk import core
from aws_cdk import aws_ec2
from aws_cdk import aws_events
from aws_cdk import aws_events_targets
from aws_cdk import aws_iam
from aws_cdk import aws_lambda
from aws_cdk import aws_lambda_event_sources
from aws_cdk import aws_logs
from aws_cdk import aws_route53
from aws_cdk import aws_route53_targets
from aws_cdk import aws_sqs


class MyStack(core.Stack):
    def __init__(self, app: core.App, id: str,
            **kwargs) -> None:

        super().__init__(app, id, **kwargs)

        stack_name = core.Stack.of(self).stack_name
        env_type = stack_name.split("-")[1]   # prod, stage, qa, dev, etc.

        layer = aws_lambda.LayerVersion(self,
                "BaseLayer",
                code=aws_lambda.Code.asset("/app/layer"),
                )

        # Use only one asset instance, so we aren't using extra cloudformation parameters.
        app_code = aws_lambda.Code.asset("/app/code")

        lambda_fn = aws_lambda.Function(self,
            "Lambda",
            function_name=f"{stack_name}-CronLambda",
            code=app_code,
            layers=[layer],
            handler="main.main",
            runtime=aws_lambda.Runtime.PYTHON_3_7,
            memory_size=768,
            timeout=core.Duration.seconds(60),
            log_retention=aws_logs.RetentionDays.ONE_MONTH,
        )

        self.enable_cron(lambda_fn)


    def enable_cron(self, lambda_fn):
        # Schedule @lambda_fn every minute
        rule = aws_events.Rule(
            self, "Rule",
            schedule=aws_events.Schedule.cron(
                minute='*',
                hour='*',
                month='*',
                week_day='*',
                year='*'),
        )

        # A toy input event.  You can add multiple inputs/targets, for example
        # scheduling many servers to be scanned by a scheduled lambda in parallel
        input_event = aws_events.RuleTargetInput.from_object(dict(foo="bar"))
        rule.add_target(aws_events_targets.LambdaFunction(lambda_fn, event=input_event))
