using System;
using UnityEngine;
namespace Asadito
{
    [Serializable] public sealed class ManagementArtEntry { public string Name; public Sprite Sprite; }
    public sealed class ManagementArtLibrary : ScriptableObject
    {
        public ManagementArtEntry[] Entries;
        public Sprite Get(string name)
        {
            foreach (var entry in Entries) if (entry.Name == name) return entry.Sprite;
            return null;
        }
    }
}
