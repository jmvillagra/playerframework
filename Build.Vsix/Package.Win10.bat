@pushd %~dp0%

@set ZIP=%ProgramFiles%\7-Zip\7z.exe

cd Win10

cd Microsoft.PlayerFramework.Js.Core
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Js.Core.zip "*"

cd ..\Microsoft.PlayerFramework.Js.Adaptive
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Js.Adaptive.zip "*"

cd ..\Microsoft.PlayerFramework.Js.TimedText
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Js.TimedText.zip "*"

cd ..\Microsoft.PlayerFramework.Js.Advertising
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Js.Advertising.zip "*"

cd ..\Microsoft.PlayerFramework.Js.Analytics
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Js.Analytics.zip "*"

cd ..\Microsoft.PlayerFramework.Analytics.Advertising
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Analytics.Advertising.zip "*"

cd ..\Microsoft.Media.AudienceInsight
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.Media.AudienceInsight.zip "*"

cd ..\Microsoft.PlayerFramework.Analytics.AudienceInsight
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Analytics.AudienceInsight.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.Core
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.Core.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.Adaptive
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.Adaptive.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.TimedText
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.TimedText.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.WebVTT
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.WebVTT.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.Advertising
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.Advertising.zip "*"

cd ..\Microsoft.PlayerFramework.Xaml.Analytics
"%ZIP%" a ..\..\Microsoft.PlayerFramework\Microsoft.PlayerFramework.Xaml.Analytics.zip "*"

cd ..\..\Microsoft.PlayerFramework

move Microsoft.PlayerFramework.Js.Core.zip Microsoft.PlayerFramework.Js.Core.Win10.vsix
move Microsoft.PlayerFramework.Js.Adaptive.zip Microsoft.PlayerFramework.Js.Adaptive.Win10.vsix
move Microsoft.PlayerFramework.Js.TimedText.zip Microsoft.PlayerFramework.Js.TimedText.Win10.vsix
move Microsoft.PlayerFramework.Js.Advertising.zip Microsoft.PlayerFramework.Js.Advertising.Win10.vsix
move Microsoft.PlayerFramework.Js.Analytics.zip Microsoft.PlayerFramework.Js.Analytics.Win10.vsix
move Microsoft.PlayerFramework.Analytics.Advertising.zip Microsoft.PlayerFramework.Analytics.Advertising.Win10.vsix
move Microsoft.PlayerFramework.Analytics.AudienceInsight.zip Microsoft.PlayerFramework.Analytics.AudienceInsight.Win10.vsix
move Microsoft.Media.AudienceInsight.zip Microsoft.Media.AudienceInsight.Win10.vsix

move Microsoft.PlayerFramework.Xaml.Core.zip Microsoft.PlayerFramework.Xaml.Core.Win10.vsix
move Microsoft.PlayerFramework.Xaml.Adaptive.zip Microsoft.PlayerFramework.Xaml.Adaptive.Win10.vsix
move Microsoft.PlayerFramework.Xaml.TimedText.zip Microsoft.PlayerFramework.Xaml.TimedText.Win10.vsix
move Microsoft.PlayerFramework.Xaml.WebVTT.zip Microsoft.PlayerFramework.Xaml.WebVTT.Win10.vsix
move Microsoft.PlayerFramework.Xaml.Advertising.zip Microsoft.PlayerFramework.Xaml.Advertising.Win10.vsix
move Microsoft.PlayerFramework.Xaml.Analytics.zip Microsoft.PlayerFramework.Xaml.Analytics.Win10.vsix

@popd

@echo.
@echo Done.
@echo.
