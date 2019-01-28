#!/bin/bash
set -e
cd `dirname $0`

dotnet build -c Release 

# Docs on CLR env variables:
# https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/clr-configuration-knobs.md

for nr_threads in 1 2 4; do
    echo "Running, nr_threads=${nr_threads}"
    export COMPlus_ThreadPool_ForceMaxWorkerThreads=${nr_threads}
    export COMPlus_ThreadPool_ForceMinWorkerThreads=${nr_threads}
    output_file=results/csharp_1m_${nr_threads}.txt
    /usr/bin/time -v dotnet ./bin/Release/netcoreapp2.2/project.dll 1000000 1.0 60000000 &> $output_file
done
