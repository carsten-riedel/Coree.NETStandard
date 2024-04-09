REM powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -channel 6.0 -Installdir $HOME\cli -Nopath"
powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -channel 7.0 -Installdir $HOME\.cli -Nopath"
REM powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -channel 8.0 -Installdir $HOME\cli -Nopath"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "[System.Environment]::SetEnvironmentVariable('DOTNET_ROOT', \"$HOME\cli\", [System.EnvironmentVariableTarget]::User)"
powershell -NoProfile -ExecutionPolicy Unrestricted -Command "$path = [System.Environment]::GetEnvironmentVariable('PATH', [System.EnvironmentVariableTarget]::User); $newPath = $path + \";$HOME\.cli;$HOME\.cli\tools\""; [System.Environment]::SetEnvironmentVariable('PATH', $newPath, [System.EnvironmentVariableTarget]::User)"


