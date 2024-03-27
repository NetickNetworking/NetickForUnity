# Netick: Networking Engine for Unity

![Netick Pic](https://i.ibb.co/QbCLKD3/Netick-Github.png)

Unity integeration of Netick.

Netick is a free server-authoritative C#/.NET networking solution for Unity, and in fact for all C#-supporting game engines, it's entirely engine-agnostic. It’s by far the most advanced free networking solution on the market. It’s built with an architecture that’s different from other free solutions. Netick makes building quality and cheat-free multiplayer games easier than ever, by implementing most of the features you need to build an FPS/TPS/Real-Time/Action game.


## Features
  * Client-Side Prediction.
  * Full and Partial Delta Snapshots.
  * Snapshot Interpolation.
  * Remote Procedure Calls (RPCs).
  * Physics Prediction
  * Interest Management [Pro].
  * Lag Compensation [Pro].
  * Code Gen.
  * Zero GC.

And many others.

## Technology and Performance

### Bandwidth
Netick 2 uses an innovative state replication algorithm that moves multiplayer game development forward. Not only does it achieve the lowest bandwidth usage ever seen, but it also simplifies networked games by ensuring full networked state update atomicity. Netick 2 uses a novel approach for Delta Snapshots that makes it possible to use Interest Management and Delta Snapshots together, performatively. This has been unheard of in AAA or indie games, due to the difficulty or impracticality of doing that. However, in Netick 2, it just works.

Netick 2 also lets you fully predict anything in the game: network properties, collections, etc. It also lets you easily interpolate anything. This greatly simplifies development for complicated or demanding projects.

Netick 2 aims to solve networking for most types of multiplayer games.

### CPU Usage

Netick 2 core is written with unsafe C# code, achieving the highest level of performance. According to simulated testing, it's able to write 200 packets to 200 clients (sized ~1000 bytes) in less than 1.5ms, all in a single core. This means the networking CPU cost of your game is as little as possible, leaving you room to do other things in the game.

![Benchmark](https://i.ibb.co/3cwvNjk/chart-1.png)
[Source](https://github.com/StinkySteak/unity-netcode-benchmark)


## Showcase
![Showcase](https://i.ibb.co/gPMHc7G/netick-games2.png)


## Helpful links:
  * Discord: https://discord.com/invite/uV6bfG66Fx
  * Docs: https://www.netick.net/docs.html
  * Site: http://www.netick.net

If you have any questions, need support, or want to report a bug, visit our discord: 
https://discord.com/invite/uV6bfG66Fx
 
## Support
Please consider supporting us on Patreon or here in GitHub so we are able to keep working on and improving Netick!
https://www.patreon.com/user?u=82493081

Enjoy!

Karrar,
Creator of Netick