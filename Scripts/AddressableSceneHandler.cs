using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;
using Netick.Unity;

namespace Netick.Samples
{
  [AddComponentMenu("Netick/Addressable Scene Handler")]
  public class AddressableSceneHandler : NetworkSceneHandler
  {
    private class AddressableSceneOperation : ISceneOperation
    {
      AsyncOperationHandle<SceneInstance>    Handle;
      bool                   ISceneOperation.IsDone            => Handle.IsDone;
      float                  ISceneOperation.Progress          => Handle.PercentComplete;
      public AddressableSceneOperation(AsyncOperationHandle<SceneInstance> handle)
      {
        Handle = handle;
      }
    }

    public string[]                          AddressableScenes = new string[0];
    public override int                      CustomScenesCount => AddressableScenes != null ? AddressableScenes.Length : 0;

    private Dictionary<string, int>          _keyToIndex;
    private Dictionary<int, string>          _indexToKey;
    private Dictionary<Scene, SceneInstance> _loadedScenes;

    private void Awake()
    {
      _keyToIndex   = new(AddressableScenes.Length);
      _indexToKey   = new(AddressableScenes.Length);
      _loadedScenes = new(AddressableScenes.Length);

      for (int i = 0; i < AddressableScenes.Length; i++)
      {
        _keyToIndex.Add(AddressableScenes[i], i);
        _indexToKey.Add(i, AddressableScenes[i]);
      }
    }

    protected override ISceneOperation LoadCustomSceneAsync(int index, LoadSceneParameters loadSceneParameters, out string sceneName)
    {
      var key   = _indexToKey[index];
      sceneName = key;

      var handle = Addressables.LoadSceneAsync(key, loadSceneParameters);
      handle.Completed += handle =>
      {
        if (handle.Status == AsyncOperationStatus.Succeeded)
          _loadedScenes.Add(handle.Result.Scene, handle.Result);
        else
          Sandbox.LogError($"Addressables.LoadSceneAsync: failed to load an addressable scene {handle.DebugName}");
      };

      return new AddressableSceneOperation(handle);
    }

    protected override ISceneOperation UnloadCustomSceneAsync(Scene scene)
    {
      var didGetSceneInstance = _loadedScenes.TryGetValue(scene, out var sceneInstance);

      if (!didGetSceneInstance || !scene.IsValid())
      {
        Sandbox.LogError($"Unloading scene: couldn't find a scene to unload {scene.name}");
        return null;
      }

      var handle        = Addressables.UnloadSceneAsync(sceneInstance);
      handle.Completed += handle =>
      {
        if (handle.Status == AsyncOperationStatus.Succeeded)
          _loadedScenes.Remove(handle.Result.Scene);
        else
          Sandbox.LogError($"Addressables.UnloadSceneAsync: failed to unload scene {scene.name}");
      };

      return new AddressableSceneOperation(handle);
    }

    // -- Addressable Scenes
    public void LoadAddressableSceneAsync(string key, LoadSceneMode loadSceneMode)
    {
      if (_keyToIndex.TryGetValue(key, out int customIndex))
        Sandbox.LoadCustomSceneAsync(customIndex, new LoadSceneParameters(loadSceneMode, Sandbox.GetDefaultPhysicsMode()));
      else
        Sandbox.LogError("Loading scene: failed to find the addressable scene key in the AddressableScenes array. Make sure to add all scenes keys to the array.");
    }

    public void LoadAddressableSceneAsync(string key, LoadSceneParameters loadSceneParameters)
    {
      if (_keyToIndex.TryGetValue(key, out int customIndex))
        Sandbox.LoadCustomSceneAsync(customIndex, loadSceneParameters);
      else
        Sandbox.LogError("Loading scene: failed to find the addressable scene key in the AddressableScenes array. Make sure to add all scenes keys to the array.");
    }

    public void UnloadAddressableSceneAsync(string key)
    {
      if (_keyToIndex.TryGetValue(key, out int customIndex))
        Sandbox.UnloadSceneAsync(customIndex);
      else
        Sandbox.LogError("Unloading scene: failed to find the addressable scene key in the AddressableScenes array. Make sure to add all scenes keys to the array.");
    }

    public void UnloadAddressableSceneAsync(Scene scene)
    {
      Sandbox.UnloadSceneAsync(scene);
    }

    // -- Build Scenes
    // NetworkSceneHandler already implements exactly the code shown here, the reason we still included it in here is for demonstration purposes.
    protected override ISceneOperation        LoadBuildSceneAsync         (int buildIndex, LoadSceneParameters loadSceneParameters)                         => new BuildSceneOperation(SceneManager.LoadSceneAsync(buildIndex, loadSceneParameters));
    protected override ISceneOperation        UnloadBuildSceneAsync       (Scene scene)                                                                     => new BuildSceneOperation(SceneManager.UnloadSceneAsync(scene));
  }

  public static class AddressableSceneHandlerSandboxExtensions
  {
    /// <summary>
    /// <i><b>[Server Only]</b></i> Loads an addressable scene asynchronously using a key.
    /// </summary>
    public static void LoadAddressableSceneAsync(this NetworkSandbox sandbox, string key, LoadSceneMode loadSceneMode)
    {
      if (sandbox.TryGetComponent<AddressableSceneHandler>(out var defaultSceneHandler))
        defaultSceneHandler.LoadAddressableSceneAsync(key, loadSceneMode);
      else
        sandbox.LogError($"{nameof(AddressableSceneHandler)} is not added to the sandbox. Make sure to add {nameof(AddressableSceneHandler)} to your sandbox prefab.");
    }

    /// <summary>
    /// <i><b>[Server Only]</b></i> Loads an addressable scene asynchronously using a key.
    /// </summary>
    public static void LoadAddressableSceneAsync(this NetworkSandbox sandbox, string key, LoadSceneParameters loadSceneParameters)
    {
      if (sandbox.TryGetComponent<AddressableSceneHandler>(out var defaultSceneHandler))
        defaultSceneHandler.LoadAddressableSceneAsync(key, loadSceneParameters);
      else
        sandbox.LogError($"{nameof(AddressableSceneHandler)} is not added to the sandbox. Make sure to add {nameof(AddressableSceneHandler)} to your sandbox prefab.");
    }

    /// <summary>
    /// <i><b>[Server Only]</b></i> Unloads an addressable scene asynchronously using a key.
    /// </summary>
    public static void UnloadAddressableSceneAsync(this NetworkSandbox sandbox, string key)
    {
      if (sandbox.TryGetComponent<AddressableSceneHandler>(out var defaultSceneHandler))
        defaultSceneHandler.UnloadAddressableSceneAsync(key);
      else
        sandbox.LogError($"{nameof(AddressableSceneHandler)} is not added to the sandbox. Make sure to add {nameof(AddressableSceneHandler)} to your sandbox prefab.");
    }

    /// <summary>
    /// <i><b>[Server Only]</b></i> Unloads an addressable scene asynchronously using a Scene struct.
    /// </summary>
    public static void UnloadAddressableSceneAsync(this NetworkSandbox sandbox, Scene scene)
    {
      if (sandbox.TryGetComponent<AddressableSceneHandler>(out var defaultSceneHandler))
        defaultSceneHandler.UnloadAddressableSceneAsync(scene);
      else
        sandbox.LogError($"{nameof(AddressableSceneHandler)} is not added to the sandbox. Make sure to add {nameof(AddressableSceneHandler)} to your sandbox prefab.");
    }
  }
}