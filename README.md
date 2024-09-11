![Netick Pic](https://i.ibb.co/QbCLKD3/Netick-Github.png)

<p align="center">
  <h3 align="center">High-Performance Networking Engine for Unity</h3>
</p>

<p align="center">
  <a href="https://netick.net/docs/2/articles/getting-started-guide/0-overview.html">Getting Started</a>
  ·
  <a href="https://netick.net/docs/2/articles/understanding-client-server-model.html">Manual</a>
  ·
  <a href="https://netick.net/docs/2/api/index.html">API</a>
  ·
  <a href="https://discord.com/invite/uV6bfG66Fx">Discord</a>
</p>

Netick is a free server-authoritative state-sync networking solution for Unity, and, in fact, for all C#-supporting game engines, it's entirely engine-agnostic. Netick is the state-of-the-art networking solution, it's the highest-performing networking solution for Unity, **achieving the lowest bandwidth and CPU usage, shown by** [**open-source benchmarks.**](#technology-and-performance) Built with an architecture that’s different from other solutions, Netick makes writing cheat-free networked games as close to writing single-player games as possible. Whether it's a competitive CS-like shooter, a 200-player battle royal, or a survival game, Netick can do it.

![Showcase](https://i.ibb.co/JRJvkCr/netick-games3.png)

What makes Netick vastly superior to other networking solutions are four core pillars:

### Simplicity

Netick is **very simple**. The API is intuitive and straightforward. Netick makes writing predicted, high-end, and cheat-free networked games as close to writing single-player games as possible. RPCs are almost completely not used, simplifying everything. It will no longer feel like you are writing a multiplayer game.

### Consistency

While other solutions are plagued by internal de-syncs and race conditions, Netick is completely void of any such issues. Netick ensures the entire networked state of the game is synced atomically and fully together in a tick-aligned manner, eliminating all race conditions and de-syncs. Your game will also behave the same way for good and bad connections. **Netick is the only solution in the market that offers this guarantee**. It is designed from the ground up to satisfy this requirement.

### Exceptional Bandwidth Usage

Netick uses an innovative Delta Snapshots algorithm that achieves the lowest bandwidth usage seen ever, shown by open-source benchmarks. Netick uses 10x higher compression precision than other networking solutions, yet uses less bandwidth.

### Radical CPU Performance

Netick is written with high-performance unmanaged C# code in a data-oriented fashion, achieving an unseen level of performance. Netick is designed to not use any CPU time when something does not move or change. You can have 10K synced objects (non-empty) and use no additional CPU time. Again, Netick is the only solution in the market that does this. This has been referred to as "Dark Magic" by one of our users.

## Installation

### Prerequisites

Unity Editor 2021 or later.

### Steps

- Open the Unity Package Manager by navigating to Window > Package Manager along the top bar.
- Click the plus icon.
- Select Add package from git URL
- Enter https://github.com/NetickNetworking/NetickForUnity.git
- Under Packages in the Package Manager, you should now see Netick with it's current version number.
- You can then import one of the samples to get started. You can do that by going to the `Samples` tab. Or instead, you can follow our [getting started guide](https://netick.net/docs/2/articles/getting-started-guide/0-overview.html) for how to do a simple game with Netick.

## Features

- Stable, clean, and powerful API
- Client-Side Prediction
- Full and Partial Delta Snapshots
- Snapshot Interpolation
- Remote Procedure Calls (RPCs)
- Sandboxing
- Physics Prediction
- Interest Management [Pro]
- Lag Compensation [Pro]
- Code Gen
- Zero GC

And many others.

## Technology and Performance

### Bandwidth

Netick 2 uses an innovative state synchronization algorithm that moves multiplayer game development forward. Not only does it achieve the lowest bandwidth usage ever seen, but it also simplifies networked games by ensuring full/atomic networked state synchronization, completely eliminating all netcode-related race conditions/desyncs, making developing networked games very as close to developing single-player games as possible. Netick 2 uses a novel approach for Delta Snapshots that makes it possible to use Interest Management and Delta Snapshots together, in a performant way. This has been unheard of in AAA or indie games, due to the difficulty or impracticality of doing that. However, in Netick 2, it just works.

Netick lets you fully sync, predict, and interpolate anything in the game: network properties, collections, etc. This greatly simplifies development for complicated or demanding projects.

![Benchmark](https://i.ibb.co/3cwvNjk/chart-1.png)
[Source](https://github.com/StinkySteak/unity-netcode-benchmark)

### CPU

Netick 2 core is written with unsafe C# code, achieving the highest level of performance. According to simulated testing, it's able to write 1 packet (for 100 moving objects) to 200 clients in less than 1.4ms, all in a single core (using any modern Intel CPU). This means the networking CPU cost of your game is as little as possible, leaving you room to do other things in the game. It also means you can have more players than before, with higher tickrate, while requiring less demanding server specs.

## Helpful links:

- Discord: https://discord.com/invite/uV6bfG66Fx
- Docs: https://www.netick.net/docs.html
- API: https://netick.net/docs/2/api/index.html
- Site: http://www.netick.net

If you have any questions, need support, or want to report a bug, visit our discord:
https://discord.com/invite/uV6bfG66Fx

## ❤️ Support

Please consider supporting us on Patreon so we are able to keep working on and improving Netick!
https://www.patreon.com/user?u=82493081

Enjoy!

Karrar,
Creator of Netick
