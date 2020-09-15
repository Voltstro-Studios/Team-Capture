![Logo](tc-logo.png)

# Team Capture

Team Capture is a multiplayer first person shooter game inspired by [Quake](https://store.steampowered.com/app/2310/QUAKE/), [Half-Life 2: Deathmatch](https://store.steampowered.com/app/320/HalfLife_2_Deathmatch/), [Team Fortress 2](http://www.teamfortress.com/) and a tf2 mod called [Open Fortress](https://www.openfortress.fun/) (yes, very [Quake engine family](https://commons.wikimedia.org/wiki/File:Quake_-_family_tree.svg) based games).

Team Capture is still in very early development and is developed by a very small team!

Team Capture is built using the [Unity game engine](https://unity.com/) with a modified version of [Mirror](https://mirror-networking.com/) networking..

## Features

Please remember that this project is still in early development!

- In-Game Console
    - With commands! 
- Headless mode
- Working weapon shooting
- Working pickups (Weapons/Health)
- Dynamic settings UI
- Dynamic settings save system
- Discord RPC intergration

## The Team

Here is everyone who is currently working on the project:

* [Voltstro](https://github.com/Voltstro) - *Project Lead*

    - [Email](mailto:me@voltstro.dev) - [Website](https://voltstro.dev)

* [EternalClickbait](https://github.com/EternalClickbait) - *Programmer*

If you think you can help out the team, please don't hesitate to email me (project lead)

## Getting the project

We currently don't offer any prebuilt builds of the project yet.

You will need to build the project yourself to play!

### Prerequisites

```
Unity 2019.4.9f1
Git
Blender 2.83
```

### Pre Setup

Since within the assets of our game we use straight raw Blender files, you will needed to have downloaded and installed [Blender 2.83 LTS](https://www.blender.org/download/lts/), and to make sure `.blend` files are associated with the Blender program.

### Setup

Once you have Blender ready:

1. Fork and clone the project

2. Open the project up in Unity

    - When opening the project for the first time, it can take awhile to open!
    
3. There might be some errors and warnings at first, but should be safe to ignore

4. There seems to be an issue with Blender model's default material not working, re-import the models folder if you are having this issue

5. Build the game, you will need to if you want to test stuff with client and server

### Testing the project

While working on the project, remember that if you alter code that runs on the server you will need to recompile the player build. You will need to also re-build the player build if you alter the scene in any major way.

You can run a server from either the command line with the `startserver` command, start a server from in the in-game 'Create Server' menu, or launch the Team-Capture exe with `-batchmode -nographics`.

Check out the [Command Line Arguments Wiki page](https://github.com/Voltstro/Team-Capture/wiki/Command-Line-Arguments) for more info on the command line arguments in this project.

# License

This project is licensed under the GNU AGPLv3 License - see the [LICENSE](/LICENSE) file for details.

# Q & A

**Q:** When will this project be finished?

**A:** We don't know, it will be a long time, and the team only consists
 of students currently.

---

**Q:** Will this game be free when it comes out?

**A:** Yes! This game and its source code will be completely free when it comes out.

---

**Q:** Why did you use the Unity game engine? Why not engine *x*?

**A:** We used the Unity game engine because it is C#, and the two members of the team are most familiar with C# and Unity.

---

**Q:** I can't program or make assets, is there any other way I can support the project?

**A:** If you want to support the project, and you can't make assets, then you can help by sharing the project. Tell your friends, family or hell, even your dog about the project, and it can massively help us!

# Special Thanks

To these projects:
- [Mirror](https://mirror-networking.com/)
- [Ignorance](https://github.com/SoftwareGuy/Ignorance)
- [Serilog](https://serilog.net/)
- [unity-fastpacedmultiplayer](https://github.com/JoaoBorks/unity-fastpacedmultiplayer)

And to:
- Family
- Friends
- Other fellow students and staff at school for suggestions, ideas and bug hunting.
- And I suppose Unity, for both making an engine that is good but will drive you insane.
