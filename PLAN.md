# to[no]ME! Linux Distro & Tonome Desktop — Kapsamlı Plan

---

## 1. PROJEYE GENEL BAKIŞ

| Başlık | Detay |
|---|---|
| **Distro Adı** | `to[no]ME!` |
| **Masaüstü Ortamı** | `Tonome Desktop` |
| **Temel Sistem** | Arch Linux (pacman, AUR, Arch repos) |
| **UI Framework** | C# (.NET 8/9) ile özel geliştirme (WinUI 3 benzeri XAML benzeri sistem) |
| **Görüntü Sunucusu** | Wayland (özel compositor) |
| **Hedef** | Oyun + Ofis süper performansı, ultra düşük RAM, full özelleştirilebilirlik |

---

## 2. MİMARİ YAPI (KATMANLAR)

```
┌──────────────────────────────────────────────────────┐
│                  KULLANICI UYGULAMALARI               │
│  (Oyunlar, Ofis, Browser, Wine, Steam, VS Code vs.)  │
├──────────────────────────────────────────────────────┤
│              TONOME DESKTOP (C#/.NET)                │
│  ┌─────────┐ ┌──────────┐ ┌──────────────────────┐  │
│  │  Shell  │ │ Compositor│ │   Settings/Controls  │  │
│  │ (Panel, │ │ (Wayland) │ │   (Renk, HDR, Ses)  │  │
│  │  Dash,  │ │           │ │                      │  │
│  │  Launcher)│ │           │ │                      │  │
│  └─────────┘ └──────────┘ └──────────────────────┘  │
│  ┌──────────────────────────────────────────────┐    │
│  │         Tonome UI Framework (C#)             │    │
│  │  (Bitmap render, XAML-like, glass effect,    │    │
│  │   2x/4x/8x anti-aliasing, jelly animasyon)   │    │
│  └──────────────────────────────────────────────┘    │
├──────────────────────────────────────────────────────┤
│              GRAFİK KATMANI                           │
│  ┌─────────┐ ┌──────────┐ ┌──────────────────────┐  │
│  │ Vulkan  │ │ OpenGL   │ │   DRM/KMS            │  │
│  │ (birincil)│ │(yedek/uyum)│ │   (doğrudan donanım) │  │
│  └─────────┘ └──────────┘ └──────────────────────┘  │
├──────────────────────────────────────────────────────┤
│              SİSTEM KATMANI                           │
│  ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐  │
│  │ systemd │ │ PipeWire  │ │ NetworkM │ │ BlueZ  │  │
│  │(boot/bg) │ │(ses/video)│ │(ağ)     │ │(BT)    │  │
│  └─────────┘ └──────────┘ └──────────┘ └────────┘  │
│  ┌─────────┐ ┌──────────┐ ┌──────────────────────┐  │
│  │ Wine   │ │ Vulkan   │ │   Arch Paket Yön.     │  │
│  │(Proton) │ │(icd loader)│ │   (pacman + AUR)   │  │
│  └─────────┘ └──────────┘ └──────────────────────┘  │
├──────────────────────────────────────────────────────┤
│              LİNUX ÇEKİRDEĞİ                          │
│  (Arch Linux kernel + Tonome özel patch'leri)        │
│  - Gaming optimizasyonları (futex, io_uring)         │
│  - HDR desteği (Intel/AMD/NVIDIA)                    │
│  - Düşük gecikme (PREEMPT, tickless)                 │
└──────────────────────────────────────────────────────┘
```

---

## 3. BİLEŞEN DETAYLARI

### 3.1 Tonome Desktop (C# ile yazılacak ana masaüstü)

#### 3.1.1 Tonome UI Framework (Çekirdek UI Kütüphanesi)

| Özellik | Açıklama |
|---|---|
| **Render Motoru** | Vulkan üzerinden bitmap tabanlı render (SkiaSharp benzeri ama C# native) |
| **UI Tanım Dili** | C# markup (XAML benzeri) — `.tonome` uzantılı dosyalar |
| **Glass Efekti** | Vulkan blended layer — arkaplan blur + transparency (Aero benzeri) |
| **Bitmap Elemanlar** | Tüm UI elemanları bitmap katmanlar halinde render edilir |
| **Anti-Pixel** | 2x/4x/8x ölçekleme + supersampling anti-aliasing (oyunlardaki gibi) |
| **Yuvarlak Köşeler** | Native corner radius desteği, tüm pencerelerde varsayılan |
| **Jelly Animasyon** | Spring/fizik tabanlı pencere animasyonları (fluid, elastic) |
| **RAM Profili** | Arka planda <50MB, tam masaüstü <200MB |
| **Tema Sistemi** | Tamamen kullanıcı tarafından değiştirilebilir JSON tabanlı tema |

**Dosya Yapısı:**
```
src/tonome-framework/
├── Tonome.Framework/           # Core framework
│   ├── Rendering/              # Vulkan bitmap renderer
│   │   ├── VulkanRenderer.cs   # Vulkan API bindings
│   │   ├── BitmapCompositor.cs # Bitmap katman birleştirme
│   │   ├── GlassEffect.cs      # Cam efekti (blur + transparency)
│   │   └── AntiPixel.cs        # 2x/4x/8x supersampling
│   ├── Animation/              # Animasyon motoru
│   │   ├── SpringAnimation.cs  # Jelly/spring fizik
│   │   ├── Carousel3D.cs       # 3D carousel (Alt+Tab)
│   │   └── DesktopSwipe3D.cs   # Super+Tab kart görünümü
│   ├── Controls/               # UI kontrolleri
│   │   ├── Button.cs
│   │   ├── TextBox.cs
│   │   ├── Slider.cs
│   │   ├── Window.cs           # Glass, rounded window
│   │   └── Dash.cs             # Ana dash/ dock
│   ├── Theming/                # Tema sistemi
│   │   ├── ThemeManager.cs
│   │   └── DefaultTheme.json
│   └── Input/                  # Giriş yönetimi
│       ├── KeyboardShortcuts.cs  # Super+R, Super+Tab vs.
│       └── Pointer.cs
├── Tonome.Shell/               # Shell (panel, dash, bildirimler)
│   ├── Panel.cs
│   ├── Dash.cs
│   ├── NotificationCenter.cs
│   └── AppLauncher.cs
├── Tonome.Compositor/          # Wayland compositor
│   ├── Compositor.cs           # Ana compositor
│   ├── WindowManager.cs        # Pencere yönetimi
│   ├── WorkspaceManager.cs     # Sanal masaüstleri
│   └── Effects.cs              # Pencere gölgeleri, glass
├── Tonome.Settings/            # Ayarlar uygulaması
│   ├── DisplaySettings.cs      # Renk, HDR, çözünürlük
│   ├── AppearanceSettings.cs   # Tema, glass, corner radius
│   └── InputSettings.cs        # Kısayollar, mouse
└── Tonome.Boot/                # Boot ekranı (Plymouth alternatifi)
    ├── BootScreen.cs           # ASCII art + animasyon
    └── AsciiAssets.cs          # ASCII sanatı ve fontlar
```

---

### 3.2. ANA ÖZELLİKLERİN TEKNİK DETAYLARI

#### 3.2.1 Boot Ekranı (ASCII Art)

```
                  ████████╗ ██████╗     ███╗   ██╗ ██████╗ ███╗   ███╗███████╗
                  ╚══██╔══╝██╔═══██╗    ████╗  ██║██╔═══██╗████╗ ████║██╔════╝
                     ██║   ██║   ██║    ██╔██╗ ██║██║   ██║██╔████╔██║█████╗
                     ██║   ██║   ██║    ██║╚██╗██║██║   ██║██║╚██╔╝██║██╔══╝
                     ██║   ╚██████╔╝    ██║ ╚████║╚██████╔╝██║ ╚═╝ ██║███████╗
                     ╚═╝    ╚═════╝     ╚═╝  ╚═══╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝

                                  ╔═══════════════════════════╗
                                  ║    to[no]ME! v1.0        ║
                                  ╚═══════════════════════════╝


                                  ╭───────────────────────╮
                                  │    ◜◝◞◟◜◝◞◟◜◝◞◟◜◝    │    ← Dönen ASCII halka
                                  ╰───────────────────────╯
```

- **Konum**: Distro adı üst orta, halka alt orta
- **Animasyon**: Halka her karede 1 karakter kayar (◜→◝→◞→◟→◜...)
- **Auto-Scale**: Terminal/ekran boyutunu algıla, ASCII art'ı genişlet/daralt

#### 3.2.2 Alt+Tab — 3D Carousel

```
        ┌───┐
      ┌─┤P2 ├─┐          ← P2 hafif dönük, perspektif
    ┌─┤P1├───┤P3├─┐      ← P1 ve P3 yanlarda
    │ │  │   │   │ │
    │ └──┘   └───┘ │
    │    ┌─────┐    │
    └────┤ P0  ├────┘      ← P0 aktif, tam önde
         │     │
         └─────┘
    [← alt]     [tab →]
```

- Tüm açık pencereler 3D perspektifte dairesel dizilir
- Aktif pencere önde büyük, diğerleri yanlarda perspektifle küçülmüş
- Jelly geçiş efektleri (spring physics)
- Her pencere canlı thumbnail (bitmap snapshot)

#### 3.2.3 Super+Tab — Desktop Switcher (3D Kartlar)

```
          ╔═══════════════════════════════════════╗
          ║    ┌────────┐  ┌────────┐  ┌────────┐║
          ║    │ DESK 1 │  │ DESK 2 │  │ DESK 3 │║  ← 3D açıyla
          ║    │  aktif  │  │        │  │        │║  ← 15° dönük
          ║    │  ico 1  │  │  ico 2  │  │  ico 3  │║
          ║    │  ico 2  │  │  ico 3  │  │  ico 1  │║
          ║    └────────┘  └────────┘  └────────┘║
          ║              ◄═══════►                ║  ← Navigasyon okları
          ╚═══════════════════════════════════════╝
```

- Masaüstü kartları 3D uzayda hafif eğimli dizilir
- Her kartta o masaüstündeki pencerelerin canlı thumbnail'leri
- Geçiş animasyonu: kartlar öne fırlar, arka plan kaybolur
- Büyük ve gösterişli animasyon (kullanıcı talebi)

#### 3.2.4 Pencere Yönetimi & Jelly Animasyonlar

- **Jelly Minimize**: Pencere küçülürken yaylanarak içe çekilir
- **Jelly Kapat**: Pencere sönerek dağılır (blob efekti)
- **Jelly Taşı**: Pencere sürüklenirken kenarlarda esner
- **Glass Kenarlık**: Pencere çerçevesi %100 glass (blur + transparan)
- **Yuvarlak Köşeler**: Tüm pencerelerde native corner radius
- **Gölge**: Dinamik, değişken renkli gölge efekti

#### 3.2.5 Anti-Pixel Sistemi (2x/4x/8x)

```
Normal render:  [████]  ← her piksel net kenarlı
2x SSAA:        [▓▓▓▓]  ← kenarlar yumuşatılmış
4x SSAA:        [████]  ← daha yumuşak
8x SSAA:        [    ]  ← neredeyse vektör kalitesi
```

- UI 2x ölçekte render edilir → 1x'e düşürülür (supersampling)
- 4K monitörde otomatik 4x/8x ölçek
- Zorin OS'teki piksel sorunu tamamen çözülür
- Her UI elemanı bitmap olduğu için SSAA doğal olarak uygulanır

#### 3.2.6 Super+R (Win+R alternatifi)

```
┌──────────────────────────────┐
│ ⚡ Çalıştır  ────────────────│ ← Glass arkaplan
│                              │
│  ┌────────────────────────┐  │
│  │ cmd / komut...        │  │
│  └────────────────────────┘  │
│                              │
│  [Linux Komutu] [Windows]    │
│  [Dosya Yolu] [Web URL]      │
└──────────────────────────────┘
```

- `Super+R` ile açılır
- Komut çalıştırma, dosya yolu, web URL, Windows .exe (Wine ile)
- Son çalıştırılanlar geçmişi

#### 3.2.7 Live Wallpaper Engine

- Video/webm/HTML arkaplan desteği
- Vulkan üzerinden donanım hızlandırmalı oynatma
- Masaüstü simgeleriyle etkileşimli katman
- Performans modu (oyun açıkken otomatik duraklat)
- RAM: <100MB ek yük

#### 3.2.8 Renk Yönetimi & Auto HDR

| Özellik | Detay |
|---|---|
| **Renk Profilleri** | sRGB, DCI-P3, Adobe RGB, Display P3 |
| **Auto HDR** | SDR→HDR dönüşümü (oyunlar için) |
| **Gece Işığı** | Otomatik mavi ışık filtresi (gün batımı) |
| **Renk Sıcaklığı** | 3000K - 9500K arası ayar |
| **Monitör Kalibrasyonu** | ICC profili yükleme + built-in kalibrasyon |
| **Parıltı** | Otomatik ortam ışığı sensörü desteği |

---

### 3.3. SİSTEM BİLEŞENLERİ

#### 3.3.1 Temel Paketler

```
Arch Linux Base
├── linux-tonome (custom kernel)
│   ├── gaming: futex2, io_uring, PREEMPT
│   ├── HDR: Intel/AMD/NVIDIA HDR patches
│   └── lowlatency: tickless, 1000Hz
├── mesa-vulkan-tonome
├── wine-tonome (wine + proton optimizasyonları)
├── pipewire (ses + video capture)
├── networkmanager
├── bluez (bluetooth)
├── vulkan-loader, vulkan-icd-loader
├── sddm-tonome (tema: Tonome glass)
└── gnome-extensions-api (uyumluluk katmanı)
```

#### 3.3.2 GNOME Eklenti Desteği

- GNOME extension API'sine uyumluluk katmanı yazılacak
- Tonome Desktop üzerinde GNOME eklentileri çalıştırılabilir
- JavaScript motoru (QuickJS veya SpiderMonkey) embed edilir
- Eklentiler Tonome'un C# sistemine bridge ile bağlanır

```
┌────────────────────────┐
│   GNOME Extension      │ (JavaScript)
├────────────────────────┤
│   Extension Bridge     │ (C++/Rust, GNOME API taklit)
├────────────────────────┤
│   Tonome JS Engine     │ (QuickJS embed)
├────────────────────────┤
│   Tonome Desktop       │ (C#)
└────────────────────────┘
```

#### 3.3.3 Paket Yöneticisi & Mağaza

```
┌─────────────────────────────────────────┐
│         tonome-store                    │
│  ┌──────────────────────────────────┐   │
│  │ Ara: ________________________   │   │
│  │                                    │   │
│  │  ┌────┐ ┌────┐ ┌────┐ ┌────┐    │   │
│  │  │Oyun │ │Ofis│ │Araç│ │Oyun│    │   │
│  │  │     │ │    │ │    │ │    │    │   │
│  │  └────┘ └────┘ └────┘ └────┘    │   │
│  │                                    │   │
│  │ [Arch] [AUR] [Flatpak] [Snap]   │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

- pacman + AUR tam destek
- Flatpak/Snap seçmeli
- Tonome Store: C# ile yazılmış, glass UI'lı mağaza arayüzü

---

### 3.4. PERFORMANS HEDEFLERİ

| Metrik | Hedef |
|---|---|
| **RAM (arkaplan, boşta)** | <200 MB (Tüm Tonome Desktop) |
| **RAM (arkaplan, minimal)** | <80 MB (sadece shell+compositor) |
| **Boot süresi** | <5 saniye (UEFI + SSD) |
| **Alt+Tab animasyon** | 60 FPS sabit |
| **Pencere açma** | <10ms |
| **Gaming performans** | Proton/Steam'de native Windows'a ±%5 |
| **Ofis performansı** | LibreOffice/Linux-native'de akıcı |

---

## 4. GELİŞTİRME AŞAMALARI

### FAZ 0: Hazırlık (1-2 Hafta)
- [ ] Geliştirme ortamı kurulumu (Arch Linux VM/container)
- [ ] .NET 8/9 SDK kurulumu
- [ ] Vulkan SDK + OpenGL kurulumu
- [ ] Repo yapısının oluşturulması
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Arch Linux ISO build sistemi (archiso)

### FAZ 1: Çekirdek Framework (4-6 Hafta)
- [ ] **Tonome.Framework.Rendering** — Vulkan bitmap renderer
- [ ] **Tonome.Framework.Controls** — Temel UI kontrolleri
- [ ] **Tonome.Framework.Theming** — Tema sistemi
- [ ] **Tonome.Framework.Animation** — Animasyon motoru (spring physics)
- [ ] **Anti-Pixel Sistemi** — 2x/4x/8x SSAA implementasyonu
- [ ] **Tonome.Framework.Input** — Klavye/fare giriş yönetimi

### FAZ 2: Compositor & Window Manager (6-8 Hafta)
- [ ] **Wayland Compositor** (libwayland C bindings + C# wrapper)
- [ ] **Window Manager** — Pencere yönetim protokolleri
- [ ] **Workspace Manager** — Sanal masaüstleri
- [ ] **Glass Efekti** — Blur + transparan katman
- [ ] **Jelly Animasyon** — Pencere minimize/kapat/taşı
- [ ] **3D Carousel** — Alt+Tab görünümü
- [ ] **3D Kart Görünümü** — Super+Tab desktop switcher

### FAZ 3: Shell & Kullanıcı Arayüzü (4-6 Hafta)
- [ ] **Panel** — Sağ üst bildirim alanı, sistem tepsisi
- [ ] **Dash** — Sol alt/orta uygulama çubuğu
- [ ] **App Launcher** — Uygulama başlatıcı (Super tuşu)
- [ ] **Notification Center** — Bildirim yönetimi
- [ ] **Super+R** — Çalıştır penceresi
- [ ] **Live Wallpaper Engine**
- [ ] **Tonome Store** — Paket mağazası

### FAZ 4: Sistem Entegrasyonu (4-6 Hafta)
- [ ] **SDDM Teması** — Tonome glass login
- [ ] **Boot Ekranı** — ASCII art + animasyon
- [ ] **Linux Kernel** — Tonome patch seti
- [ ] **GNOME Extension Desteği** — Bridge katmanı
- [ ] **Wine/Proton** — Entegrasyon + optimizasyon
- [ ] **Renk Yönetimi + HDR**
- [ ] **Vulkan/OpenGL** — İkili destek

### FAZ 5: Paketleme & ISO (2-4 Hafta)
- [ ] **archiso** — Özel ISO yapılandırması
- [ ] **tonome-bootstrap** — Kurulum scripti
- [ ] **Live USB** — Kalıcı depolamalı live ortam
- [ ] **Kurulum** — Arch'a kıyasla çok kolay (GUI installer)
- [ ] **Otomatik donanım algılama** — NVIDIA/AMD/Intel
- [ ] **Otomatik bölümleme** — LVM, Btrfs, LUKS

### FAZ 6: Test & Kararlılık (Sürekli)
- [ ] Unit testler (NUnit/xUnit)
- [ ] Integration testler
- [ ] Donanım uyumluluk testleri (100+ farklı sistem)
- [ ] Gaming benchmark (100+ oyun)
- [ ] Ofis benchmark (LibreOffice, GIMP, VS Code)
- [ ] RAM/CPU profil çıkarma

---

## 5. TEKNOLOJİ YIĞINI

| Bileşen | Teknoloji | Sebep |
|---|---|---|
| **UI Framework** | C# (.NET 9) | XAML benzeri, WinUI 3 benzeri deneyim |
| **Render** | Vulkan 1.3 | Düşük seviye, yüksek performans, HDR |
| **2D Render** | SkiaSharp (Vulkan backend) | Kanıtlanmış, hızlı, anti-aliasing |
| **Compositor** | libwayland + C# FFI | Wayland protokol standardı |
| **Window Manager** | C# custom | Tam kontrol, Tonome'a özel |
| **Animasyon** | Spring physics (C#) | Jelly efekt için fizik motoru |
| **3D Carousel** | Vulkan matrix transform | GPU hızlandırmalı 3D |
| **Bitmap Render** | Vulkan compute shader | UI elemanları bitmap olarak |
| **Glass Effect** | Vulkan subpass + blur | Donanım hızlandırmalı blur |
| **JS Engine** | QuickJS embed | GNOME extension çalıştırmak için |
| **CSS Parser** | C# custom parser | Tema stil tanımı |
| **Boot** | C# native (Plymouth değil) | ASCII art + animasyon |
| **Live Wallpaper** | FFmpeg + Vulkan | Video oynatma + donanım ivme |
| **HDR** | libdisplay-info + Vulkan HDR | Otomatik HDR yönetimi |
| **Gaming API** | DXVK + VKD3D-Proton | DirectX → Vulkan dönüşümü |
| **Wine** | Proton-Tonome (custom fork) | Oyun uyumluluğu |
| **ISO Builder** | archiso | Arch Linux ISO yapısı |
| **Installer** | C# GUI + calamares | Kolay kurulum |
| **Extension Bridge** | C++/Rust + C# interop | GNOME extension API taklidi |

---

## 6. DOSYA YAPISI (KÖK DİZİN)

```
tonome/
├── PLAN.md                          # Bu plan
├── README.md                        # Genel bilgi
├── LICENSE                          # GPLv3 / MIT / seçim
│
├── src/
│   ├── Tonome.Framework/            # Core UI framework
│   ├── Tonome.Shell/                # Masaüstü shell
│   ├── Tonome.Compositor/           # Wayland compositor
│   ├── Tonome.Settings/             # Ayarlar uygulaması
│   ├── Tonome.Boot/                 # Boot ekranı
│   ├── Tonome.Store/                # Uygulama mağazası
│   └── Tonome.ExtensionBridge/      # GNOME extension desteği
│
├── kernel/                          # Linux kernel patches
│   └── patches/
│       ├── 01-hdr-support.patch
│       ├── 02-gaming-optimizations.patch
│       └── 03-lowlatency.patch
│
├── packages/                        # Arch PKGBUILD'ları
│   ├── tonome-desktop/
│   ├── tonome-kernel/
│   ├── tonome-wine/
│   └── tonome-mesa/
│
├── iso/                             # ISO yapılandırması
│   ├── archiso/
│   │   ├── profiledef.sh
│   │   ├── packages.x86_64
│   │   └── pacman.conf
│   ├── boot/
│   │   ├── loader.conf
│   │   └── archiso.conf
│   └── grub/                        # GRUB teması
│       └── theme/
│
├── scripts/                         # Yardımcı scriptler
│   ├── build.sh                     # Tüm projeyi build et
│   ├── build-iso.sh                 # ISO oluştur
│   └── setup-dev.sh                 # Dev ortamı kur
│
├── tests/                           # Testler
│   ├── unit/
│   ├── integration/
│   └── benchmarks/
│
└── docs/                            # Dokümantasyon
    ├── architecture.md
    ├── theming.md
    ├── extension-api.md
    └── development.md
```

---

## 7. ÖNCELİK SIRASI (KRİTİK YOL)

```
1. Tonome.Framework.Rendering (Vulkan bitmap renderer)
   ↓
2. Tonome.Compositor (Wayland compositor + window manager)
   ↓
3. Tonome.Framework.Controls (Temel UI controls)
   ↓
4. Tonome.Shell (Panel + Dash + Launcher)
   ↓
5. Animasyonlar (3D Carousel, Jelly, Desktop Switcher)
   ↓
6. Anti-Pixel Sistemi
   ↓
7. Glass Efekti + Tema Sistemi
   ↓
8. Super+R + Super+Tab
   ↓
9. Live Wallpaper Engine
   ↓
10. Boot Ekranı (ASCII art)
    ↓
11. GNOME Extension Desteği
    ↓
12. Renk Yönetimi + Auto HDR
    ↓
13. Wine/Proton Entegrasyonu
    ↓
14. Tonome Store
    ↓
15. Installer + ISO
    ↓
16. Test & Kararlılık
```

---

## 8. RİSKLER & ÇÖZÜMLER

| Risk | Çözüm |
|---|---|
| **C# ile Wayland compositor** zor olabilir | libwayland C kütüphanesine FFI bindings, kritik kısımlar C/Rust |
| **Vulkan bitmap render** karmaşık | SkiaSharp Vulkan backend kullanılabilir (kanıtlanmış) |
| **GNOME extension uyumluluğu** | GNOME Shell'in JS motoru ve API'sini taklit etme (Mutter'un API dökümanı) |
| **HDR desteği** | Linux'ta HDR hala gelişmekte, kernel + mesa + compositor tarafında koordinasyon |
| **NVIDIA sürücü sorunları** | NVIDIA'nın açık kaynak kernel modülü (nvidia-open) + Wayland desteği |
| **Performans** | Kritik yollar C# unsafe kod veya C/Rust ile yazılır |
| **Donanım uyumluluğu** | Geniş test matrisi (Intel, AMD, NVIDIA, farklı monitörler) |

---

## 9. KISA VADELİ İLK ADIMLAR

```
Hemen başlamak için:

1. Boş bir C# projesi oluştur (dotnet new)
2. Vulkan'ı C#'tan çağırabilmek için Silk.NET veya VulkanSharp ekle
3. Basit bir pencere aç (SDL2/Silk.NET ile)
4. Pencereye bir bitmap render et (bir daire, yuvarlak köşe)
5. Wayland compositor'ın temelini atmaya başla
```

---

**Bu plan canlı bir dokümandır. Geliştirme ilerledikçe güncellenecektir.**
