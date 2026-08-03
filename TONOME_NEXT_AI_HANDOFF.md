# to[no]ME! — Sonraki AI Oturumuna Geçiş Dokümantasyonu

> **Tarih:** 2026-08-03 (güncellendi — 4. oturum)
> **Son commit:** `659f813` (master, GitHub'a push edildi — gerçek cam + boot splash)
> **Depo:** `https://github.com/eymenomersenyigit-lang/tonome`

---

## 1. PROJE HEDEFİ (NE YAPIYORUZ)

Arch Linux tabanlı, **C# (.NET 9) ile yazılmış özel bir masaüstü ortamı** (Tonome Desktop) ve
**GUI kurulumcuyu (Tonome.Installer)** içeren, **GitHub Actions'tan indirilebilir artifact olarak
üretilen canlı bir ISO** inşa etmek.

**Somut başarı kriteri:** ISO'yu Oracle VirtualBox'ta başlatıp **masaüstünün görünmesi** ve
potansiyel yatırımcıya gösterilebilir hale gelmesi.

**Mevcut durum özeti:** ISO build ediliyor ve artifact üretiliyor; **initramfs + switch-root artık ÇALIŞIYOR** (live ortamı boot edip LightDM login ekranına ulaşıyor). Ancak **masaüstüne hâlâ ulaşılamıyor** — kullanıcı şifresini (`live`/`live`) girdikten sonra session anında çöküp **LightDM login ekranına geri atıyor** (login loop). Boot animasyonu da bu yüzden görünmüyor. Teşhis için session log'u var ama **henüz kullanıcıdan alınmadı** (aşağıda 3. bölüm).

---

## 2. TAMAMLANAN İŞLER (COMMIT GEÇMİŞİ + DURUM)

### 2.1 C# Masaüstü Bileşenleri (`src/`)
- `Tonome.Framework` — çekirdek UI framework
- `Tonome.Compositor` — Wayland compositor
- `Tonome.Shell` — panel/dash
- `Tonome.Settings` — ayarlar
- `Tonome.Session` — oturum yöneticisi
- `Tonome.Boot` — boot ekranı
- `Tonome.Installer` — C# GUI kurulumcu (calamares yerine geçti, commit `182a5b6`)
- Hepsini `tonome-desktop` paketinde birleştiren workflow commit `fbd1d14`

### 2.2 Paketleme / ISO Altyapısı
| Commit | Ne yapıldı |
|---|---|
| `80b62e9` | İlk commit: distro foundation |
| `5a30aef` | FAZ 2: compositor, WM, 3D efektler, packaging, ISO build |
| `a632b79` | FAZ 3: shell bileşenleri, installer, live boot altyapısı |
| `dd95744` | CI fix: container çalışma dizini, SKFilterQuality deprecation |
| `cfece27` | CI fix: makepkg bypass, pre-built artifact'lerden `.pkg.tar.zst` üretimi |
| `528dfcf` | Bash fix: `local` keyword fonksiyon dışında kullanılmıştı |
| `4422f2c` | Bash fix: `session_size` aynı sorun |
| `74a27ba` | pkg.tar.zst arşiv içeriği kökte (alt dizin içinde değil) |
| `fa58d89` | syslinux/ grub/ boot config dizinleri, bootmodes fix |
| `7ec56a2` | Container `--privileged` (mkarchiso mount izinleri için) |
| `00babee` | pacman.conf fix (merged community repo), airootfs archiso/ altına, live kullanıcı + autologin |
| `3c6ab53` | profiledef.sh file_permissions temizliği |
| `6d331bb` | packages.x86_64: AUR-only/deprecated paketler çıkarıldı, vulkan-mesa-layers + fastfetch eklendi |

### 2.3 Canlı Boot Hatalarının Düzeltmeleri (KRİTİK SIRA)
| Commit | Sorun | Çözüm |
|---|---|---|
| `a9ec8de` | `.PKGINFO` `./` prefix'liydi | `shopt -s dotglob` + `bsdtar -cf out.pkg.tar.zst *` (bare .PKGINFO) |
| `b74ad5e` | Orphaned `iso/archiso/boot/` | Silindi; syslinux yolları `../boot/` yapıldı; `PROMPT 0` eklendi (autoboot) |
| `85710dd` | Bootloader config'leri yanlış yol | archiso v89 düzeni: `/tonome/boot/x86_64/` → `%INSTALL_DIR%/boot/%ARCH%` |
| `8c99bac` | Switch Root hatası | `mkinitcpio-archiso` paketi eklendi |
| `eb9c80d` | Switch Root hâlâ kırık | `airootfs/etc/mkinitcpio.conf.d/archiso.conf` (HOOKS listesi), `airootfs/etc/mkinitcpio.d/linux.preset` (PRESETS=('archiso')), `mkinitcpio-nfs-utils` eklendi |
| `5bc9b87` | `Mounting '' to '/run/archiso/bootmnt'` (kök neden) | Tüm bootloader APPEND satırlarına `archisosearchuuid=%ARCHISO_UUID%` eklendi; `curl`, `nbd`, `ca-certificates` paketlere eklendi |
| `2ed0b39` | LightDM login loop'un ilk kök nedenleri | `accountsservice` eklendi + `accounts-daemon` enable; `live` kullanıcısı `autologin,nopasswdlogin` gruplarına eklendi; session `wayland-sessions` yerine **`xsessions`** oldu (`/usr/share/xsessions/tonome.desktop`); `xorg-server` eklendi |
| `b63625c` | CI build hatası: `usermod: group 'autologin' does not exist` (exit 6) | `groupadd -f autologin` + `groupadd -f nopasswdlogin` (`customize_airootfs.sh`'te); `nvidia-dkms`/`nvidia-utils` çıkarıldı (DKMS `linux-headers` olmadan fail; VM demo için mesa/llvmpipe yeterli) |
| `dfde4e3` | Session anında crash (login loop'un asıl nedeni) | `SkiaSharp.NativeAssets.Linux` eklendi (eksik `libSkiaSharp.so`), GL fallback zinciri 4.6→4.5→4.3→3.3, X11 session, loglama (`~/tonome-session.log`) |
| `659f813` | Gerçek cam efekt + boot animasyonu | `Glass.cs` (backdrop snapshot + blur), `BootSplash.cs`, `SystemPanel`/`Window`/`Dash` cam'a bağlandı, `Control.DrawGlassBackground` → `Glass.Draw` |

### 2.4 Şu anki commit'lerde neler değişti (4. oturum)

- **`dfde4e3` — session crash düzeltmeleri (KRİTİK):**
  - `Tonome.Framework.csproj`: `SkiaSharp.NativeAssets.Linux` eklendi → publish'te `libSkiaSharp.so` üretiliyor (yerel doğrulandı). Öncesinde sadece Win32 natives referans ediliyordu → Linux'ta ilk satırda crash.
  - `TonomeApplication.cs`: GL version fallback 4.6 → 4.5 → 4.3 → 3.3 (llvmpipe max 4.5), her deneme loglanıyor, pencere exception'ları loglanıyor, `Log()` hem console'a hem `$HOME/tonome-session.log`'a yazıyor.
  - `TonomeRenderer.InitializeSkia()`: `GRGlInterface.Create` / `GRContext.CreateGl` / `SKSurface.Create` null-guard'ları, fb boyutu >= 1 zorlaması.
  - `Program.cs`: try/catch + `AppDomain.UnhandledException` handler + env loglama + render lambda'da tüm UI init.
  - `/usr/bin/tonome-session` wrapper'ı: `XDG_SESSION_TYPE=x11`, `XDG_SESSION_DESKTOP=tonome`, `GTK_USE_PORTAL=1`, `LIBGL_ALWAYS_SOFTWARE=1`, `MESA_GL_VERSION_OVERRIDE=4.5`; **tüm stdout/stderr `>> "${HOME:-/tmp}/tonome-session.log" 2>&1`'a yönleniyor.**
  - Session tipi **X11** (Tonome.Session Silk.NET/GLFW/SDL + OpenGL + Skia pencereli uygulama, gerçek bir Wayland compositor DEĞİL). `xorg-server` paketi ISO'ya eklendi.
- **`659f813` — cam + boot splash:**
  - `src/Tonome.Framework/Rendering/Glass.cs` (YENİ): statik sınıf. `BeginFrame(SKSurface surface, SKCanvas canvas)` wallpaper sonrası, UI öncesi çağrılır → `surface.Snapshot()` alır, matrix'i kaydeder. `Draw`/`DrawPanel` her panel için arkasındaki pikselleri `SKImageFilter.CreateBlur` ile GPU'da bulanıklaştırır, tint + sheen (üstte parlak şerit) + 1px border çizer, yuvarlak köşe path'leriyle. Snapshot alınamazsa düz yarı-saydam fallback. **Not:** cam her zaman wallpaper snapshot'ını bulanıklaştırır, üst üste binen pencereleri DEĞİL (bilinen sınırlama, demo için kabul edildi).
  - `src/Tonome.Shell/BootSplash.cs` (YENİ): 3 saniyelik tam ekran splash — `to[no]ME!` logosu (pulse), `TONOME DESKTOP` alt yazısı, dönen ring + alt tarafta ilerleme çubuğu, fade-in/fade-out. **Session render lambda'sına bağlandı** → session çökerse splash da görünmez (şu an bu durumdayız).
  - `SystemPanel.cs`: `Glass.DrawPanel` (sadece alt köşeler yuvarlak, 16px), renk `(14,14,24)` alpha 195.
  - `Control.cs` `DrawGlassBackground` → `Glass.Draw`'a yönlendirildi (tint `(20,20,34)` alpha 205) → `Window`, `Dash`, `AppDash` otomatik gerçek cam.
  - `TonomeRenderer` → `public SKSurface Surface => _skiaSurface;` eklendi.

---

## 3. AKTİF SORUN — LOGIN LOOP / SESSION ÇÖKMESİ (ÇÖZÜLDÜ)

### 3.0 Belirtiler ve Teşhis
Live ISO'da şifre girildikten sonra login loop (LightDM'ye dönme) sorunu yaşanıyordu.
Kullanıcıdan alınan VirtualBox ekran görüntüsüne göre teşhis konuldu:
- `ls -la /usr/lib/tonome/session/` çıktısında `Tonome.Session` (ve muhtemelen diğer C# native executable'lar) izinleri `-rw-r--r--` olarak görünüyordu (`+x` eksikti).
- `cat tonome-session.log` çıktısı şu hatayı veriyordu: `/usr/bin/tonome-session: line 8: /usr/lib/tonome/session/Tonome.Session: Permission denied`.
- Bu nedenle wrapper script çalışamıyor ve anında exit oluyordu (LightDM login loop).

### 3.1 Çözüm
`.github/workflows/build-iso.yml` dosyasına, paketlemeden hemen önce tüm C# native executable dosyalarına `chmod +x` uygulama adımı eklendi:
```bash
# Ensure all generated Linux native executables have +x permission
chmod +x build/compositor/Tonome.Compositor \
         build/shell/Tonome.Shell \
         build/settings/Tonome.Settings \
         build/session/Tonome.Session \
         build/boot/Tonome.Boot \
         build/installer/Tonome.Installer 2>/dev/null || true
```
Bu sayede `.pkg.tar.zst` paketleri içerisine çalıştırılabilir dosyalar doğru yetkilerle eklenecek.

### 3.2 `dfde4e3`'ün çözdükleri (artık bu nedenler değil)
- Eksik `libSkiaSharp.so` (Win32-only natives referansı) — `SkiaSharp.NativeAssets.Linux` eklendi, linux-x64 publish'te doğrulandı.
- GL 4.6 isteme → llvmpipe max 4.5 olduğu için init crash'i — fallback zinciri eklendi + `MESA_GL_VERSION_OVERRIDE=4.5`.
- Wayland session tipi (gerçek Wayland compositor değil) → X11 session + `xorg-server`.
- Görünürlük (hangisi çöküyordu belli değildi) → loglama eklendi.

### 3.3 `dfde4e3` sonrası hâlâ olası çökme nedenleri (sırayla kontrol et)
1. **`libglfw.so.3` eksik / yanlış loader:** Silk.NET GLFW native'i publish'te var mı kontrol edildi (evet), ama `LD_LIBRARY_PATH` doğru mu? Wrapper `exec` ile çalıştırıyor; .NET self-contained publish dizinindeki natives aynı dizinde olduğundan bulunmalı.
2. **X11 oturum açma sorunu:** LightDM `user-session` X11 olarak başlatıyor; `DISPLAY` env'i greeter'dan geçiyor mu? `xauth` cookie'si live kullanıcısında mı?
3. **GLFW penceresi oluşturulamıyor** (X server'a bağlanamama, GLX yoksa): `libglfw.so.3` → X11 → GLX zinciri. `MESA_GL_VERSION_OVERRIDE=4.5` + `LIBGL_ALWAYS_SOFTWARE=1` set ediliyor; GLFW'nin `GLFW_PLATFORM` denemesi loglarda olmalı.
4. **SkiaSharp GL context (GLX) llvmpipe ile** — `GRGlInterface.Create()` GLX üzerinden; eğer NULL dönerse `dfde4e3`'te guard exception fırlatıyor → log'a düşer. Bu durumda çözüm: GLFW `WindowHint` ile GLX yerine EGL (GLFW_CONTEXT_CREATION_API=EGL) veya doğrudan osmesa.
5. **`Glass.cs` yeni kod:** `surface.Snapshot()` GPU'dan okuma — llvmpipe'ta yavaş ama çökmemeli; `try/catch` içinde. BootSplash `SKFont.MeasureText` — sorun değil. (Yeni commit `659f813` build edildi, çalışma zamanı test edilmedi.)
6. **Pencere boyutu / fullscreen:** GLFW window 1920x1080 isteniyor; VM ekranı daha küçükse pencere stilleri sorun çıkarabilir.

### 3.4 El ile session çalıştırma prosedürü (login loop'u bypass)
TTY'den (Ctrl+Alt+F2, `live`/`live`):
```bash
# X zaten LightDM üzerinde dönüyorsa DISPLAY=:0 kullan
DISPLAY=:0 XDG_SESSION_TYPE=x11 LIBGL_ALWAYS_SOFTWARE=1 MESA_GL_VERSION_OVERRIDE=4.5 \
  /usr/lib/tonome/session/Tonome.Session 2>&1 | tee /home/live/tonome-manual.log
```
Bu, wrapper'ın log yönlendirmesini atlar ve hatayı doğrudan konsolda gösterir. Yine de boşsa, `ldd` ile natives kontrol:
```bash
ldd /usr/lib/tonome/session/libSkiaSharp.so | grep "not found"
ldd /usr/lib/tonome/session/libglfw.so.3 | grep "not found"
```

### 3.5 Boot animasyonu neden yok (iki ayrı düzey)
- **Tonome.Boot ASCII splash (`src/Tonome.Boot/Program.cs`):** hiçbir zaman boot zincirine bağlanmadı — plymouth yerine geçmesi gerekiyordu ama initramfs'te değil, `customize_airootfs.sh`'te de değil. Ayrıca paketi ISO'da (sadece CI publish'inde). Boot zincirine eklenmedi.
- **`BootSplash` (in-session):** session çöktüğü için render edilemiyor. Önce session çözülmeli.

---

## 3.6 ESKİ SORUN (ÇÖZÜLDÜ — arşiv): initramfs switch-root
### 3.6.1 Teşhis Edilen Kök Neden (commit `5bc9b87` ile çözüldü)
Eski APPEND: `archiso_loop_mnt=/run/archiso/bootmnt earlymodules=loop` → **kaynak cihaz parametresi yoktu.**

- initramfs'teki `archiso_loop_mnt` hook'u ISO'nun bulunduğu CD-ROM cihazını `archisosearchuuid` ile bulur.
- `archisosearchuuid=%ARCHISO_UUID%`: mkarchiso build sırasında `iso_uuid` değişkenini yazar.
- `iso_uuid` = `TZ=UTC printf '%(%F-%H-%M-%S-00)T' $SOURCE_DATE_EPOCH` → ör. `2026-07-31-10-48-00-00`.
- xorriso `SOURCE_DATE_EPOCH`'u ISO9660 volume date olarak yazar → `blkid` UUID'si de aynı format → hook `resolve_device UUID=...` ile CD-ROM'u bulur.
- GRUB tarafında releng `%ARCHISO_SEARCH_FILENAME%` (`/boot/<iso_uuid>.uuid` marker dosyası) kullanır; `_make_version()` bunu otomatik oluşturur (mkarchiso kaynak satır 1696).

### 3.6.2 Düzeltilen Dosyalar (`5bc9b87`)
- `iso/archiso/syslinux/{isolinux,syslinux,archiso_sys,archiso_pxe}.cfg` → `APPEND archisobasedir=%INSTALL_DIR% archisosearchuuid=%ARCHISO_UUID%`
- `iso/archiso/grub/{grub.cfg,loopback.cfg}` → aynı parametre linux satırına eklendi
- `iso/archiso/packages.x86_64` → `curl` (archiso_pxe_http hook'u için), `nbd` (archiso_pxe_nbd hook'u → `nbd-client`), `ca-certificates` eklendi

> **NEDEN BU PAKETLER GEREKLİ:** `archiso_pxe_http` install hook'u `add_binary curl`, `archiso_pxe_nbd` install hook'u `add_binary nbd-client` çağırır. Bunlar yoksa **mkinitcpio build aşamasında başarısız olur** (CI ISO aşamasında patlar).

---

## 4. SONRAKİ ADIMLAR (SIRAYLA — GÜNCEL)

> **Login loop (permission denied) sorunu için fix uygulandı (`build-iso.yml`'de `chmod +x`).**

1. Yapılan `chmod +x` değişikliklerini commit ve push et.
2. CI üzerinde GitHub Actions ISO yapımının bitmesini bekle (~1.5 saat).
3. Kullanıcıya yeni üretilen ISO'yu indirip VirtualBox'ta test etmesini söyle.
4. Session sorunsuz başlarsa → masaüstünün ve boot animasyonunun (BootSplash) göründüğünü doğrula.
5. Eğer başka bir nedenle crash olursa, yeni testte tekrar `/home/live/tonome-session.log` dosyasının içeriğine bakarak exception teşhis et.
6. **Boot animasyonu (uzun vadeli):** `Tonome.Boot` ASCII splash'i boot zincirine bağla (plymouth tema ya da customize_airootfs/initramfs aşamasında), aksi halde sadece in-session splash kalacak.

### 4.1 Geçerli config değerleri (doğrulama için)
- `profiledef.sh`: `install_dir="tonome"`, `arch="x86_64"`, bootmodes `bios.syslinux uefi.grub`
- `airootfs/etc/mkinitcpio.conf.d/archiso.conf`:
  `HOOKS=(base udev microcode modconf kms memdisk archiso archiso_loop_mnt archiso_pxe_common archiso_pxe_nbd archiso_pxe_http archiso_pxe_nfs block filesystems keyboard)` + `COMPRESSION="xz"` `COMPRESSION_OPTIONS=(-9e)`
- `airootfs/etc/mkinitcpio.d/linux.preset`:
  `PRESETS=('archiso')`, `ALL_kver='/boot/vmlinuz-linux'`, `archiso_config='/etc/mkinitcpio.conf.d/archiso.conf'`, `archiso_image="/boot/initramfs-linux.img"`
- **ÖNEMLİ:** 2026 `linux` paketi artık `/etc/mkinitcpio.d/linux.preset` içermiyor. airootfs dosyaları pacstrap'tan ÖNCE kopyalandığı için bizim preset'imiz hayatta kalıyor. Initramfs'i tetikleyen hook: `90-mkinitcpio-install.hook` (path trigger `usr/lib/modules/*/vmlinuz`).
- Session wrapper (`build-iso.yml` satır 163): env + log yönlendirme (`~/tonome-session.log`). Desktop dosyası `/usr/share/xsessions/tonome.desktop`, `Exec=/usr/bin/tonome-session`.

---

## 5. OLASI HATALAR & ÇÖZÜMLERİ (GEÇMİŞTEN GELEN BİLGİ)

### 5.1 Yaşanmış hatalar
| Belirti | Neden | Çözüm |
|---|---|---|
| `Switch Root: /new_root not found` | initramfs normal HOOKS ile build edildi (archiso hook'ları yok) | `archiso.conf` + `linux.preset` airootfs'a ekle (commit `eb9c80d`) |
| `Mounting '' to '/run/archiso/bootmnt'` → `ERROR: '' device did not show up after 30 seconds` | APPEND'de `archisosearchuuid` yok; `archisodevice` boş kalıyor | `archisosearchuuid=%ARCHISO_UUID%` ekle (commit `5bc9b87`) |
| `.PKGINFO` bozuk → pacman hata | bsdtar `./` prefix ekliyordu | `shopt -s dotglob` + `*` glob'u |
| bootloader dosyaları bulunamıyor | archiso v89 yeni düzen `%INSTALL_DIR%/boot/%ARCH%/` | config'leri `%INSTALL_DIR%`/`%ARCH%` placeholder'larına çevir |
| Hook `archiso_pxe_nfs` hata | `nfsmount` (/usr/lib/initcpio/nfsmount) eksik | `mkinitcpio-nfs-utils` ekle |
| Hook `archiso_pxe_nbd` build hatası | `nbd-client` eksik | `nbd` paketini ekle |
| Hook `archiso_pxe_http` build hatası | `curl` eksik | `curl` paketini ekle |
| `memdisk` hook build hatası | `memdiskfind` (/usr/bin) eksik | `syslinux` paketi sağlıyor (zaten listede) |

### 5.2 Olası sonraki hatalar (henüz görülmedi, tahmin)
- **`archiso_loop_mnt` mount edemezse:** VirtualBox CD-ROM aygıtının ISO'ya bağlı olduğundan emin olun; ISO'yu SATA CD-ROM olarak takın.
- **Masaüstü açılmaz / siyah ekran / login loop:** `tonome-session` wrapper'ı (`/usr/bin/tonome-session`) çıktıyı **`/home/live/tonome-session.log`**'a yazıyor. Önce bu dosyayı oku, sonra elle `DISPLAY=:0 LIBGL_ALWAYS_SOFTWARE=1 /usr/bin/tonome-session` ile konsol çıktısına bak (bkz. 3.1–3.4). Compositor başarısızsa `journalctl -b` kontrol edin.
- **`customize_airootfs.sh` çalışmazsa:** root shell düşer; script'in çıktısını kontrol edin.
- **GRUB girişinde `initramfs-linux-fallback.img`:** Bu dosya üretilmiyor (sadece `initramfs-linux.img`). "Fallback" menü girişi seçilirse açılmaz — ana girişi seçin. (İstenirse fallback girişi config'lerden silinebilir.)
- **UEFI boot yerine BIOS seçilirse:** `uefi.grub` + `bios.syslinux` ikisi de tanımlı; VirtualBox'ta hangisi denenirse denensin aynı initramfs kullanılır.

---

## 6. ÖNEMLİ ORTAM NOTLARI

- **Kullanıcı şu an canlı ISO'da, aktif test yapıyor.** TTY'den (Ctrl+Alt+F2, `live`/`live`) komut çalıştırabiliyor. Diyalog Türkçe sürüyor.
- **Session log yolu:** `/home/live/tonome-session.log` (wrapper `>> "${HOME:-/tmp}/tonome-session.log" 2>&1`). `cat` bu dosyayı gösterir; boşsa wrapper hiç çalışmadı.
- **Kullanıcının interneti çok yavaş**; GitHub'a erişim aralıklı kesiliyor.
  - Push şu komutla yapılıyor: `cmd /c "git push origin master"` (PowerShell doğrudan git push bazen bağlanamıyor).
- **`gh` CLI mevcut değil** — Actions durumu web arayüzünden izleniyor.
- **CI akışı:** `build` (dotnet publish, 7 proje) → `build-iso` (archlinux:latest + `--privileged` container; pacman paketleri kurar, `create_package()` helper'ı ile `/repo/tonome/x86_64/` yerel repoya paketleri kurar, `repo-add`, sonra `mkarchiso -v -w /work -o /output iso/archiso`) → `release` (yalnızca `main` branch'i).
- **CI build'i makul sürede bitirmek için** ağır paketler yorum satırı: `wine`, `wine-mono`, `wine-gecko`, `steam`, `libreoffice-fresh`, `firefox`, `thunderbird`.
- **mkarchiso kaynak kodu** yerel kopya: `C:\Users\heyme\.local\share\opencode\tool-output\tool_fb7b86f7d001ZRnv6E9ij9lhDY` (1911 satır). Referans satırlar: 1657 (`iso_uuid`), 1696 (`search_filename`), 473/593/674/831 (`%ARCHISO_UUID%` sed).
- **Paket içerik kopyaları (yerel doğrulama için):**
  - `C:\Users\heyme\AppData\Local\Temp\opencode\mkarchiso-pkg` (mkinitcpio-archiso-73 hook'ları)
  - `C:\Users\heyme\AppData\Local\Temp\opencode\mkinitcpio-pkg` (mkinitcpio-41)
- **Referans (releng v89) config'ler** GitHub `archlinux/archiso` v89 `configs/releng/` altında; `syslinux/archiso_sys-linux.cfg` ve `grub/grub.cfg` birebir uyarlama kaynağımızdır.
- **Yerel doğrulama:** `dotnet build src\Tonome.Session -c Release` (0 hata/uyarı) ve `dotnet publish -r linux-x64 --self-contained` → natives kontrol (`libSkiaSharp.so` mevcut). Yerel makine Windows olduğu için çalışma zamanı testi sadece ISO'da yapılabiliyor.

---

## 7. KLASÖR YAPISI (ÖZET)

```
tonome/
├── .github/workflows/build-iso.yml   # CI: build → build-iso → release
├── iso/archiso/
│   ├── profiledef.sh                 # install_dir="tonome", bootmodes bios.syslinux+uefi.grub
│   ├── packages.x86_64               # paket listesi (ağır olanlar yorumda)
│   ├── pacman.conf
│   ├── airootfs/                     # rootfs kopyalanan dosyalar
│   │   ├── etc/mkinitcpio.conf.d/archiso.conf
│   │   ├── etc/mkinitcpio.d/linux.preset
│   │   └── root/customize_airootfs.sh
│   ├── syslinux/{isolinux,syslinux,archiso_sys,archiso_pxe}.cfg
│   └── grub/{grub.cfg,loopback.cfg}
├── packages/                         # PKGBUILD (şu an doğrudan CI'da paket üretiliyor)
├── scripts/
├── src/Tonome.{Framework,Compositor,Shell,Settings,Session,Boot,Installer}
│   ├── Tonome.Framework/Rendering/Glass.cs   # YENİ: gerçek backdrop-blur cam helper
│   ├── Tonome.Framework/Rendering/TonomeRenderer.cs  # Surface özelliği eklendi
│   └── Tonome.Shell/BootSplash.cs            # YENİ: in-session 3 sn boot splash
├── tests/
├── Makefile, Tonome.slnx, PLAN.md    # PLAN.md detaylı mimari plan
└── (bu dosya: Desktop'ta TONOME_NEXT_AI_HANDOFF.md)
```

---

## 8. SON TEST KOMUTU / YÖNTEM

1. En yeni artifact'u GitHub Actions'tan indir (`.iso` dosyası).
2. VirtualBox: Yeni VM → Linux/Arch Linux 64-bit → SATA CD-ROM'a ISO → en az 4GB RAM, 2 CPU → Başlat.
3. Boot menüsünde **"Boot to[no]ME! Linux"** girişini seç.
4. Log'larda `running hook [archiso_loop_mnt]` ve ardından `Mounting <UUID> to '/run/archiso/bootmnt'` başarılı olmalı.
5. Başarı ölçütü: **Tonome masaüstünün (panel + dash + wallpaper) ekranda görünmesi.**

## 9. GELECEK PLAN
Daha kullanıcı odaklı daha fazla araç(task manager,gerçek super+r,hesap makinesi,özelleştire,ayarlar,network,) özellik geliştirme ve daha performanslı olması için wayland dan çıkış(ilk masaüstü ortamı çalışırsa).
Full glass tema ve brkaç goneme eklentisi ve uygulaması ile dolu kullanalabilir bir işletm sistemi.
