#!/bin/bash
set -e
cd `dirname $0`

for nr_threads in 1 2 4; do
    echo "Running, nr_threads=${nr_threads}"
    export GOMAXPROCS=${nr_threads}
    output_file=results/golang_1m_${nr_threads}.txt
    /usr/bin/time -v go run main-kjp.go -numtasks 1000000 -sleep 1s &> $output_file
done
