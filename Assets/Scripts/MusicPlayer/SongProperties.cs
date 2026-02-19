using System;
using UnityEngine;

[Serializable]
public class SongProperties
{
    public AudioClip clip;
    public string displayName;
    public int bpm;
    public float volume = 1f;
    public uint songId;
}
