![Logo](tc-banner.png)

# Team-Capture
[![License](https://img.shields.io/github/license/Voltstro-Studios/Team-Capture.svg)](/LICENSE)
[![CI Build](https://img.shields.io/github/workflow/status/Voltstro-Studios/Team-Capture/Game%20Build/master?label=Build&logo=github)](https://github.com/Voltstro-Studios/Team-Capture/actions)
[![Discord](https://img.shields.io/badge/Discord-Voltstro-7289da.svg?logo=discord)](https://discord.voltstro.dev) 
[![YouTube](https://img.shields.io/badge/Youtube-Voltstro-red.svg?logo=youtube)](https://www.youtube.com/Voltstro)

**This project is in a very early alpha, a lot of things are either broken or buggy, not implemented, or are in the process of being implemented!**

Team-Capture is a multiplayer first person shooter game inspired by [Quake](https://store.steampowered.com/app/2310/QUAKE/), [Half-Life 2: Deathmatch](https://store.steampowered.com/app/320/HalfLife_2_Deathmatch/), [Team Fortress 2](http://www.teamfortress.com/) and a tf2 mod called [Open Fortress](https://www.openfortress.fun/) (aka "Boomer Shooters").

Team-Capture is still in very early development and is developed by a very small team! Expect bugs and other random stuff to occur while playing.

Team-Capture is built using the [Unity game engine](https://unity.com/), using [Mirror](https://mirror-networking.com) as it's netcode.

## Features

Please remember that this project is still in early development!

- In-Game Console
    - With commands!
- Headless/Console mode (Windows/Linux)
- Working weapon shooting
- Working pickups (Weapons/Health)
- Working weapon switching
- Lag Compensation
- Auth Movement
- Dynamic settings UI
- Dynamic settings save system
- Discord RPC integration
- Steam integration
    - Steam Auth
- Server MOTDs (HTML or regular text)
- Offline play
- Cross-platform play (Windows, Linux)
- Well documented API

For a roadmap of what is either being worked on, or planed to come, check out the [projects](https://github.com/Voltstro-Studios/Team-Capture/projects) tab.

## Team

Here is a list of people who work on the project:

* [Voltstro](https://github.com/Voltstro) - *Project Lead*

    - [Email](mailto:me@voltstro.dev) - [Website](https://voltstro.dev)

And here are some people who have worked on the project in the past:

* [EternalClickbait](https://github.com/EternalClickbait) - *Programmer*

* [HelloHowIsItGoing](https://github.com/HelloHowIsItGoing) - *Testing & Ideas*

If you think you can help out the team, please don't hesitate to email me (project lead)

## Getting the project

### Downloads

As this project is in an alpha state, a lot of things will constantly change, so it is recommended to build the project your self. We do also offer automatic ["Nightly" builds (DevOps)](https://nightly.link/Voltstro-Studios/Team-Capture/workflows/main/master).

We also upload [releases](https://github.com/Voltstro-Studios/Team-Capture/releases) every version milestone. The builds may be more stable then the DevOps builds but are still an early alpha and may lack new features and improvements that the DevOps builds have.

### Building

#### Prerequisites

```
Unity 2021.1.20f1
PowerShell Core
Git
```

#### Pre Setup

You may want [PowerShell Core](https://github.com/PowerShell/PowerShell#get-powershell) installed on your system if you want to use the scripts as well as some other features. This is however optional.

#### Setup

Once you have PowerShell ready:

1. Fork and clone the project

2. Open the project up in Unity

    - When opening the project for the first time, it can take awhile to open!

3. There might be some errors and warnings at first, but should be safe to ignore

4. You need to build a player build to play and test, goto Tools **->** Volt Unity Builder **->** Volt Builder **->** Build Player

    - Building for the first time may take awhile (30~ mins), once the cache is built it takes about 90~ secs. This all depends on your computer.

    - We suggest docking Volt Unity Builder somewhere for easy access. Always use this tool to build the game!

#### Testing the project

While working on the project, remember that if you alter code that runs on the server you will need to recompile the player build. You will need to also re-build the player build if you alter the scene in any major way.

You can run a server from either the command line with the `startserver` command, start a server from in the in-game 'Create Server' menu, launch the Team-Capture exe with `-batchmode -nographics`, or via running the PowerShell scripts in the build directory.

You can also test the build from the editor by going to Team Capture **->** Build **->** Launch Player *etc*.

Check out the [Command Line Arguments Wiki page](https://github.com/Voltstro-Studios/Team-Capture/wiki/Command-Line-Arguments) for more info on the command line arguments in this project.

### Platform Support

Team-Capture plans to support most large x64-bit based desktop platforms.

|OS             |Client|Server|Notes                                                                                                                            |
|---------------|------|------|---------------------------------------------------------------------------------------------------------------------------------|
|Windows (x64)  |✔     |✔     |Native.                                                                                                                          |
|GNU/Linux (x64)|✔     |✔     |Native. Requires [VA-API support](https://github.com/Voltstro-Studios/UnityWebBrowser#enable-va-api-support) for some components.|
|macOS (x64)    |✖     |✖     |No physical hardware to test or develop on.                                                                                      |

# License

This project is licensed under the GNU AGPLv3 License - see the [LICENSE](/LICENSE) file for details.

See [`thirdpartylegalcode.txt`](/thirdpartylegalcode.txt) and [`thirdpartylegalassets.txt`](/thirdpartylegalassets.txt) for third-party assets used.

# Q & A

**Q:** Is this a serious project?

**A:** Nah, this is just a hobby project that was originally started as a way to learn networking in Unity.

---

**Q:** Will this game be free when it comes out?

**A:** Team-Capture will always remain free and open-source.

---

**Q:** Why did you use the Unity game engine? Why not engine *x*?

**A:** At the start of this project, Unity seemed link an obvious choice, it was C# and we knew how to use it. Sometimes I regret that choice of sticking with it.

---

**Q:** Why not use [MLAPI](hhttps://github.com/Unity-Technologies/com.unity.netcode.gameobjects)?

**A:** When we started planning for this project, MLAPI was not apart of the Unity ecosystem. On top of that, at the time, MLAPI had really bad documentation, and a lack of community, so Mirror was the obvious chose. Now we are too far into development to change, not that we would.

# Special Thanks

To these projects:
- [Mirror](https://mirror-networking.com/) - Networking Code
- [Serilog](https://serilog.net/) - Logger
- [FPSSample](https://github.com/Unity-Technologies/FPSSample) - Lots of code design inspiration, console backbone code.

And to:
- Family
- Friends
- Other fellow students and staff at school for suggestions, ideas and bug hunting.
- And I suppose Unity, for both making an engine that is good but will drive you insane.

# Links

- [Website](https://tc.voltstro.dev)
- [Source Code Repo](https://github.com/Voltstro-Studios/Team-Capture)
- [Assets Source Repo](https://gitlab.com/Voltstro-Studios/TC/Team-Capture-Assets)