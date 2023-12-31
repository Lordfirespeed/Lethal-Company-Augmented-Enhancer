# Contributing

## Template `Enhancer.csproj.user`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <LETHAL_COMPANY_DIR>$(HOME)/Steam/steamapps/common/Lethal Company</LETHAL_COMPANY_DIR>
    <TEST_PROFILE_DIR>$(HOME)/.config/r2modmanPlus-local/LethalCompany/profiles/Test Enhancer</TEST_PROFILE_DIR>
      <PACK_THUNDERSTORE>false</PACK_THUNDERSTORE>
  </PropertyGroup>

    <!-- Create your 'Test Profile' using your modman of choice before enabling this. 
  Enable by setting the Condition attribute to "true". *nix users should switch out `copy` for `cp`. -->
    <Target Name="CopyToTestProfile" AfterTargets="PostBuildEvent" Condition="false">
        <MakeDir
                Directories="$(TEST_PROFILE_DIR)/BepInEx/plugins/Lordfirespeed-Augmented_Enhancer"
                Condition="Exists('$(TEST_PROFILE_DIR)') And !Exists('$(TEST_PROFILE_DIR)/BepInEx/plugins/Lordfirespeed-Augmented_Enhancer')"
        />
        <Exec Command="cp &quot;$(TargetPath)&quot; &quot;$(TEST_PROFILE_DIR)/BepInEx/plugins/Lordfirespeed-Augmented_Enhancer/&quot;" />
    </Target>
</Project>
```