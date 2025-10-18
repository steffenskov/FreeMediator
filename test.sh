#!/bin/sh

rm TestResults -rf 2> /dev/null
rm coverage-report -rf 2> /dev/null

dotnet test --results-directory ./TestResults --collect:"XPlat Code Coverage"


reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html
rm TestResults -rf 2> /dev/null

xdg-open coverage-report/index.htm
