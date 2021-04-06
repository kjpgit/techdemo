## Overview

Benchmark how may DNS queries/second AWS Lambda can do, in a VPC.
Most microservices heavily rely on DNS, so performance and scalability here is fundamental.

## Results

Results with a concurrency of 256, in a VPC.  All other settings are in the CDK code.

  * A cacheable request (foo.somewhere.com) can do about 1000/sec per Lambda (250-300K/sec total).

  * A non-cacheable request ($randomuuid.somewhere.com) only does 40/sec per Lambda! (10k/sec total)  I used a route53 wildcard record for this test.

Note that I had 2 automatically-created ENIs for the above test, so that still
doesn't make sense.  Documentation says each VPC ENI is only supposed to do 1k/sec.

Also, the /etc/resolv.conf in Lambda seems to use a 169.254.x resolver (which indicates a hypervisor is involved), instead of directly querying the VPC's resolver through the ENI.

Source code of Lambda [here](lambda/main.py)

## Conclusion

Performance of Lambda DNS is highly variable (a cache seems to be involved) and undocumented.
I have no idea what the performance guarantees or limits are, but it doesn't seem to be based on ENIs.

AWS support told us about the (undocumented) DNS cache and this test verifies it exists.  However that raises even more questions.
