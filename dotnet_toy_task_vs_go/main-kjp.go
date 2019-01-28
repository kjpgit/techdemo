// NB: Originally authored by https://github.com/coder543
// Hacked by kjp

package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"runtime"
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
		scoreboard.AddHit()
		time.Sleep(sleep)
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
	for i := 0; ; i++ {
		totalHits := atomic.LoadInt64(&s.score)
		elapsed := time.Since(s.lastDump)
		s.lastDump = time.Now()
		log.Printf("elapsed=%s totalHits=%d\n", elapsed, totalHits)
		log.Println(getMemInfo())
		if (totalHits > 60000000) {
		    break;
		}
		time.Sleep(100 * time.Millisecond)
	}
}

func getMemInfo() string {
	data, err := ioutil.ReadFile("/proc/self/status")
	if err != nil {
		log.Panicln("could not read from proc")
	}
	dataStr := (string(data))

	// gcalive represents the amount of memory that's actually alive,
	// rather than just the amount of rss memory that the runtime
	// keeps around to amortize allocations.
	memstats := runtime.MemStats{}
	runtime.ReadMemStats(&memstats)
	gcalive := memstats.HeapInuse / 1024

	return fmt.Sprintf("gcalive=%d\n%s", gcalive, dataStr);
}
