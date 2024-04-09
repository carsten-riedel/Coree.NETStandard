#!/bin/bash
#curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 6.0
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 7.0 -Installdir $HOME/.cli -Nopath
#curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 8.0


echo -n 'DOTNET_ROOT=$HOME/.cli' > DOTNETROOT.txt
echo -n '$HOME/.cli/tools' > DOTNETTOOLSPATH.txt

