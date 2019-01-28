#!/usr/bin/python3
import sys
import os

# Generate a markdown table (table.md) from results/*.txt
# Usage: report.py RESULTS_DIR TABLE_FILE


def get_table_row(file_name):
    short_name = os.path.basename(file_name)
    rss_values = []
    vss_values = []

    def thousands(s):
        return "{:,}".format(s)

    for line in open(file_name):
        line = line.strip()
        if "VmRSS:" in line:
            val = int(line.split()[-2])
            rss_values.append(val)
        if "VmSize:" in line:
            val = int(line.split()[-2])
            vss_values.append(val)
        if "User time (seconds):" in line:
            user_time = line.split()[-1]
        if "System time (seconds):" in line:
            system_time = line.split()[-1]
        if "Elapsed (wall clock) time (h:mm:ss or m:ss):" in line:
            wall_time = line.split()[-1]
        if "Maximum resident set size (kbytes):" in line:
            max_rss = int(line.split()[-1])

    short_name = short_name.replace("csharp_1m", "c#")
    short_name = short_name.replace("golang_1m", "go")
    short_name = short_name.replace(".txt", "t")

    ret = [
        short_name,
        user_time,
        system_time,
        thousands(sum(rss_values) // len(rss_values)),
        thousands(max_rss),
        wall_time[:-3],
        ]
    return [str(x) for x in ret]


lines = []
lines.append(["Name", "UserCPU", "SysCPU", "AvgRSS", "MaxRSS", "Wall"])

results_dir = sys.argv[1]

for name in sorted(os.listdir(results_dir)):
    row = get_table_row(results_dir + "/" + name)
    lines.append(row)

for line in lines:
    print("    ".join(line))

