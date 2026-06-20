using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunWindowResourceLoader : IUIResourceLoader
    {
        private readonly IUIResourceLoader _fallback;

        public CultivationRunWindowResourceLoader(IUIResourceLoader fallback)
        {
            _fallback = fallback;
        }

        public GameObject LoadGameObject(string location, Transform parent = null, string packageName = "")
        {
            if (location == CultivationRunWindow.AssetLocation)
            {
                return CultivationRunWindowPrefabFactory.CreatePanel(parent);
            }

            if (_fallback != null)
            {
                return _fallback.LoadGameObject(location, parent, packageName);
            }

            throw new InvalidOperationException($"No UI resource loader is available for {location}.");
        }

        public async UniTask<GameObject> LoadGameObjectAsync(
            string location,
            Transform parent = null,
            CancellationToken cancellationToken = default,
            string packageName = "")
        {
            if (location == CultivationRunWindow.AssetLocation)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(cancellationToken);
                return CultivationRunWindowPrefabFactory.CreatePanel(parent);
            }

            if (_fallback != null)
            {
                return await _fallback.LoadGameObjectAsync(location, parent, cancellationToken, packageName);
            }

            throw new InvalidOperationException($"No UI resource loader is available for {location}.");
        }
    }
}
