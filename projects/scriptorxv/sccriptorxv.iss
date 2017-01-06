[_ISTool]
EnableISX=false

[InnoIDE_Settings]
LogFileOverwrite=false


[Setup]
AppName=SpringCard Scriptor XV
AppVersion=15.04
AppPublisher=SpringCard
AppPublisherURL=http://www.springcard.com
AppSupportURL=http://www.springcard.com/support
AppUpdatesURL=http://www.springcard.com/download/software.html
AppVerName=SpringCard Scriptor XV
AppCopyright=Copyright © SpringCard SAS 2015

OutputBaseFilename=scscriptorxv-1504
OutputDir=.

DefaultGroupName=SpringCard

DefaultDirName={pf}\SpringCard\ScriptorXV
ExtraDiskSpaceRequired=5

Compression=bzip
UseSetupLdr=yes
SolidCompression=true
PrivilegesRequired=admin
SignTool=SignToolCmd

AllowNoIcons=true
AllowCancelDuringInstall=false
AllowRootDirectory=false
AllowUNCPath=false

DisableStartupPrompt=true
DisableDirPage=true
DisableProgramGroupPage=true
DisableReadyPage=true
DisableReadyMemo=true
DisableFinishedPage=false

WindowStartMaximized=false
Uninstallable=true

WizardImageFile=I:\builder\ressources\setup\Setup Wizard (left).bmp
WizardSmallImageFile=I:\builder\ressources\setup\Setup Wizard (top).bmp

[Dirs]
Name: "{app}"

[Files]
Source: "..\..\_output\scscriptorxv.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "..\..\_output\scscriptorxv.exe.config"; DestDir: "{app}"; Flags: ignoreversion;

[Icons]
Name: "{group}\Scriptor XV"; Filename: "{app}\scscriptorxv.exe"; WorkingDir: "{app}"; IconFilename: "{app}\scscriptorxv.exe";

