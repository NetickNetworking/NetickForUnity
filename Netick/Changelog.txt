### [0.11.8] Changelog - 2024/3/20

- Fixed an issue with generic structs.
- Fixed an issue with NetworkArray<T> interpolation.
- Fixed an issue with non-networked static Rigidbody2D causing Netick to change its state into dynamic.
- Fixed various other issues.

### [0.11.4] Changelog - 2024/3/12 

- Changed the behaviour of OnChanged. Now, by default, it's not invoked during rollback/resims. Now it's only ever invoked when the value is changed during a new state/tick in the client or when it's changed by the server. 
- Changed Sandboxs struct name to LaunchResults.
- Changed Sandboxs.Server to LaunchResults.Servers, and is now an array. To get the first server: launchResults.Servers[0].
- Changed the internal scene management system.
- Objects with Prediction Mode set to Input Source won't be rolled back to server state now. Set Prediction Mode to Everyone for that.
- Added support for additive scene load. To load an additive scene: Sandbox.LoadSceneAsync(index, LoadSceneMode.Additive). To unload an additively loaded scene: Sandbox.UnloadSceneAsync(index).
- Added Network.Launch method for starting Netick. This is a single method with two parameters to start Netick in any kind of configuration.
- Added Host as a starting mode supported by Netick. When starting Netick in Host mode, Sandbox.ConnectedPlayers will include the local server player, when started as a regular server, it won't include the local server player.
- Added Single Player as a starting mode supported by Netick. When starting Netick in Single Player mode, the networking loop (send and receive) won't be updated, and no transport instance will be created.
- Added OnPlayerConnected and OnPlayerDisconnected. These methods respect Host mode, so when starting Netick in host mode, OnPlayerConnected will be invoked for the local server player.
- Added Sandbox.Events. This allows you to subscribe to network events from anywhere, instead of having to use NetworkEvents Listener.
- Improved client performance.
- Fixed an issue with Network Prefab Pool not reusing instances.
- Fixed an issue with GetPreviousValue for compressible float-based types.
- Fixed an issue with auto-calculation of the bounds of a HitShape Container.
- Fixed an issue with Sandbox.OverlapSphere not clearing the hit list.
- Fixed an issue causing RenderInvokeOrder of NetworkRender to always be Update, even when choosing LateUpdate.
- Fixed an issue causing Netick to break in 32-bit systems/mode.
- Fixed an issue with switching scenes directly after starting Netick.
- Fixed an issue causing input types to keep being registered even if not using that input type anymore.
- Fixed an issue causing every pending input in the client to be sent.
- Fixed an issue causing a freeze when switching scenes at high ping.
- Fixed an issue with NetworkBehaviourRef<T> and NetworkObjectRef causing them to not work with null values.
- Fixed an issue causing scene objects and prefabs to be set as dirty even when they have not changed.

- Fixed various other issues.

### [0.10.4] Changelog - 2024/2/14

- Added implementation for NetworkAnimator. This is a component that can be used to sync Mecanim parameters and other state variables.

### [0.10.2] Changelog - 2024/2/8

- Added NetworkLinkedList<T>.
- Added NetworkQueue<T>.
- Added NetworkStack<T>.
- Added NetworkQueueSnapshot<T>, NetworkLinkedListSnapshot<T>, and NetworkStackSnapshot<T> structs with getters for each on OnChangedData. These structs represent the previous state of a network collection.
- Added NetworkObjectRef as a helper struct to sync a reference to a network object.
- Added NetworkBehaviourRef<T> as a helper struct to sync a reference to a network behavior instance.
- Added NetworkTimer as a helper struct to simplify doing a networked timer.
- Added IsAuthortative field to OnChangedData, which can be used to know if the source of the OnChanged event is local or caused by the server.
- Added foreach support to NetworkArray and NetworkArrayStruct variants.
- Added compile-time log errors for some code gen errors.
- Fixed an issue with network string concatenation.
- Fixed an issue causing a NetworkArray OnChanged to be invoked multiple times for the same index in the client.
- Fixed an issue with OnConnectedToServer not being invoked.
- Fixed various other issues.

### [0.9.14] Changelog - 2024/1/31

- Fixed a stutter with Remote Interpolation in the first few seconds after connecting to the server.
- Fixed a stutter with CSP in the first few seconds after connecting to the server.
- Fixed OnConnectedToServer not being invoked in the client.

### [0.9.11] Changelog - 2024/1/27

- Added the ability to associate an input with an index. This makes it possible to use more than one input (up to 10) of the same type per NetworkPlayer, differentiated using the index. This feature enables support for multiple local players all associated with the same NetworkPlayer, but differentiated with the input index. The index has been added as an optional parameter for all input-related methods.
- Fixed an issue when an object tries to destroy itself in NetworkStart.
- Fixed an issue with float Auto Interpolation, causing inaccurate interpolation data to be returned.
- Fixed an issue causing NetworkStart to be invoked before the object has applied its initial network state.
- Fixed an issue causing Interpolation Delay to be high when first connecting.
- Fixed an issue causing a weird behavior when data that needs to be sent exceeds Max Sendable Data Size.
- Fixed various other issues.

### [0.9.7] Changelog - 2024/1/12

- Improved bandwidth usage. 
- Improved serialization performance.
- Added support for adding network behaviors to Sandbox prefab. Now it's possible to have 'global' network behaviors that are accessible with Sandbox.GetComponent<T>().
- Added Sandbox.AuthoritativeTick and Sandbox.PredictedTick. Sandbox.AuthoritativeTick always returns the latest received server tick. Sandbox.PredictedTick returns the last predicted tick on the client, regardless of whether this is a resimulation.
- Changed the location where OnConnectedToServer is invoked from. Now it's not invoked by the transport and instead as part of the internal Netick loop.
- Fixed an issue with Rigidbody.isKinematic not replicating when using NetworkRigidbody.
- Fixed an issue with string network property not replicating correctly.
- Fixed an issue with Unity 2023.
- Fixed various other issues.

### [0.9.5] Changelog - 2023/12/29

- Fixed an issue causing NetworkStart to be invoked on an object in the client before applying its full initial network state. Whatever state you assign in the server for a networked object after spawning it will be available, in full, when NetworkStart is called in the client on that object.
- Fixed an issue causing building the game to fail.

### [0.9.3] Changelog - 2023/12/27

- Improved heavy packet loss handling. Theoretically, It's possible to receive no packets from the server for an entire day (or more) and be back on track after that stops happening. Without needing to send the full networked world state.
- Improved CSP handling of heavy packet loss. Now the client behaves more expectedly when heavy packet loss (no packets for seconds) occurs.
- Improved CSP performance.
- Improved memory usage on the client.
- Added TransportConnection field to NetworkConnection, which can be used to get the transport connection.
- Fixed an issue with NetworkTransform rotation interpolation.
- Fixed various other issues.

### [0.8.11] Changelog - 2023/12/14

- Changed NetworkSandbox.IsVisiable to NetworkSandbox.IsVisible.
- Changed StartNode.ServerAndClient to StartNode.MultiplePeers.
- Added StartAsMultiplePeers to Netick.Unity.Network. This can be used to start multiple clients (and choose to start a server with them or not) in the same Unity instance. Netick.Unity.Network.StartAsServerAndClient is now marked as obsolete.
- Fixed an issue causing NetworkStart to be called after other callbacks of NetworkBehaviour.
- Fixed an issue causing a removed object to still be in the simulation.
- Fixed an issue causing LiteNetLibTransport to throw an exception when a client wants to connect despite the number of connected clients being MaxPlayers.
- Fixed various other issues.

### [0.8.8] Changelog - 2023/11/30

- Changed the behavior of [OnChanged] methods to act similarly to Netick 1. Now [OnChanged] methods are not called for non-default/inspector values, and only called when the property value changes during runtime.
- Changed the default precision value of network properties to 0.001. This should greatly decrease bandwidth usage. If you don't want any compression, pass -1 to precision parameter of [Networked].
- Fixed an issue with Auto Interpolation when passing a valid precision value to [Networked].
- Fixed various other issues.

### [0.8.6] Changelog - 2023/11/19

- Fixed an issue causing [OnChanged] methods on NetworkArray to not work correctly. 
- Fixed an issue causing network state changes to not be detected if they weren't done in NetworkFixedUpdate.

### [0.8.4] Changelog - 2023/11/17

- Fixed a CodeGen issue with parameterless Rpc methods.
- Fixed a CodeGen issue in Unity +2022 causing "Reference has errors".
- Fixed an issue with NetworkArray when used with a type smaller than 4 bytes, like short, causing incorrect elements to be changed when using the array indexer setter.
- Fixed an issue with NetworkString causing the string to be longer than it should be.
- Fixed an issue with IL2CPP causing build to fail.

### [0.8.2] Changelog - 2023/11/16

- Fixed a desync issue that happens with very heavy packet loss (no packets for +2 seconds).

