Remove-Item -Path publish -Recurse
& dotnet publish source\examples\Stageplay.Example\Stageplay.Example.csproj -c Release -r win-x64 --self-contained -p:PublishAOT=true -o publish\win-x64
& dotnet run --project source\examples\Stageplay.Example.ContentBuilder\Stageplay.Example.ContentBuilder.csproj -c Release -- build -c source\examples\ExampleContent -o publish\content_staging
& dotnet run --project source\examples\Stageplay.Example.ContentBuilder\Stageplay.Example.ContentBuilder.csproj -c Release -- fsarc -c publish\content_staging -o publish\win-x64\Content\data01.fsarc
