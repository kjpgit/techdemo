Benchmark the performance / throttling of AWS Lambda,
specifically DNS queries in a VPC.

For example, I tested with 256 concurrency (fanout=8), and found:

  * A cacheable domain (foo.somewhere.com) gets 250-300k/sec total.  This is about 1K/sec per lambda.

  * A non-cacheable domain (\*.somewhere.com) only gets 10k/sec total (Note: A \* is replaced with a random UUID).
    This is only 40/sec per Lambda!

Note that I had 2 automatically-created ENIs for the above test, so that still
doesn't make sense.  Documentation says each VPC ENI is only supposed to do 1k/sec.

Source code of Lambda [here](lambda/main.py)

tl;dr Performance of Lambda DNS is highly variable and undocumented.
