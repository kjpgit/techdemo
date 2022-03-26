TEST_URL = http://localhost:5001
CURL_TEST = curl -s -w "\nResponse: %{http_code}\n" \
    -H "Content-type: application/json" \
    -H "Authorization: bearer 12345" \
    -H "X-My-Environment: staging"
.PHONY: build run br test
build:
	@# Yellow colored warnings are illegible
	NO_COLOR=1 dotnet build
run:
	@# Faster than calling dotnet run
	DOTNET_URLS=$(TEST_URL) ./bin/Debug/net6.0/server

br: build run

test:
	@echo "## Adding invalid job"
	@$(CURL_TEST) $(TEST_URL)/api/AddJob -d @test/invalid_job.json
	@echo "## Adding good job"
	@$(CURL_TEST) $(TEST_URL)/api/AddJob -d @test/good_job.json
	@echo "## Adding good job - idempotent retry"
	@$(CURL_TEST) $(TEST_URL)/api/AddJob -d @test/good_job.json
	@echo "## Taking job"
	@$(CURL_TEST) $(TEST_URL)/api/TakeJob -d @test/take_job.json
	@echo "## Taking job a 2nd time (does not exist)"
	@$(CURL_TEST) $(TEST_URL)/api/TakeJob -d @test/take_job.json
