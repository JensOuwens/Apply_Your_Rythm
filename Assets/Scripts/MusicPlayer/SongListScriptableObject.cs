using UnityEngine;

[CreateAssetMenu(
    fileName = "SongList",
    menuName = "Audio/Song List",
    order = 1)]
public class SongListScriptableObject : ScriptableObject
{
    public SongProperties[] songs;
}
