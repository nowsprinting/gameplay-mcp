// Copyright (c) 2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace GameplayMcp.Internals
{
    /// <summary>
    /// Serializes a <see cref="GameObject"/> to a JSON string.
    /// </summary>
    internal static class GameObjectSerializer
    {
        /// <summary>
        /// Serializes the given <paramref name="gameObject"/> to a JSON string.
        /// Includes name, hierarchy path, and all public properties and fields of each Component.
        /// </summary>
        /// <param name="gameObject">The target GameObject.</param>
        /// <returns>JSON string representing the GameObject.</returns>
        internal static string Serialize(GameObject gameObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
