<div align="center">

# Crystal Fall

**Аркада на выживание в пиксель-арте на Unity 6.**<br>
Уворачивайся от красных кристаллов, лови зелёные и синие и продержись как можно дольше.

[![Release](https://img.shields.io/github/v/release/Zekrom1221/crystal-fall?style=flat-square&color=2ea44f)](https://github.com/Zekrom1221/crystal-fall/releases/latest)
[![License](https://img.shields.io/github/license/Zekrom1221/crystal-fall?style=flat-square)](LICENSE)
![Unity](https://img.shields.io/badge/Unity-6000.3.8f1-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=dotnet&logoColor=white)
![URP](https://img.shields.io/badge/URP-2D-000000?style=flat-square&logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=flat-square)

[![Скачать для Windows](https://img.shields.io/badge/%D0%A1%D0%BA%D0%B0%D1%87%D0%B0%D1%82%D1%8C-Windows%20x64-2ea44f?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Zekrom1221/crystal-fall/releases/latest)

[English](README.md) · **Русский**

[Геймплей](#геймплей) · [Управление](#управление) · [Архитектура](#архитектура) · [Сборка](#сборка-из-исходников) · [Авторы ассетов](#авторы-ассетов)

<img src="docs/screenshot.png" alt="Crystal Fall: волшебник стоит в кристальной пещере, сверху падают красный и зелёный кристаллы" width="800">

</div>

> [!NOTE]
> Сторонних спрайтов и звуков в репозитории нет: их лицензии запрещают выкладывать файлы. В билде из [Releases](https://github.com/Zekrom1221/crystal-fall/releases/latest) есть всё, а откуда каждый пак — в разделе [Авторы ассетов](#авторы-ассетов).

## Возможности

- Три вида кристаллов: красные ранят, зелёные лечат, синие дают щит
- Сложность растёт первую минуту: кристаллы падают чаще, и красных становится больше
- Спавнер с кулдаунами: один и тот же бонус не выпадет снова слишком скоро
- Таймер выживания и рекорд, который сохраняется между запусками
- Меню паузы, всё управление с клавиатуры
- Пиксельная пещера с 2D-освещением и параллакс-фоном

## О проекте

Соло-проект: делал его несколько месяцев, изучая Unity с нуля. Сложнее всего было разобраться, как устроен сам Unity: компоненты, жизненный цикл MonoBehaviour (`Awake`, `Start`, `Update`…) и его API. По ходу работы заметно прокачал C# и понимание геймдева в целом.

## Геймплей

С потолка пещеры падают кристаллы. Подойди к зоне старта и нажми <kbd>E</kbd>: после отсчёта 3-2-1 начинается забег. Очки — это время, которое ты продержался.

| | Кристалл | Эффект |
|:-:|----------|--------|
| 🔴 | Красный | −1 HP (всего их 3). После удара 0.5 с неуязвимости с миганием и тряской камеры |
| 🟢 | Зелёный | +1 HP, но не больше максимума |
| 🔵 | Синий   | Щит на 3 секунды, красные кристаллы не ранят |

<p align="center">
  <img src="docs/screenshot-hit.png" alt="Волшебник вспыхивает красным после удара; одно из трёх сердец в углу пустое" width="640"><br>
  <sub>Удар: волшебник вспыхивает красным, одного сердца больше нет.</sub>
</p>

### Рост сложности

Интервал спавна линейно уменьшается с 0.7 до 0.2 с за первые 60 секунд, поэтому кристаллов в секунду становится всё больше и больше. Одновременно доля красных растёт с 55% до 80%.

```mermaid
xychart-beta
    title "Кристаллов в секунду по ходу забега"
    x-axis "Время от старта, с" ["0", "10", "20", "30", "40", "50", "60", "90"]
    y-axis "Кристаллов в секунду" 0 --> 6
    bar [1.4, 1.6, 1.9, 2.2, 2.7, 3.5, 5.0, 5.0]
    line [0.8, 1.0, 1.2, 1.5, 2.0, 2.7, 4.0, 4.0]
```

<sub>Столбцы — все кристаллы. Линия — красные по базовому весу (из-за замен по кулдауну на деле их чуть больше). После 60 с кривая не растёт.</sub>

Бонусы тоже редеют: минимальный промежуток между двумя зелёными (или двумя синими) первые полторы минуты равен 1.5 с, потом растёт на 1 с за минуту, до 10 с.

Цифры выше — значения, настроенные в сцене `Game`; дефолты в скриптах другие.

## Управление

| Клавиша | Действие |
|---------|----------|
| <kbd>A</kbd> <kbd>D</kbd> | Движение |
| <kbd>E</kbd> | Старт забега (в зоне старта), рестарт после проигрыша, выбор в меню паузы |
| <kbd>Esc</kbd> | Пауза / продолжить |
| <kbd>W</kbd> <kbd>S</kbd> | Навигация по меню паузы |

## Стек

| Область | Что используется |
|---------|------------------|
| Движок | Unity 6 (6000.3.8f1), C# |
| Рендер | Universal Render Pipeline 2D, 2D-освещение |
| Камера | Cinemachine 2 с собственным `CinemachineExtension` |
| Ввод | Input System со сгенерированной C#-обёрткой |
| UI | uGUI + TextMeshPro |

## Архитектура

Игра — набор MonoBehaviour-компонентов, связанных в одной сцене. Они общаются прямыми вызовами методов и C#-событиями.

```mermaid
flowchart TD
    SGT["StartGameTrigger"] -->|"StartGame()"| GM["GameManager"]
    GM -->|"Start/StopSpawning()"| CS["CrystalSpawner"]
    CS -->|"читает кривые"| DM["DifficultyManager"]
    CS -->|"создаёт"| CR["Crystal"]
    CR -->|"TakeDamage / Heal / ActivateShield"| PH["PlayerHealth"]
    PH -.->|"OnHealthChanged"| HUI["HealthUI"]
    PH -.->|"OnDeath"| GM
    GM -->|"Show(time)"| GO["GameOverUI"]
    GO -->|"TrySetRecord()"| RM[("RecordManager")]
    GM -.->|"OnGameOver"| P["Player"]
```

<sub>Сплошные стрелки — вызовы методов, пунктирные — C#-события. Не показаны: `TimerUI`, `RecordHUD`, `PauseManager` и скрипты камеры.</sub>

- **GameManager**: синглтон со стейт-машиной из трёх состояний. Запускает и останавливает спавнер, считает время выживания, рассылает `OnGameStart` / `OnGameOver`.
- **PlayerHealth**: здоровье, кадры неуязвимости и щит. Наружу отдаёт C#-события (`OnHealthChanged`, `OnShieldChanged`, `OnDeath`), чтобы его не опрашивали каждый кадр: `HealthUI` слушает изменение здоровья, `GameManager` — смерть.
- **DifficultyManager**: каждый параметр сложности — функция от прошедшего времени (линейная интерполяция за `rampDuration`). Баланс настраивается в инспекторе, без правки кода.
- **CrystalSpawner**: взвешенный случайный выбор с кулдаунами, схема ниже.
- **CinemachineClamp**: собственное расширение `CinemachineExtension`, которое не выпускает камеру за границы уровня. **ParallaxLayer** отвечает за многослойный фон.
- **RecordManager**: рекорд хранится в `PlayerPrefs`.
- **BuildScript** (Editor): проверяет сцены (потерянные скрипты, битые ссылки) и собирает Windows-билд из командной строки.

<details>
<summary><b>Состояния игры</b></summary>

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Idle
    Idle --> Playing: E в зоне старта, отсчёт
    Playing --> GameOver: HP закончились
    GameOver --> Idle: E, перезагрузка сцены
    note right of Playing
        Esc ставит паузу (timeScale = 0)
        в Idle и Playing
    end note
```

</details>

<details>
<summary><b>Что происходит, когда красный кристалл попадает в игрока</b></summary>

```mermaid
sequenceDiagram
    autonumber
    participant C as Crystal (красный)
    participant PH as PlayerHealth
    participant UI as HealthUI
    participant GM as GameManager
    participant CS as CrystalSpawner
    participant GO as GameOverUI
    participant RM as RecordManager
    Note over C,PH: не срабатывает при щите или неуязвимости
    C->>PH: TakeDamage()
    PH-->>UI: OnHealthChanged(hp)
    Note over PH: звук + тряска камеры
    alt hp > 0
        PH->>PH: 0.5 с неуязвимости, мигание
    else hp == 0
        PH-->>GM: OnDeath
        GM->>CS: StopSpawning()
        GM->>GO: Show(survivalTime)
        GO->>RM: TrySetRecord(time)
        RM-->>GO: новый рекорд?
        Note over GM: OnGameOver: Player отключает ввод, RecordHUD обновляется
    end
```

</details>

<details>
<summary><b>Как спавнер выбирает кристалл</b></summary>

```mermaid
flowchart TD
    T["Тик спавна"] --> R{"Взвешенный бросок"}
    R -->|"доля красных"| RED["Красный"]
    R -->|"доля зелёных"| GREEN["Зелёный"]
    R -->|"остаток"| S{"Щит активен?"}
    S -->|"да"| GREEN
    S -->|"нет"| BLUE["Синий"]
    GREEN --> GC{"Зелёный на кулдауне?"}
    GC -->|"нет"| OG(["создать зелёный"])
    GC -->|"да, щит есть"| RED
    GC -->|"да, щита нет"| BLUE
    BLUE --> BC{"Синий на кулдауне?"}
    BC -->|"нет"| OB(["создать синий"])
    BC -->|"да"| RED
    RED --> OR(["создать красный"])
```

Доля красных берётся из `DifficultyManager`, зелёным достаётся 55% от остатка. Пока активен щит, синие кристаллы не появляются.

</details>

<details>
<summary><b>Структура проекта</b></summary>

```
Assets/
├── Scenes/Game.unity       единственная сцена
├── Scripts/
│   ├── Camera/             ограничение Cinemachine, параллакс, тряска камеры
│   ├── Crystals/           подбор кристаллов, спавнер
│   ├── Gameplay/           состояние игры, сложность, рекорды, триггер старта
│   ├── Input/              действия Input System + сгенерированная обёртка
│   ├── Player/             движение, здоровье и щит
│   └── UI/                 HUD, меню паузы, экран проигрыша
├── Editor/BuildScript.cs   проверка сцены + сборка из командной строки
├── Art/  Animations/  Prefabs/
└── ThirdParty/             не в репозитории, см. «Авторы ассетов»
```

</details>

## Сборка из исходников

1. Установи через Unity Hub **Unity 6000.3.8f1** с модулем *Windows Build Support*.
2. Склонируй репозиторий и открой папку в Unity Hub.
3. Сторонних спрайтов и звуков в репозитории нет, поэтому скрипты компилируются и сцена открывается, но без спрайтов и звуков. Чтобы вернуть графику, импортируй паки из раздела [Авторы ассетов](#авторы-ассетов). У пакетов из Asset Store GUID не меняются, поэтому ссылки в сцене должны восстановиться после импорта.
4. Сборка: **Tools → Crystal Fall → Build Windows**. Билд появится в `Builds/Windows/`.

> [!TIP]
> Сборка работает и без интерфейса, например в CI:
> ```
> Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
> ```
> Только проверка сцены, без сборки: `-executeMethod BuildScript.ValidateFromCommandLine`.

## Авторы ассетов

| Ассет | Автор | Где взять |
|-------|-------|-----------|
| Evil Wizard 2 (персонаж игрока) | Luiz Melo | [Unity Asset Store](https://assetstore.unity.com/packages/2d/characters/evil-wizard-2-284501) |
| Pixel Art Gem Pack - Animated | karsiori | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/pixel-art-gem-pack-animated-277559) |
| Fantasy Pixel Trees | EF Studio | [Unity Asset Store](https://assetstore.unity.com/packages/2d/environments/fantasy-pixel-trees-269458) |
| Free Crystal Cave Pixel Art Backgrounds | CraftPix.net | [craftpix.net](https://craftpix.net/freebies/free-crystal-cave-pixel-art-backgrounds/) |

Все четыре пака бесплатные. Звуковые эффекты сделаны в онлайн-генераторе звуков и в репозиторий не входят.

## Лицензия

Код распространяется под лицензией [MIT](LICENSE). Сторонние ассеты принадлежат их авторам, и эта лицензия на них не распространяется.
