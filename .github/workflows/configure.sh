#!/bin/bash
#curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 6.0
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 7.0 -Installdir $HOME/.cli -Nopath
#curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -channel 8.0

export DOTNET_ROOT=$HOME/.cli
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools'