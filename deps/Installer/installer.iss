; BracketEngine2 Deps installer script

; Dep checker
#define public Dependency_Path_NetCoreCheck "deps\"
#include "inc\CodeDependencies.iss"

; Variables
#define MyAppName "BracketEngine2 Dependencies"
#define MyAppVersion "1.0"
#define MyAppVerName "{#MyAppName} {#MyAppVersion}"
#define MyAppPublisher "BracketProto"
#define MyAppURL "https://bracketproto.com"

[Setup]
AppId={{623D1E9E-B542-4AA3-BAFB-EE6A535BBFF1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\{#MyAppPublisher}\{#MyAppName}
DisableDirPage=yes
OutputDir=..\
OutputBaseFilename="{#MyAppName} v{#MyAppVersion}"
SetupIconFile=inc\img\setup.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
DisableWelcomePage=false
ArchitecturesInstallIn64BitMode=x64compatible or arm64
PrivilegesRequired=admin
CreateUninstallRegKey=no
Uninstallable=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Setup Images
Source: "inc\img\logo.bmp"; DestDir: "{tmp}"; Flags: dontcopy
Source: "inc\img\BracketProto_small.bmp"; DestDir: "{tmp}"; Flags: dontcopy

[Code]
function PreferArm64Files: Boolean;
begin
  Result := IsArm64;
end;

function PreferX64Files: Boolean;
begin
  Result := not PreferArm64Files and IsX64Compatible;
end;

function PreferX86Files: Boolean;
begin
  Result := not PreferArm64Files and not PreferX64Files;
end;

function CheckInternetConnection: Boolean;
var
  WinHttpReq: Variant;
begin
  Result := False;
  try
    WinHttpReq := CreateOleObject('WinHttp.WinHttpRequest.5.1');
    WinHttpReq.Open('GET', 'https://www.google.com/', False);
    WinHttpReq.Send('');
    if WinHttpReq.Status = 200 then
      Result := True;
  except
    Result := False;
  end;
end;

function SwitchHasValue(Name: string; Value: string): Boolean;
begin
  Result := CompareText(ExpandConstant('{param:' + Name + '}'), Value) = 0;
end;

function InitializeSetup: Boolean;
begin
  #ifdef Dependency_Path_NetCoreCheck
    Dependency_AddDotNet90SDK;
    Dependency_AddVC2013;
  #endif
  Result := True;
end;

procedure AboutButtonOnClick(Sender: TObject);
begin
  MsgBox('Installer made for BracketProto by oxmc', mbInformation, mb_Ok);
end;

procedure WebsiteBtnImageOnClick(Sender: TObject);
var
  ErrorCode: Integer;
begin
  ShellExec('open', 'https://bracketproto.com', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

procedure InitializeWizard;
var
  WebsiteBtnImage : TBitmapImage;
  AboutButton: TNewButton;

begin
  // About Button
  AboutButton := TNewButton.Create(WizardForm);
  AboutButton.Parent := WizardForm;
  AboutButton.Caption := '&About';
  AboutButton.OnClick := @AboutButtonOnClick;

  // Positioning the AboutButton relative to the CancelButton
  AboutButton.Left := WizardForm.ClientWidth - AboutButton.Width - ScaleX(370);
  AboutButton.Top := WizardForm.CancelButton.Top + ScaleY(70);
  
  // ProjectWebsite "Button" image
  WebsiteBtnImage := TBitmapImage.Create(WizardForm);
  WebsiteBtnImage.Parent := WizardForm;
  WebsiteBtnImage.AutoSize := True;
  try
    ExtractTemporaryFile('BracketProto_small.bmp');
    WebsiteBtnImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\BracketProto_small.bmp'));
  except
    MsgBox('Failed to load BracketProto_small image.', mbError, MB_OK);
  end;

  // Positioning the ProjectWebsite "Button" image relative to the CancelButton
  WebsiteBtnImage.Left := WizardForm.ClientWidth - WebsiteBtnImage.Width - ScaleX(460);
  WebsiteBtnImage.Top := WizardForm.CancelButton.Top + ScaleY(65);
  
  WebsiteBtnImage.OnClick := @WebsiteBtnImageOnClick;
end;