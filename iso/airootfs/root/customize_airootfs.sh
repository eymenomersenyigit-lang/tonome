#!/usr/bin/env bash
# to[no]ME! live boot customization
set -e

# Start Calamares installer automatically in live mode
cat > /etc/sddm.conf << 'SDDM'
[Autologin]
User=live
Session=tonome.desktop
Relogin=true
SDDM

# Create live user
useradd -m -G wheel,audio,video,storage,power -s /bin/bash live
echo "live:live" | chpasswd
echo "root:toor" | chpasswd

# Sudo for live user
echo "live ALL=(ALL) NOPASSWD: ALL" >> /etc/sudoers.d/10-live

# Start Calamares on first login
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
