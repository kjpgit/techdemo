import * as cdk from '@aws-cdk/core';
import * as lambda from "@aws-cdk/aws-lambda";
import * as aws_logs from "@aws-cdk/aws-logs";
import * as iam from "@aws-cdk/aws-iam";
import * as ec2 from "@aws-cdk/aws-ec2";
import * as path from 'path';


export class DnstestStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // Warning: this creates a VPC with NAT GWs.
    // Delete the stack when done testing, to avoid cost.

    const vpc = new ec2.Vpc(this, 'test_vpc', {
      cidr: '172.29.0.0/16',
      maxAzs: 2,
      subnetConfiguration: [
        {
          subnetType: ec2.SubnetType.PUBLIC,
          name: 'Public',
          cidrMask: 20,
        },
        {
          subnetType: ec2.SubnetType.PRIVATE,
          name: 'Private',
          cidrMask: 20,
        }
      ],
    });

    const lambda_name = `${this.stackName}-Lambda`
    const func = new lambda.Function(this, 'Lambda', {
      functionName: lambda_name,
      code: lambda.Code.fromAsset(path.join(__dirname, '../lambda')),
      runtime: lambda.Runtime.PYTHON_3_6,
      handler: 'main.handler',
      timeout: cdk.Duration.minutes(1),
      logRetention: aws_logs.RetentionDays.ONE_MONTH,
      memorySize: 768,
      vpc: vpc,
    });

    // This causes a CFN circular dependency error at deploy time.
    // Because of course this is too complicated for AWS /s.
    //func.grantInvoke(func)

    func.addToRolePolicy(new iam.PolicyStatement({
      effect: iam.Effect.ALLOW,
      resources: ['*'],
      actions: ['lambda:InvokeFunction']
    }));

  }
}
