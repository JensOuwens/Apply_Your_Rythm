using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Composer))]
public class OrganismManager : MonoBehaviour
{
    [SerializeField]
    private Metronome metronome;
    [SerializeField]
    private MusicPlayer musicPlayer;
    [SerializeField]
    private Composer composer;
    [SerializeField]
    private SplineContainer spline;
    [SerializeField] 
    private OrganismAnim[] catchAnims;
    [SerializeField]
    private Bucket[] buckets;

    public Action onMoveBucket;

    private void OnValidate()
    {
        if (spline == null)
            return;
        catchAnims = GetComponentsInChildren<OrganismAnim>();
        buckets = GetComponentsInChildren<Bucket>();
        composer ??= GetComponent<Composer>();
        enabled = metronome && composer && musicPlayer;
    }

    private void Start()
    {
        metronome.OnBeat.AddListener(OnBeat);
        foreach (var bucketAnim in buckets) 
            bucketAnim.Initialize(metronome, this);
    }

    private void OnBeat()
    {
        var beat = composer.GetBeat(metronome.GetNearestBeat(musicPlayer.currentSongPositionInMS));
        if (beat != null)
            onMoveBucket.Invoke();
    }
    
    private Bucket GetBucketFromPlayerId(int playerId) => buckets.First(a => a.CurrentState == playerId + 1); // + 1 to align playerId to spline pos

    public void TriggerAnim(int playerId, float animLength)
    {
        catchAnims[playerId].GooberAnim(animLength);
        var bucket = GetBucketFromPlayerId(playerId);
        bucket.FollowOrganism(catchAnims[playerId], animLength);
        bucket.Fill();
    }

    public void TriggerAnimHold(int playerId, BeatData beat)
    {
        var holdDurationSec = (beat.beatEnd - beat.beatStart + 1) * (metronome.beatDurationInMS / 1000f);
        catchAnims[playerId].GooberHoldAnim(holdDurationSec);
        var bucket = GetBucketFromPlayerId(playerId);
        bucket.Fill();
    }

    public void InitOrganisms()
    {
        foreach (var organismAnim in catchAnims)
        {
            organismAnim.SetIdleSpeed(metronome.beatDurationInMS / 1000f);
        }
    }
}
