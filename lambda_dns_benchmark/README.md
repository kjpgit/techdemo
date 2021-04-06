## Overview

Benchmark how may DNS queries/second AWS Lambda can do, in a VPC.
Most microservices heavily rely on DNS, so performance and scalability here is fundamental.

## Results (VPC)

Results with a concurrency of 256, in a VPC, us-east-2.  All other settings are in the CDK code.

  * A cacheable request (foo.somewhere.com) can do about 1000/sec per Lambda (250-300K/sec total).

  * A non-cacheable request ($randomuuid.somewhere.com) only does 40/sec per Lambda! (10k/sec total)  I used a route53 wildcard record for this test.

Note that I had 2 automatically-created ENIs for the above test (one per subnet), so that still
doesn't make sense.  Documentation says each VPC ENI is only supposed to do 1k/sec, but I'm getting 5x that.

Also, the /etc/resolv.conf in Lambda seems to use a 169.254.x resolver (which indicates a hypervisor is involved), instead of directly querying the VPC's resolver through the ENI.

Source code of Lambda [here](lambda/main.py)

## Results (non-VPC)

  * A non-cacheable request ($randomuuid.somewhere.com) does 80/sec per Lambda (20k/sec total).
    This is 2x of the VPC result.  The resolver was also a 169.254.x.

## Conclusion

AWS support told us about the (undocumented) DNS cache and this test verifies it exists.
However that makes performance highly variable.

I have no idea what the performance guarantees or limits are, but it doesn't
seem to be using the published ENI rate.  Limiting it by ENIs, which are in
turn based on Lambda security groups, would be confusing-as-hell for
developers.  It should scale based on Lambda size and concurrency.

