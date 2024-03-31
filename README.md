# Netick: Networking Engine for Unity [Beta]

![Netick Pic](https://i.ibb.co/QbCLKD3/Netick-Github.png)

[![Manual](https://img.shields.io/badge/docs-manual-informational.svg)](https://netick.net/docs/2/index.html) [![API](https://img.shields.io/badge/docs-api-informational.svg)](https://netick.net/docs/2/api/index.html) [![Discord](https://img.shields.io/badge/discord-grey?logo=discord&logoColor=white)](https://discord.com/invite/uV6bfG66Fx)

Unity integeration of Netick.

Netick is a server-authoritative C#/.NET networking solution for Unity, and, in fact, for all C#-supporting game engines, it's entirely engine-agnostic. Netick is the state-of-the-art networking solution, it's the world's highest-performing networking solution for Unity, achieving the lowest bandwidth and CPU usage, proven by third-party open-source benchmarks. It’s built with an architecture that’s different from other free solutions. Netick makes building quality and cheat-free multiplayer games easier than ever before. Whether it's a competitive CS-like shooter, a 200-player battle royale, or a survival game, Netick can do it. 

## Installation

### Prerequisites
Unity Editor version 2021 or later.

### Steps
- Open the Unity Package Manager by navigating to Window > Package Manager along the top bar.
- Click the plus icon.
- Select Add package from git URL
- Enter https://github.com/karrarrahim/NetickForUnity.git
- Under Packages in the Package Manager, you should now see Netick with it's current version number.
- You can then import one of the samples to get started.


## Features
  * Stable, clean, and powerful API.
  * Client-Side Prediction.
  * Full and Partial Delta Snapshots.
  * Snapshot Interpolation.
  * Remote Procedure Calls (RPCs).
  * Sandboxing
  * Physics Prediction
  * Interest Management [Pro].
  * Lag Compensation [Pro].
  * Code Gen.
  * Zero GC.

And many others.

## Technology and Performance
### Bandwidth
Netick 2 uses an innovative state synchronization algorithm that moves multiplayer game development forward. Not only does it achieve the lowest bandwidth usage ever seen, but it also simplifies networked games by ensuring full networked state update atomicity. Netick 2 uses a novel approach for Delta Snapshots that makes it possible to use Interest Management and Delta Snapshots together, performatively. This has been unheard of in AAA or indie games, due to the difficulty or impracticality of doing that. However, in Netick 2, it just works.

Netick 2 also lets you fully predict anything in the game: network properties, collections, etc. It also lets you easily interpolate anything. This greatly simplifies development for complicated or demanding projects.

![Benchmark](https://i.ibb.co/3cwvNjk/chart-1.png)
[Source](https://github.com/StinkySteak/unity-netcode-benchmark)

### CPU

Netick 2 core is written with unsafe C# code, achieving the highest level of performance. According to simulated testing, it's able to write 1 packet (each ~1000 bytes) to 200 clients in less than 1.4ms, all in a single core (using any modern Intel CPU). This means the networking CPU cost of your game is as little as possible, leaving you room to do other things in the game. It also means you can have more players than before, with higher tickrate.


## Showcase
![Showcase](https://i.ibb.co/gPMHc7G/netick-games2.png)


## Helpful links:
  * Discord: https://discord.com/invite/uV6bfG66Fx
  * Docs: https://www.netick.net/docs.html
  * API: https://netick.net/docs/2/api/index.html
  * Site: http://www.netick.net

If you have any questions, need support, or want to report a bug, visit our discord: 
https://discord.com/invite/uV6bfG66Fx
 
## ❤️ Support
Please consider supporting us on Patreon or here in GitHub so we are able to keep working on and improving Netick!
https://www.patreon.com/user?u=82493081

Enjoy!

Karrar,
Creator of Netick