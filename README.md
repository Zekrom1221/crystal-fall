# Crystal Fall

![Unity](https://img.shields.io/badge/Unity-6000.3.8f1-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6)
![License](https://img.shields.io/badge/license-MIT-blue)

2D pixel-art arcade survival game made with Unity 6: dodge red crystals, catch green and blue ones, and survive as long as you can.

Аркада на выживание в пиксель-арте на Unity 6: уворачивайся от красных кристаллов, лови зелёные и синие и продержись как можно дольше.

<!-- ![Gameplay](docs/gameplay.gif) -->

**[Download for Windows / Скачать для Windows](https://github.com/Zekrom1221/crystal-fall/releases/latest)**

[English](#english) · [Русский](#русский)

---

## English

### Gameplay

Crystals fall from the ceiling of a cave. Walk into the start zone and press **E**: after a 3-2-1 countdown the run begins.

| Crystal | Effect |
|---------|--------|
| Red     | −1 HP (you have 3). After a hit you get 0.5 s of invulnerability with a blink and a camera shake |
| Green   | +1 HP, up to the maximum |
| Blue    | Shield for 3 seconds, red crystals don't hurt |

- Difficulty ramps up over the first 60 seconds: crystals spawn more often (every 0.7 s → 0.2 s) and the share of red ones grows (55% → 80%).
- Bonuses get rarer over time: the minimum gap between two green (or two blue) crystals stays at 1.5 s for the first minute and a half, then grows by 1 s per minute, up to 10 s.
- Your score is the survival time. The best time is saved between sessions.

The numbers above are the values tuned in the `Game` scene; the defaults in the scripts differ.

### Controls

| Key     | Action                                  |
|---------|-----------------------------------------|
| A / D   | Move                                    |
| E       | Start / restart / confirm in pause menu |
| Esc     | Pause                                   |
| W / S   | Navigate pause menu                     |

The in-game UI is in Russian.

### Tech stack

Unity 6 (6000.3.8f1) · C# · URP 2D with 2D lights · Cinemachine 2 · Input System (generated C# wrapper) · TextMeshPro

### Architecture

```
 StartGameTrigger --(E, countdown)--> GameManager            state: Idle -> Playing -> GameOver
                                       |        ^
                     Start/StopSpawning|        | OnDeath
                                       v        |
 DifficultyManager <--f(time)--- CrystalSpawner  PlayerHealth --OnHealthChanged--> HealthUI
                                       |          ^
                                spawns |          | TakeDamage / Heal / ActivateShield
                                       v          |
                                    Crystal ------+ (OnTriggerEnter2D)

 GameManager --Show()-----> GameOverUI  (time, best record, restart on E)
 GameManager.OnGameOver --> RecordHUD   (best time from RecordManager / PlayerPrefs)
                        --> Player      (disables input)
```

- **GameManager**: singleton with a three-state machine. Starts and stops the spawner, tracks survival time, raises `OnGameStart` / `OnGameOver`.
- **PlayerHealth**: HP, invulnerability frames and shield. Exposes C# events (`OnHealthChanged`, `OnShieldChanged`, `OnDeath`) instead of being polled: `HealthUI` listens to health changes, `GameManager` to death.
- **DifficultyManager**: every difficulty parameter is a function of elapsed time (linear interpolation over `rampDuration`), so balancing is done in the Inspector without touching code.
- **CrystalSpawner**: weighted random choice with cooldowns. A bonus crystal that would come too soon after the previous one is swapped for another type, and no blue crystals spawn while the shield is active.
- **CinemachineClamp**: custom `CinemachineExtension` that keeps the camera inside the level bounds. **ParallaxLayer** handles the multi-layer background.
- **RecordManager**: best time stored in `PlayerPrefs`.
- **BuildScript** (Editor): validates scenes (missing scripts, broken references) and builds the Windows player from the command line.

### Project structure

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
└── ThirdParty/             not in the repository, see below
```

### Opening the project

1. Install **Unity 6000.3.8f1** with *Windows Build Support* via Unity Hub.
2. Clone the repository and open the folder in Unity Hub.
3. Third-party art and audio are **not included**: their licenses don't allow redistributing the files. The scripts compile and the scene opens, but sprites and sounds will be missing. To restore the visuals, import the packs from [Credits](#credits). Asset Store packages keep their original GUIDs, so scene references should reconnect after import.
4. Build via **Tools → Crystal Fall → Build Windows**, or headless:

   ```
   Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
   ```

   The build goes to `Builds/Windows/`. To run only the scene check, use `-executeMethod BuildScript.ValidateFromCommandLine`.

To just play, download the build from [Releases](https://github.com/Zekrom1221/crystal-fall/releases/latest).

### Credits

| Asset | Author | Source |
|-------|--------|--------|
| Evil Wizard 2 (player character) | Luiz Melo | [Unity Asset Store](https://assetstore.unity.com/packages/2d/characters/evil-wizard-2-284501) |
| Pixel Art Gem Pack - Animated | karsiori | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/pixel-art-gem-pack-animated-277559) |
| Fantasy Pixel Trees | EF Studio | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/fantasy-pixel-trees-269458) |
| Free Crystal Cave Pixel Art Backgrounds | CraftPix.net | [craftpix.net](https://craftpix.net/freebies/free-crystal-cave-pixel-art-backgrounds/) |

All four packs are free. Sound effects were made with an online sound effect generator and are not included in the repository.

### License

The code is licensed under [MIT](LICENSE). Third-party assets belong to their authors and are not covered by this license.

---

## Русский

### Геймплей

С потолка пещеры падают кристаллы. Подойди к зоне старта и нажми **E**: после отсчёта 3-2-1 начинается забег.

| Кристалл | Эффект |
|----------|--------|
| Красный  | −1 HP (всего их 3). После удара 0.5 с неуязвимости с миганием и тряской камеры |
| Зелёный  | +1 HP, но не больше максимума |
| Синий    | Щит на 3 секунды, красные кристаллы не ранят |

- Сложность растёт первые 60 секунд: кристаллы появляются чаще (раз в 0.7 с → 0.2 с), и красных становится больше (55% → 80%).
- Бонусы со временем редеют: минимальный промежуток между двумя зелёными (или двумя синими) первые полторы минуты равен 1.5 с, потом растёт на 1 с за минуту, до 10 с.
- Очки — это время выживания. Лучший результат сохраняется между запусками.

Цифры выше — значения, настроенные в сцене `Game`; дефолты в скриптах другие.

### Управление

| Клавиша | Действие                                   |
|---------|--------------------------------------------|
| A / D   | Движение                                   |
| E       | Старт / рестарт / выбор в меню паузы       |
| Esc     | Пауза                                      |
| W / S   | Навигация по меню паузы                    |

### Стек

Unity 6 (6000.3.8f1) · C# · URP 2D с 2D-освещением · Cinemachine 2 · Input System (сгенерированная C#-обёртка) · TextMeshPro

### Архитектура

Схема — в [английском разделе](#architecture), ниже кратко по классам.

- **GameManager**: синглтон со стейт-машиной из трёх состояний. Запускает и останавливает спавнер, считает время выживания, рассылает `OnGameStart` / `OnGameOver`.
- **PlayerHealth**: здоровье, кадры неуязвимости и щит. Наружу отдаёт C#-события (`OnHealthChanged`, `OnShieldChanged`, `OnDeath`), чтобы его не опрашивали каждый кадр: `HealthUI` слушает изменение здоровья, `GameManager` — смерть.
- **DifficultyManager**: каждый параметр сложности — функция от прошедшего времени (линейная интерполяция за `rampDuration`). Баланс настраивается в инспекторе, без правки кода.
- **CrystalSpawner**: взвешенный случайный выбор с кулдаунами. Если бонус выпадает слишком скоро после предыдущего, он заменяется другим типом, а пока активен щит, синие кристаллы не появляются.
- **CinemachineClamp**: собственное расширение `CinemachineExtension`, которое не выпускает камеру за границы уровня. **ParallaxLayer** отвечает за многослойный фон.
- **RecordManager**: рекорд хранится в `PlayerPrefs`.
- **BuildScript** (Editor): проверяет сцены (потерянные скрипты, битые ссылки) и собирает Windows-билд из командной строки.

### Как открыть проект

1. Установи через Unity Hub **Unity 6000.3.8f1** с модулем *Windows Build Support*.
2. Склонируй репозиторий и открой папку в Unity Hub.
3. Сторонних спрайтов и звуков в репозитории **нет**: их лицензии запрещают выкладывать файлы. Скрипты компилируются и сцена открывается, но без спрайтов и звуков. Чтобы вернуть графику, импортируй паки из раздела [Авторы ассетов](#авторы-ассетов). У пакетов из Asset Store GUID не меняются, поэтому ссылки в сцене должны восстановиться после импорта.
4. Сборка: **Tools → Crystal Fall → Build Windows** или из командной строки:

   ```
   Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
   ```

   Билд появится в `Builds/Windows/`. Только проверка сцены, без сборки: `-executeMethod BuildScript.ValidateFromCommandLine`.

Чтобы просто поиграть, скачай билд из [Releases](https://github.com/Zekrom1221/crystal-fall/releases/latest).

### Авторы ассетов

| Ассет | Автор | Где взять |
|-------|-------|-----------|
| Evil Wizard 2 (персонаж игрока) | Luiz Melo | [Unity Asset Store](https://assetstore.unity.com/packages/2d/characters/evil-wizard-2-284501) |
| Pixel Art Gem Pack - Animated | karsiori | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/pixel-art-gem-pack-animated-277559) |
| Fantasy Pixel Trees | EF Studio | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/fantasy-pixel-trees-269458) |
| Free Crystal Cave Pixel Art Backgrounds | CraftPix.net | [craftpix.net](https://craftpix.net/freebies/free-crystal-cave-pixel-art-backgrounds/) |

Все четыре пака бесплатные. Звуковые эффекты сделаны в онлайн-генераторе звуков и в репозиторий не входят.

### Лицензия

Код распространяется под лицензией [MIT](LICENSE). Сторонние ассеты принадлежат их авторам, и эта лицензия на них не распространяется.
