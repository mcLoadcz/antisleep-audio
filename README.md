# AntiSleepDAC 
**[🇨🇿 Scroll down for Czech version](#česká-verze--czech-version)**

A lightweight, standalone Windows utility designed to prevent USB Audio DACs (Digital-to-Analog Converters) from automatically going to sleep or entering standby mode due to inactivity.

Originally created to solve the Auto Power Down (APD) timeout issue on **Cambridge Audio** devices (like the DacMagic line), it will work for any audio device that incorrectly shuts down or drops its connection when no signal is present.

## How it Works
Some DACs automatically power off or enter a deep standby state if they don't detect an incoming audio signal for a certain period of time (usually 30 minutes). 

**AntiSleepDAC** prevents this by generating and playing exactly **1 second of a completely silent PCM WAV file** every 25 minutes. Rather than playing this sound to your default system audio and potentially mixing with your games or browser, it interfaces directly with the low-level Windows API (`winmm.dll`) to send the silent pulse **specifically to the device you select**.

## Features
* **Device Selection:** Target the silent pulse directly to the DAC, bypassing your default system sound settings.
* **Invisible Audio:** The periodic sound is 100% silent (zeroed out PCM data).
* **GUI & Config:** Easy-to-use Windows interface to select your device which automatically saves to a `config.ini` file.
* **System Tray:** Runs cleanly in the background. Accessible via a small icon in your taskbar tray.

## Usage

### Setup Mode
Simply double-click `AntiSleepDAC.exe`. A window will appear listing all available physical audio outputs on your PC.
1. Select your target audio device (e.g., Cambridge Audio).
2. Click **"Uložit a schovat na pozadí"** (Save and hide to background). 
3. The program will save the device name to `config.ini` and shrink down to a system tray icon, silently keeping your DAC awake.

### Auto-start on Boot
Since the application remembers your device via `config.ini`, you can automate it.
1. Press `Win + R` to open the Run dialog.
2. Type `shell:startup` and hit Enter.
3. Create a shortcut to `AntiSleepDAC.exe` in this folder.
4. Right-click the shortcut, go to **Properties**, and add `--hidden` to the end of the **Target** field.
   *(Example: `"C:\path\to\AntiSleepDAC.exe" --hidden`)*

The program will now start invisibly with Windows and automatically begin protecting your selected device.

## Alternative: Hardware Fix
If you prefer not to use a software workaround, you can often disable the Auto Power Down feature mechanically:
* **DacMagic 200M:** While in standby mode, press and hold the volume knob for 5 seconds until the LEDs blink.
* **DacMagic 100:** While powered on, press and hold the 'Source' button for 5 seconds until the sample rate LEDs flash.

## Compilation
You don't need Visual Studio to compile the application. Using the built-in C# compiler in Windows:
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:AntiSleepDAC.exe /optimize /target:winexe AntiSleepDAC.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
```

---

# Česká verze / Czech version

Lehká samostatná Windows utilita navržená tak, aby zabránila externím USB DAC (Digital-to-Analog) audio převodníkům v automatickém usínání kvůli nečinnosti.

Aplikace prapůvodně vznikla k řešení timeoutu Auto Power Down (APD) u přístrojů **Cambridge Audio** (např. řada DacMagic), nicméně funguje naprosto spolehlivě s jakoukoliv jinou zvukovou kartou, která se samovolně odpojuje, když zrovna nic nehraje.

## Jak to funguje
Některé převodníky se po zhruba 30 minutách ticha bez signálu samy natvrdo vypnou. **AntiSleepDAC** tomuto brání tak, že každých 25 minut vygeneruje a dočasně přehraje **1 sekundu naprosto nulového PCM ticha**. 

Aplikace ke komunikaci využívá nízkoúrovňové jádro přes knihovnu `winmm.dll`. Ticho tedy neposílá do běžného systémového směšovače (kde by hrozila kolize s běžnými zvuky), ale namíří signál zacíleně **pouze do toho hardwarového zařízení, které si vysloveně vyberete**. 

## Vlastnosti aplikce
* **Volba Přehrávacího Zařízení:** Zvuk ticha se posílá exkluzivně jen a pouze na zvolený výstup.
* **Neviditelné Audio:** Tón je garantované 100% ticho (samé nuly) a nedojde k lupnutí.
* **Grafické prostředí a Konfigurace:** Aplikace obsahuje okno s výběrem a pamatuje si nastavení v souboru `config.ini`.
* **System Tray (Lišta oznámení):** Běží čistě na pozadí jako malá ikonka vedle hodin. Tlačítkém pravé myši kontextově vyvoláte navrácení okna i kompletní vypnutí.

## Způsob použití

### Počáteční Nastavení
Dvojklikem spusťte `AntiSleepDAC.exe`. 
1. V seznamu připojených audio zařízení si najděte svůj převodník (např. Cambridge Audio).
2. Klikněte pod ním na tlačítko **Uložit a schovat na pozadí**.
3. Program automaticky uschová váš výběr do souboru `config.ini` hned vedle programu a následně okno zmizí do systémové lišty. Zde zůstane neustále běžet a chránit převodník.

### 100% Bezúdržbové Autospuštění
Abyste to nemuseli zapínat po každém spuštění PC, můžete vše automatizovat:
1. Zmáčkněte `Win + R`, napíšte `shell:startup` a stiskněte Enter. Otevře se složka Po Spuštění.
2. Udělejte si do této složky Zástupce vašeho programu `AntiSleepDAC.exe`.
3. Na zástupce klikněte pravým, vyberte **Vlastnosti** -> **Cíl**, a na samotný konec připište parametr `--hidden`.
   *(Příklad: `"C:\cesta\k\programu\AntiSleepDAC.exe" --hidden`)*

Program nyní po každém startu Windows nenápadně sám naběhne podle uloženého config.ini.

## Alternativa přímo na těle přístroje
U Cambridge Audio je obvykle možné tuhle otravnou ochranu vypnout i manuálně sáhnutím na HW prvky bez softwaru:
* **DacMagic 200M:**  Během standby modu podržte knoflík hlasitosti aspoň 5 sekund; panel dvakrát blikne. 
* **DacMagic 100:** Během zapnutého stavu udržte stisknuté tlačítko "Source" asi 5 sekund dokud neprobliknou lampičky frekvencí napravo.

## Kompilace z podkladových zdrojových kódů
Abyste tuhle C# aplikaci mohli modifikovat, nepotřebujete žádné velké Visual Studio prostředí. Windows má vše už v sobě. Stačí na C# soubor aplikovat tento příkaz ve vestavěné příkazové řádce:
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:AntiSleepDAC.exe /optimize /target:winexe AntiSleepDAC.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
```
