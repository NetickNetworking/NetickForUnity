<img width="1581" height="575" alt="NetickGithubMain2" src="https://github.com/user-attachments/assets/4ade3ba8-b2f5-4731-97b2-c12cf8daaba5" />

<p align="center">
  <h3 align="center">High-Performance Networking Engine for Unity</h3>
</p>

<p align="center">
  <a href="https://assetstore.unity.com/packages/tools/network/netick-networking-engine-292096">Asset Store Link</a>
  ·
  <a href="https://netick.net/docs/2/articles/getting-started-guide/0-overview.html">Getting Started</a>
  ·
  <a href="https://netick.net/docs/2/articles/understanding-client-server-model.html">Manual</a>
  ·
  <a href="https://netick.net/docs/2/api/index.html">API</a>
  ·
  <a href="https://discord.com/invite/uV6bfG66Fx">Discord</a>
</p>

Netick is a free, server-authoritative, state-sync networking solution for Unity, built for performance, reliability, and ease of use. It’s a state-of-the-art networking solution, delivering industry-leading bandwidth efficiency and CPU performance, achieving the lowest bandwidth usage, as shown by [open-source benchmarks](https://github.com/StinkySteak/unity-netcode-benchmark). Designed with a fundamentally different architecture from other solutions, Netick makes developing cheat-resistant multiplayer games feel almost as simple as writing single-player games. Whether you’re building a competitive shooter, a casual party game, a 200-player battle royale, or a large-scale survival game, Netick is built to handle it all. It’s also the **only (state-sync) networking solution for Unity with a built-in replay system** and the only free one **proven to handle poor network conditions** - demonstrated [here](https://github.com/StinkySteak/unity-network-library-benchmark-on-bad-network-condition).

<img width="1920" height="1382" alt="NetickGithubShowcase2" src="https://github.com/user-attachments/assets/d6fb3820-9ea4-4836-9842-3a2947f0afd6" />

What makes Netick vastly superior to other networking solutions are four core pillars:

### Simplicity

Netick makes writing high-end, predicted multiplayer games as straightforward as writing single-player ones. Its minimalistic and intuitive API removes the need for almost all RPCs, allowing you to write simpler and easier-to-maintain code.

### Consistency

While other solutions are plagued by internal de-syncs and race conditions, Netick is completely void of such issues. Netick ensures the entire networked state of the game is synced atomically and fully together in a tick-aligned manner, eliminating all race conditions and de-syncs. Your game will also behave the same way for good and bad connections. **Netick is the only solution in the market that offers this guarantee**. It is designed from the ground up to satisfy this requirement.

### Exceptional Bandwidth Usage

Thanks to a cutting-edge Delta Snapshots algorithm, Netick achieves industry-leading bandwidth usage. Open-source benchmarks show Netick syncing more data with less cost—using 10x higher compression precision than other solutions while still requiring less bandwidth.

### Radical CPU Performance

Netick is engineered for performance at scale. Built in high-performance unmanaged C# with a data-oriented approach, Netick achieves an unseen level of performance. You can have 10,000 active objects with zero additional CPU usage. In real-world tests, Netick 2 can compress and write the full transform state of 250 continuously moving objects (syncing position and rotation) to 250 clients in under ~0.35ms on a single core (modern Intel CPU).
One user called it “Dark Magic.” We call it next-gen netcode.

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
- Game Replay
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

Netick 2 uses an innovative state synchronization algorithm that moves multiplayer game development forward. Not only does it achieve the lowest bandwidth usage ever seen, but it also simplifies networked games by ensuring full/atomic networked state synchronization, completely eliminating all netcode-related race conditions/desyncs, making developing networked games as close to developing single-player games as possible. Netick 2 uses a novel approach for Delta Snapshots that makes it possible to use Interest Management and Delta Snapshots together, in a performant way. This has been unheard of in AAA or indie games, due to the difficulty or impracticality of doing that. However, in Netick 2, it just works.

Netick lets you fully sync, predict, and interpolate anything in the game: network properties, collections, etc. This greatly simplifies development for complicated or demanding projects.

![Benchmark](https://i.ibb.co/3cwvNjk/chart-1.png)
[Source](https://github.com/StinkySteak/unity-netcode-benchmark)

### CPU

Netick 2 core is written with unmanaged C# code, utilizing novel algorithms and achieving the highest level of performance. According to simulated testing, **Netick 2 can compress and write 250 continually moving objects (syncing position and rotation) to 250 clients in less than 0.35ms**, all in a single core (using any modern Intel CPU), giving you the best server performance ever seen in the market. This means the networking CPU cost of your game is as little as possible, leaving you room to do other things in the game. It also means you can have more players than before, with a higher tickrate, while requiring less demanding server specs.

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
