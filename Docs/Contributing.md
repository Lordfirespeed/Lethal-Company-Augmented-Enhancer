# Contributing

## Template `Enhancer.csproj.user`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <LETHAL_COMPANY_DIR>$(HOME)/Steam/steamapps/common/Lethal Company</LETHAL_COMPANY_DIR>
    <TEST_PROFILE_DIR>$(HOME)/.config/r2modmanPlus-local/LethalCompany/profiles/Test Enhancer</TEST_PROFILE_DIR>
  </PropertyGroup>

  <Target Name="CopyToTestProfile" AfterTargets="PostBuildEvent" Condition="false">
    <Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(TEST_PROFILE_DIR)/BepInEx/plugins/Lordfirespeed-Augmented_Enhancer/&quot;" />
  </Target>
</Project>
```