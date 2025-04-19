#!/bin/sh

rm TestResults -rf 2> /dev/null
rm coverage-report -rf 2> /dev/null

dotnet test --results-directory ./TestResults --collect:"XPlat Code Coverage"


coverage_file=$(find . -name 'coverage.cobertura.xml' -print -quit)

# Check if the file exists and move it
if [[ -n "$coverage_file" ]]; then
  mv "$coverage_file" .
  echo "Moved coverage file to the current directory."
else
  echo "Coverage file not found."
fi


reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
rm coverage.cobertura.xml
rm TestResults -rf 2> /dev/null

xdg-open coverage-report/index.htm
