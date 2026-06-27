# DNF横版对战Demo | DNF-Style 2D Fighting Game Demo

一个参考DNF风格制作的2D横版对战游戏Demo，使用Unity 6开发。

A 2D side-scrolling fighting game demo inspired by DNF (Dungeon Fighter Online), built with Unity 6.

## 游戏截图 | Screenshot

![游戏截图](screenshot.png)

## 游戏说明 | Description

玩家控制黑色火柴人与紫色火柴人AI敌人进行对战，敌人会自动追踪玩家并发动攻击。

Player controls a black stick figure against a purple AI enemy that automatically tracks and attacks the player.

## 操作方式 | Controls

| 按键 / Key | 功能 / Function |
|-----------|----------------|
| A / D | 左右移动 / Move Left & Right |
| Space | 跳跃 / Jump |
| J | 攻击（连按触发连招）/ Attack (press again for combo) |

## 核心功能 | Features

### 战斗系统 | Combat System
- **连招系统 / Combo System**：Attack1播放至40%-80%窗口期内按J可接Attack2，支持提前按键缓冲 / Press J during 40%-80% of Attack1 to trigger Attack2; early input is buffered
- **HitBox检测 / HitBox Detection**：基于normalizedTime精确控制攻击判定窗口，HashSet防止重复伤害 / Precise hit window via normalizedTime; HashSet prevents duplicate damage
- **HitStop冻帧 / Hit Stop**：命中瞬间压缩timeScale至0.05，持续0.15秒，增强打击感 / On hit, timeScale drops to 0.05 for 0.15s for impactful feel
- **击退系统 / Knockback**：拳击4f，踢腿12f，各自独立可调 / Punch 4f, Kick 12f, individually tunable

### 动画系统 | Animation System
- **玩家动画 / Player**：Idle / Run / Jump / Attack1 / Attack2 / Hurt / Death
- **敌人动画 / Enemy**：Idle / Run / Attack1 / Attack2 / Hurt
- **动画节奏调节 / Speed Tuning**：通过Animator Speed参数精细控制各状态播放速率 / Fine-tuned playback speed per state via Animator Speed

### 敌人AI | Enemy AI
- 5状态状态机：Idle → Chase → Attack → Hurt → Dead / 5-state machine: Idle → Chase → Attack → Hurt → Dead
- 自动追踪玩家，进入攻击范围后发动攻击 / Auto-tracks player and attacks when in range
- 受击硬直（HurtDuration）/ Hurt stun on hit

### 视觉反馈 | Visual Feedback
- 受伤红色闪烁 / Red flash on hurt
- 屏幕闪红（玩家被击中）/ Screen flash when player takes damage
- 死亡灰色渐变消失 / Grey fade-out on death
- 4层视差滚动背景 / 4-layer parallax scrolling background

### 音效 | Audio
- 命中音效（拳/踢各自独立）/ Hit sound (punch & kick separate)
- 出拳风声（动画20%时触发）/ Whoosh sound triggered at 20% of attack animation
- 受伤音效 / Hurt sound

### UI
- 玩家血条左上，敌人血条右上 / Player HP bar top-left, Enemy HP bar top-right
- 事件系统解耦 / Decoupled via C# event system

## 技术亮点 | Technical Highlights

- **Animator状态机 / Animator State Machine**：Has Exit Time精确控制动画过渡 / Precise animation transitions using Has Exit Time
- **C#事件系统 / C# Event System**：血条通过`event Action`订阅血量变化，完全解耦 / Health bar subscribes to HP changes via `event Action`
- **刚体休眠问题 / Rigidbody Sleep Fix**：ImmediateOverlapCheck主动检测解决漏检 / Active overlap check solves missed collisions from sleeping rigidbodies
- **单例模式 / Singleton**：HitStop、ScreenFlash全局单例调用 / Global singleton access for HitStop and ScreenFlash
- **协程 / Coroutine**：击退、HitStop、硬直均使用协程控制时序 / Knockback, HitStop, and stun timing managed via Coroutines

## 开发环境 | Environment

- Unity 6000.2.13f1
- 2D Built-In Render Pipeline
- C#
