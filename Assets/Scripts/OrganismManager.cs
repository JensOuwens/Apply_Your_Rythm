using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class OrganismManager : MonoBehaviour
{
    [SerializeField]
    private SplineContainer spline;
    [SerializeField]
    private Vector2Int[] positions;
    [SerializeField]
    private int[] unUsedIndices;
    [SerializeField] 
    private List<GooberCatchAnim> catchAnims = new();
    private GooberCatchAnim[] usedAnims;
    [SerializeField] 
    private List<AnimateAlongSplineOnBeat> animSplines = new();
    private AnimateAlongSplineOnBeat[] usedAnimSpline;

    private void OnValidate()
    {
        if (spline == null)
            return;
        
        var tempList = new List<Vector2Int>();
        for (var i = 0; i < spline.Splines[0].Count; i++)
        {
            if (unUsedIndices.Contains(i))
                continue;
            tempList.Add(new Vector2Int((int)spline.Splines[0][i].Position.x, (int)spline.Splines[0][i].Position.y));
        }
        
        positions = tempList.ToArray();
        catchAnims = GetComponentsInChildren<GooberCatchAnim>().ToList();
        animSplines = GetComponentsInChildren<AnimateAlongSplineOnBeat>().ToList();
    }

    public void TriggerAnim(int playerId) => usedAnims[playerId].GooberAnim();
    public void TriggerAnimHold(int playerId, BeatData beat, Metronome metronome)
    {
        var holdDurationSec = (beat.beatEnd - beat.beatStart + 1) * (metronome.beatDurationInMS / 1000f);
        usedAnims[playerId].GooberHoldAnim(holdDurationSec);

        foreach (var anim in animSplines) 
            anim.SetTempDisabled(holdDurationSec);
    }

    private void FixedUpdate()
    {
        usedAnims = new GooberCatchAnim[positions.Length];
        usedAnimSpline = new AnimateAlongSplineOnBeat[positions.Length];
        for (var i = 0; i < positions.Length; i++)
        {
            var foundIndex = -1;
            var dist = 999f;
            for (var j = 0; j < catchAnims.Count; j++)
            {
                var catchAnim = catchAnims[j];
                var newDist = Vector2.Distance(positions[i], catchAnim.transform.position);
                if (!(newDist < dist)) continue;
                dist = newDist;
                foundIndex = j;
            }

            var foundCatchAnim = catchAnims[foundIndex];
            if (foundCatchAnim)
                usedAnims[i] = foundCatchAnim;
            var foundSplineAnim = animSplines[foundIndex];
            if (foundSplineAnim)
                usedAnimSpline[i] = foundSplineAnim;
        }
    }
}
