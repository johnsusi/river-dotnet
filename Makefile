.PHONY: all clean build coverage test report

build:
	dotnet build

test:
	rm -Rf test/River.Streaming.Test/TestResults
	dotnet test --collect:"XPlat Code Coverage"

report: test
	reportgenerator -reports:test/River.Streaming.Test/TestResults/**/coverage.cobertura.xml -targetdir:coveragereport "-reporttypes:Html;HtmlSummary" -historydir:coveragehistory
	