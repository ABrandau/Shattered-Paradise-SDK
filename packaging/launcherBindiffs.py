#!/usr/bin/python3

import os
import sys
import getopt
import urllib.request
from github import Github

system = ''
tag = ''
outputdir = ''
winplatform = ''
repository = ''
packaging_installer_name = ''


def do_bsdiff(url, newfile):
    # gets filename from url
    idx = asset.browser_download_url.rfind('/')
    oldfile = outputdir + asset.browser_download_url[idx:]

    urllib.request.urlretrieve(asset.browser_download_url, oldfile)
    update = newfile + ".update"
    downgrade = newfile + ".downgrade"

    updateCommand = "bsdiff " + oldfile + " " + newfile + " " + update
    downgradeCommand = "bsdiff " + newfile + " " + oldfile + " " + downgrade

    print("Calling bsdiff with \'" + updateCommand + "\'")
    os.system(updateCommand)
    print("Calling bsdiff with \'" + downgradeCommand + "\'")
    os.system(downgradeCommand)

    os.remove(oldfile)
    pass


try:
    opts, args = getopt.getopt(sys.argv[1:], "s:t:o:r:", [
                               "system=", "tag=", "outputdir=", "repository="])
except getopt.GetoptError:
    print("launcherBindiffs.py -s <system> -t <tag> -o <outputdir> -r <repository>")
    sys.exit(2)

for opt, arg in opts:
    print(opt, " - ", arg)
    if opt in ("-s", "--system"):
        system = arg
    elif opt in ("-t", "--tag"):
        tag = arg
    elif opt in ("-o", "--outputdir"):
        outputdir = arg
    elif opt in ("-r", "--repository"):
        repository = arg

with open('mod.config') as file:
    packaging_installer_name = [line for line in file if line.startswith("PACKAGING_INSTALLER_NAME")][0].split('=')[1][1:-2]

print("Getting latest release")
g = Github()
repo = g.get_repo(repository)
assets = repo.get_latest_release().get_assets()

linuxnewfile = outputdir + "/" + packaging_installer_name + "-" + tag + "-x86_64.AppImage"
macoscompatnewfile = outputdir + "/" + packaging_installer_name + "-" + tag + "-compat.dmg"
macosnewfile = outputdir + "/" + packaging_installer_name + "-" + tag + ".dmg"
winx64newfile = outputdir + "/" + packaging_installer_name + "-" + tag + "-" + "x64-winportable.zip"
winx86newfile = outputdir + "/" + packaging_installer_name + "-" + tag + "-" + "x86-winportable.zip"

print("Scanning assets:")
for asset in assets:
    print(asset.browser_download_url)
    if "update" in asset.browser_download_url or "downgrade" in asset.browser_download_url:
        print("Skipping")
        continue

    if system == "linux":
        if "AppImage" in asset.browser_download_url:
            do_bsdiff(asset.browser_download_url, linuxnewfile)
    elif system == "macos":
        if "compat" in asset.browser_download_url:
            do_bsdiff(asset.browser_download_url, macoscompatnewfile)
        elif "compat" not in asset.browser_download_url and "dmg" in asset.browser_download_url :
            do_bsdiff(asset.browser_download_url, macosnewfile)
    else:
        if "x64-winportable" in asset.browser_download_url:
            do_bsdiff(asset.browser_download_url, winx64newfile)
        if "x86-winportable" in asset.browser_download_url:
            do_bsdiff(asset.browser_download_url, winx86newfile)
