Example python lambda that demonstrates:

* CDK deployment of a "real world" python Lambda
* Using Docker to cache dependencies and insulate build system from CDK requirements
* Private github repo(s) for company-specific python dependencies
* Python external dependencies with version pinning (requests, boto3)
* Lambda Layers to hold large dependencies, so the web console can edit the function code

I have used this boilerplate on 3 CDK projects and thought I'd share it, since it's non-trivial.
It's amazing how much boilerplate a 2019 cloud python lambda with dependencies needs.

To run:

* Set your AWS account id in deploy/app.py
* Read the top of deploy/deploy.sh
* Run GITHUB_ACCESS_TOKEN="not-needed-for-demo" ./deploy/deploy.sh
* Delete the acme-prod-DemoLambda cloudformation stack when you're done
