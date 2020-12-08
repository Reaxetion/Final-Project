#!/bin/bash
repourl=`git remote get-url origin` && len=`expr ${#repourl} - 4` && repository=`echo ${repourl} | cut -c29-$len` && echo $repository >.ucf/.repo
curl https://plato.mrl.ai:8081/git/gaim.vsix -o .ucf/gaim.vsix
code --install-extension ./.ucf/gaim.vsix
rm .ucf/gaim.vsix
