# Live boot: auto-start Tonome Desktop and then show installer
#!/usr/bin/env bash

# Start Tonome Desktop session (runs tonome-session)
export XDG_SESSION_TYPE=wayland
export XDG_CURRENT_DESKTOP=tonome
export DESKTOP_SESSION=tonome

# Start the desktop environment
exec tonome-session

# After desktop is ready, show the installer
if pgrep -x "Tonome.Session" > /dev/null; then
  # Launch installer in a terminal for simplicity
  sudo -u live bash -c "calamares -d" &
fi
