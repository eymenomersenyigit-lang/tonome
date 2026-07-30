#!/usr/bin/env bash
# to[no]ME! live boot customization
set -e

# Create live user
useradd -m -G wheel,audio,video,storage,power -s /bin/bash live
echo "live:live" | chpasswd
echo "root:toor" | chpasswd

# Sudo for live user
echo "live ALL=(ALL) NOPASSWD: ALL" >> /etc/sudoers.d/10-live

# LightDM autologin for live user
cat > /etc/lightdm/lightdm.conf << 'LIGHTDM'
[Seat:*]
autologin-user=live
autologin-user-timeout=0
user-session=tonome
greeter-session=lightdm-gtk-greeter
LIGHTDM

# Installer desktop file for live user
mkdir -p /home/live/.config/autostart
cat > /home/live/.config/autostart/calamares.desktop << 'CALAMARES'
[Desktop Entry]
Type=Application
Name=Install to[no]ME!
Exec=calamares -d
Icon=system-software-install
Terminal=false
Categories=System;
CALAMARES

chown -R live:live /home/live

# Enable services
systemctl enable lightdm
systemctl enable NetworkManager
systemctl enable bluetooth
