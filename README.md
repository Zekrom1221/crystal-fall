<div align="center">

# Crystal Fall

**2D pixel-art arcade survival game made with Unity 6.**<br>
Dodge red crystals, catch green and blue ones, survive as long as you can.

[![Release](https://img.shields.io/github/v/release/Zekrom1221/crystal-fall?style=flat-square&color=2ea44f)](https://github.com/Zekrom1221/crystal-fall/releases/latest)
[![License](https://img.shields.io/github/license/Zekrom1221/crystal-fall?style=flat-square)](LICENSE)
![Unity](https://img.shields.io/badge/Unity-6000.3.8f1-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=dotnet&logoColor=white)
![URP](https://img.shields.io/badge/URP-2D-000000?style=flat-square&logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=flat-square)

[![Download for Windows](https://img.shields.io/badge/Download-Windows%20x64-2ea44f?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Zekrom1221/crystal-fall/releases/latest)

**English** · [Русский](README.ru.md)

[Gameplay](#gameplay) · [Controls](#controls) · [Architecture](#architecture) · [Building](#building-from-source) · [Credits](#credits)

<img src="docs/screenshot.png" alt="Crystal Fall: a wizard stands in a crystal cave while red and green crystals fall from above" width="800">

</div>

> [!NOTE]
> Third-party art and sound effects are not included in this repository: their licenses don't allow redistributing the files. The playable build in [Releases](https://github.com/Zekrom1221/crystal-fall/releases/latest) has everything, and [Credits](#credits) lists where each pack comes from.

## Features

- Three crystal types: red ones hurt, green ones heal, blue ones give a shield
- Difficulty ramps up over the first minute: crystals fall more often and more of them are red
- The spawner uses cooldowns, so the same bonus can't drop again too soon
- Survival timer and a best-time record saved between sessions
- Pause menu, everything is played on the keyboard
- Pixel-art cave with 2D lighting and a parallax background

## About

Solo project, built over several months while learning Unity from scratch. The hardest part was understanding how Unity itself works: components, the MonoBehaviour lifecycle (`Awake`, `Start`, `Update`…) and its API. Along the way I significantly improved my C# and my understanding of game development in general.

## Gameplay

Crystals fall from the ceiling of a cave. Walk into the start zone and press <kbd>E</kbd>: after a 3-2-1 countdown the run begins. Your score is the time you survive.

| | Crystal | Effect |
|:-:|---------|--------|
| 🔴 | Red   | −1 HP (you have 3). After a hit you get 0.5 s of invulnerability with a blink and a camera shake |
| 🟢 | Green | +1 HP, up to the maximum |
| 🔵 | Blue  | Shield for 3 seconds, red crystals don't hurt |

<p align="center">
  <img src="docs/screenshot-hit.png" alt="The wizard flashes red after being hit; one of the three hearts in the corner is empty" width="640"><br>
  <sub>Taking a hit: the wizard flashes red and one heart is gone.</sub>
</p>

### Difficulty curve

The spawn interval goes down linearly from 0.7 s to 0.2 s over the first 60 seconds, so the number of crystals per second grows faster and faster. At the same time the share of red crystals rises from 55% to 80%.

```mermaid
xychart-beta
    title "Crystals per second during a run"
    x-axis "Time since start, s" ["0", "10", "20", "30", "40", "50", "60", "90"]
    y-axis "Crystals per second" 0 --> 6
    bar [1.4, 1.6, 1.9, 2.2, 2.7, 3.5, 5.0, 5.0]
    line [0.8, 1.0, 1.2, 1.5, 2.0, 2.7, 4.0, 4.0]
```

<sub>Bars: all crystals. Line: red crystals by their base weight (cooldown swaps make the real number slightly higher). After 60 s the curve stays flat.</sub>

Bonuses also get rarer: the minimum gap between two green (or two blue) crystals stays at 1.5 s for the first minute and a half, then grows by 1 s per minute, up to 10 s.

The numbers above are the values tuned in the `Game` scene; the defaults in the scripts differ.

## Controls

| Key | Action |
|-----|--------|
| <kbd>A</kbd> <kbd>D</kbd> | Move |
| <kbd>E</kbd> | Start the run (in the start zone), restart after game over, confirm in the pause menu |
| <kbd>Esc</kbd> | Pause / resume |
| <kbd>W</kbd> <kbd>S</kbd> | Navigate the pause menu |

The in-game UI is in Russian.

## Tech stack

| Area | Used |
|------|------|
| Engine | Unity 6 (6000.3.8f1), C# |
| Rendering | Universal Render Pipeline 2D, 2D lights |
| Camera | Cinemachine 2 with a custom `CinemachineExtension` |
| Input | Input System with a generated C# wrapper |
| UI | uGUI + TextMeshPro |

## Architecture

The game is a set of MonoBehaviours wired together in one scene. They talk through direct method calls and C# events.

```mermaid
flowchart TD
    SGT["StartGameTrigger"] -->|"StartGame()"| GM["GameManager"]
    GM -->|"Start/StopSpawning()"| CS["CrystalSpawner"]
    CS -->|"reads curves"| DM["DifficultyManager"]
    CS -->|"spawns"| CR["Crystal"]
    CR -->|"TakeDamage / Heal / ActivateShield"| PH["PlayerHealth"]
    PH -.->|"OnHealthChanged"| HUI["HealthUI"]
    PH -.->|"OnDeath"| GM
    GM -->|"Show(time)"| GO["GameOverUI"]
    GO -->|"TrySetRecord()"| RM[("RecordManager")]
    GM -.->|"OnGameOver"| P["Player"]
```

<sub>Solid arrows are method calls, dotted arrows are C# events. Not shown: `TimerUI`, `RecordHUD`, `PauseManager` and the camera scripts.</sub>

- **GameManager**: singleton with a three-state machine. Starts and stops the spawner, tracks survival time, raises `OnGameStart` / `OnGameOver`.
- **PlayerHealth**: HP, invulnerability frames and shield. Exposes C# events (`OnHealthChanged`, `OnShieldChanged`, `OnDeath`) instead of being polled: `HealthUI` listens to health changes, `GameManager` to death.
- **DifficultyManager**: every difficulty parameter is a function of elapsed time (linear interpolation over `rampDuration`), so balancing is done in the Inspector without touching code.
- **CrystalSpawner**: weighted random choice with cooldowns, see the diagram below.
- **CinemachineClamp**: custom `CinemachineExtension` that keeps the camera inside the level bounds. **ParallaxLayer** handles the multi-layer background.
- **RecordManager**: best time stored in `PlayerPrefs`.
- **BuildScript** (Editor): validates scenes (missing scripts, broken references) and builds the Windows player from the command line.

<details>
<summary><b>Game states</b></summary>

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Idle
    Idle --> Playing: E in start zone, countdown
    Playing --> GameOver: HP reaches 0
    GameOver --> Idle: E, scene reload
    note right of Playing
        Esc pauses the game (timeScale = 0)
        in Idle and Playing
    end note
```

</details>

<details>
<summary><b>What happens when a red crystal hits the player</b></summary>

```mermaid
sequenceDiagram
    autonumber
    participant C as Crystal (red)
    participant PH as PlayerHealth
    participant UI as HealthUI
    participant GM as GameManager
    participant CS as CrystalSpawner
    participant GO as GameOverUI
    participant RM as RecordManager
    Note over C,PH: ignored while shielded or invulnerable
    C->>PH: TakeDamage()
    PH-->>UI: OnHealthChanged(hp)
    Note over PH: sound + camera shake
    alt hp > 0
        PH->>PH: 0.5 s invulnerability, blinking
    else hp == 0
        PH-->>GM: OnDeath
        GM->>CS: StopSpawning()
        GM->>GO: Show(survivalTime)
        GO->>RM: TrySetRecord(time)
        RM-->>GO: new best?
        Note over GM: OnGameOver: Player stops input, RecordHUD refreshes
    end
```

</details>

<details>
<summary><b>How the spawner picks a crystal</b></summary>

```mermaid
flowchart TD
    T["Spawn tick"] --> R{"Weighted roll"}
    R -->|"red share"| RED["Red"]
    R -->|"green share"| GREEN["Green"]
    R -->|"rest"| S{"Shield active?"}
    S -->|"yes"| GREEN
    S -->|"no"| BLUE["Blue"]
    GREEN --> GC{"Green on cooldown?"}
    GC -->|"no"| OG(["spawn Green"])
    GC -->|"yes, shield on"| RED
    GC -->|"yes, no shield"| BLUE
    BLUE --> BC{"Blue on cooldown?"}
    BC -->|"no"| OB(["spawn Blue"])
    BC -->|"yes"| RED
    RED --> OR(["spawn Red"])
```

The red share comes from `DifficultyManager`, green takes 55% of the rest. While the shield is active, no blue crystals spawn.

</details>

<details>
<summary><b>Project structure</b></summary>

```
Assets/
├── Scenes/Game.unity       the only scene
├── Scripts/
│   ├── Camera/             Cinemachine clamp, parallax, camera shake
│   ├── Crystals/           crystal pickup, spawner
│   ├── Gameplay/           game state, difficulty, records, start trigger
│   ├── Input/              Input System actions + generated wrapper
│   ├── Player/             movement, health and shield
│   └── UI/                 HUD, pause menu, game over screen
├── Editor/BuildScript.cs   scene validation + headless build
├── Art/  Animations/  Prefabs/
└── ThirdParty/             not in the repository, see Credits
```

</details>

## Building from source

1. Install **Unity 6000.3.8f1** with *Windows Build Support* via Unity Hub.
2. Clone the repository and open the folder in Unity Hub.
3. Third-party art and audio are not included, so the scripts compile and the scene opens, but sprites and sounds are missing. To restore the visuals, import the packs from [Credits](#credits). Asset Store packages keep their original GUIDs, so scene references should reconnect after import.
4. Build via **Tools → Crystal Fall → Build Windows**. The build goes to `Builds/Windows/`.

> [!TIP]
> The build also runs headless, which is handy for CI:
> ```
> Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
> ```
> Use `-executeMethod BuildScript.ValidateFromCommandLine` to run only the scene check.

## Credits

| Asset | Author | Source |
|-------|--------|--------|
| Evil Wizard 2 (player character) | Luiz Melo | [Unity Asset Store](https://assetstore.unity.com/packages/2d/characters/evil-wizard-2-284501) |
| Pixel Art Gem Pack - Animated | karsiori | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/pixel-art-gem-pack-animated-277559) |
| Fantasy Pixel Trees | EF Studio | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/fantasy-pixel-trees-269458) |
| Free Crystal Cave Pixel Art Backgrounds | CraftPix.net | [craftpix.net](https://craftpix.net/freebies/free-crystal-cave-pixel-art-backgrounds/) |

All four packs are free. Sound effects were made with an online sound effect generator and are not included in the repository.

## License

The code is licensed under [MIT](LICENSE). Third-party assets belong to their authors and are not covered by this license.
