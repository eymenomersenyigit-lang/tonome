# Tonome Desktop Makefile
# Targets:
#   dev       - Build in debug mode
#   publish   - Publish release for linux-x64
#   packages  - Build Arch Linux packages
#   iso       - Build full ISO (publish + packages + ISO)
#   clean     - Clean build artifacts

.PHONY: dev publish packages iso clean

dev:
	dotnet build

publish:
	./scripts/build.sh publish

packages:
	cd packages/tonome-desktop && makepkg -si --noconfirm
	cd packages/tonome-session && makepkg -si --noconfirm

iso:
	./scripts/build.sh iso

clean:
	dotnet clean
	rm -rf build/
	find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

docker-iso:
	docker build -t tonome-iso -f scripts/Dockerfile.iso .
	docker run --rm -v $(PWD)/build/iso:/output tonome-iso
