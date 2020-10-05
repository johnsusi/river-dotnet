.PHONY: all clean build nuget test report

build:
	dotnet build

clean:
	rm -Rf nupkg/* src/River.Streaming/bin src/River.Streaming/obj test/River.Streaming.Test/bin test/River.Streaming.Test/obj

nuget:
	dotnet pack -o dist

test:
	rm -Rf test/River.Streaming.Test/TestResults
	dotnet test --collect:"XPlat Code Coverage"

report: test
	reportgenerator -reports:test/River.Streaming.Test/TestResults/**/coverage.cobertura.xml -targetdir:coveragereport "-reporttypes:Html;HtmlSummary" -historydir:coveragehistory
