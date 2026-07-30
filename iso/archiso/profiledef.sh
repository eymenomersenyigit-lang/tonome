#!/usr/bin/env bash
# Profile definition for to[no]ME! Linux ISO

iso_name="tonome-linux"
iso_label="TONOME_$(date +%Y%m)"
iso_publisher="Tonome Development Team"
iso_application="to[no]ME! Linux Live/Installation Disc"
iso_version="$(date +%Y.%m.%d)"
install_dir="tonome"
buildmodes=('iso')
bootmodes=('bios.syslinux'
           'uefi.grub')
arch="x86_64"
pacman_conf="pacman.conf"
airootfs_image_type="squashfs"
airootfs_image_tool_options=('-comp' 'xz' '-Xbcj' 'x86')
file_permissions=(
  ["/etc/shadow"]="0:0:600"
  ["/etc/gshadow"]="0:0:600"
  ["/root"]="0:0:700"
  ["/root/.automated_script.sh"]="0:0:755"
  ["/usr/local/bin/choose-mirror"]="0:0:755"
  ["/usr/local/bin/Installation_guide"]="0:0:755"
  ["/usr/local/bin/tonome-before-install"]="0:0:755"
)
