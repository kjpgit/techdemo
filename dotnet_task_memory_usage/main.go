package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"runtime"
	"strings"
	"sync/atomic"
	"time"
)

func main() {
	numTasks := flag.Int("numtasks", 1, "number of tasks to run")
	sleep := flag.Duration("sleep", 1*time.Second, "duration to sleep")
	flag.Parse() // updates the pointers above to whatever values are provided
	log.Printf("numTasks=%d sleep=%s\n", *numTasks, *sleep)

	log.Printf("OS threads for Goroutine worker pool: %d", runtime.GOMAXPROCS(0))

	scoreboard := Scoreboard{lastDump: time.Now()}

	for i := 0; i < *numTasks; i++ {
		go workTask(&scoreboard, *sleep)
	}

	scoreboard.PollScores()
}

func workTask(scoreboard *Scoreboard, sleep time.Duration) {
	for {
		time.Sleep(sleep)
		scoreboard.AddHit()
	}
}

type Scoreboard struct {
	lastDump time.Time
	score    int64
}

func (s *Scoreboard) AddHit() {
	atomic.AddInt64(&s.score, 1)
}

func (s *Scoreboard) PollScores() {
	for i := 0; i < 10; i++ {
		time.Sleep(1 * time.Second)
		totalHits := atomic.LoadInt64(&s.score)
		elapsed := time.Since(s.lastDump)
		s.lastDump = time.Now()
		log.Printf("elapsed=%s totalHits=%d\n", elapsed, totalHits)
		log.Println(getMemInfo())
		atomic.StoreInt64(&s.score, 0)
	}
}

func getMemInfo() string {
	data, err := ioutil.ReadFile("/proc/self/statm")
	if err != nil {
		log.Panicln("could not read from statm")
	}
	dataStr := strings.Split(string(data), " ")

	// gcalive represents the amount of memory that's actually alive,
	// rather than just the amount of rss memory that the runtime
	// keeps around to amortize allocations.
	memstats := runtime.MemStats{}
	runtime.ReadMemStats(&memstats)
	gcalive := memstats.HeapInuse / 4096 //make it in terms of 4K pages to be consistent

	return fmt.Sprintf("(NB: 4K pages) vmsize=%s vmrss=%s gcalive=%d", dataStr[0], dataStr[1], gcalive)
}
