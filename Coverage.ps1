$artifactLocation = 'artifacts\bin\Nito.Mvvm.Async\Debug\net46'
$testProjectLocation = 'test/UnitTests'
$outputLocation = 'testResults'
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/StephenCleary/BuildTools/bb636fc76a9017d3cee13d7229539007077845ef/Coverage.ps1'))
