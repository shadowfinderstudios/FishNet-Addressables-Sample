using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

using System.Collections;
using System.Collections.Generic;

using FishNet;
using FishNet.Object;
using FishNet.Managing.Scened;
using FishNet.Managing;
using FishNet.Managing.Object;

using GameKit.Dependencies.Utilities;
using System;

[Serializable]
public class AddressablePackage
{
    public string key;
    public string[] value;
}

[Serializable]
public class NetSceneProcessor : DefaultSceneProcessor
{
    /// <summary>
    /// List of addressable packages and their addressables to load.
    /// </summary>
    public List<AddressablePackage> _addressablePackages = new();

    /// <summary>
    /// Whether addressables have been loaded.
    /// </summary>
    public bool _addressablesAreLoaded;

    /// <summary>
    /// Reference to your NetworkManager.
    /// </summary>
    private NetworkManager _networkManager => InstanceFinder.NetworkManager;

    /// <summary>
    /// Used to load and unload addressables in async.
    /// </summary>
    private AsyncOperationHandle<IList<GameObject>> _asyncHandle;

    /// <summary>
    /// Loads an addressables package by string.
    /// </summary>
    public IEnumerator LoadAddressables(AsyncOperation ao)
    {
        foreach (var addressablePackageGroup in _addressablePackages)
        {
            ushort id = addressablePackageGroup.key.GetStableHashU16();
            var spawnables = (SinglePrefabObjects)_networkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
            var cache = CollectionCaches<NetworkObject>.RetrieveList();
            foreach (var addressableName in addressablePackageGroup.value)
            {
                var handle = Addressables.LoadAssetsAsync<GameObject>(addressableName, addressable =>
                {
                    var nob = addressable.GetComponent<NetworkObject>();
                    if (nob != null) cache.Add(nob);
                });
                yield return handle;
            }
            spawnables.AddObjects(cache);
            CollectionCaches<NetworkObject>.Store(cache);
        }
        _addressablesAreLoaded = true;
        ao.allowSceneActivation = true;
    }

    /// <summary>
    /// Loads an addressables package by string.
    /// </summary>
    public void UnloadAddressables(List<AddressablePackage> addressablesPackage)
    {
        foreach (var addressablePackageGroup in addressablesPackage)
        {
            ushort id = addressablePackageGroup.key.GetStableHashU16();
            var spawnablePrefabs = (SinglePrefabObjects)_networkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
            spawnablePrefabs.Clear();
            Addressables.Release(_asyncHandle);
        }
    }

    /// <summary>
    /// Begin loading a scene using an async method.
    /// </summary>
    /// <param name="sceneName">Scene name to load.</param>
    public override void BeginLoadAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneParameters parameters)
    {
        AsyncOperation ao = UnitySceneManager.LoadSceneAsync(sceneName, parameters);
        LoadingAsyncOperations.Add(ao);
        CurrentAsyncOperation = ao;
        CurrentAsyncOperation.allowSceneActivation = false;
        StartCoroutine(LoadAddressables(ao));
    }

    /// <summary>
    /// Begin unloading a scene using an async method.
    /// </summary>
    /// <param name="sceneName">Scene name to unload.</param>
    public override void BeginUnloadAsync(UnityScene scene)
    {
        CurrentAsyncOperation = UnitySceneManager.UnloadSceneAsync(scene);
        UnloadAddressables(_addressablePackages);
    }

    /// <summary>
    /// Returns if a scene load or unload percent is done.
    /// </summary>
    /// <returns></returns>
    public override bool IsPercentComplete()
    {
        return (GetPercentComplete() >= 0.9f);
    }

    /// <summary>
    /// Returns the progress on the current scene load or unload.
    /// </summary>
    /// <returns></returns>
    public override float GetPercentComplete()
    {
        return (CurrentAsyncOperation == null) ? 1f : CurrentAsyncOperation.progress;
    }

    /// <summary>
    /// Returns if all asynchronized tasks are considered IsDone.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator AsyncsIsDone()
    {
        bool notDone;
        do
        {
            notDone = false;
            foreach (AsyncOperation ao in LoadingAsyncOperations)
            {

                if (!ao.isDone)
                {
                    notDone = true;
                    break;
                }
            }
            yield return null;
        } while (notDone);

        yield break;
    }
}