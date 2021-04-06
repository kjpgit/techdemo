## Overview

Benchmark the performance / throttling of AWS Lambda,
specifically DNS queries in a VPC.

## Results

Results with a concurrency of 256

  * A cacheable request (foo.somewhere.com) gets 250-300k/sec total.  This is about 1K/sec per lambda.

  * A non-cacheable request ($randomuuid.somewhere.com) only gets 10k/sec total.
    This is only 40/sec per Lambda!

Note that I had 2 automatically-created ENIs for the above test, so that still
doesn't make sense.  Documentation says each VPC ENI is only supposed to do 1k/sec.

Source code of Lambda [here](lambda/main.py)

## Conclusion

Performance of Lambda DNS is highly variable (a cache seems to be involved) and undocumented.
I have no idea what the performance guarantees or limits are, but it doesn't seem to be based on ENIs.
