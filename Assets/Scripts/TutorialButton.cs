using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] private List<Composer> composers = new();
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Metronome metronome;
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private void OnValidate() => spriteRenderer ??= GetComponent<SpriteRenderer>();
    
    public void SubscribeComposer(Composer composer) => composers.Add(composer);

    private void OnEnable()
    {
        metronome.OnBeat.AddListener(OnBeat);
        ComposerCreator.ComposerSubscribed += SubscribeComposer;
    }

    private void OnDisable()
    {
        metronome.OnBeat.RemoveListener(OnBeat);
        ComposerCreator.ComposerSubscribed -= SubscribeComposer;
    }

    private void OnBeat()
    {
        BeatData beat = composers[0].GetBeat(metronome.GetNearestBeat(musicPlayer.GetSongPositionInMS()));
        spriteRenderer.sprite = beat != null ? onSprite : offSprite;
    }
}
