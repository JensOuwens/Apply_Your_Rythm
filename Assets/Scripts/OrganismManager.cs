using System;
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
    }

    public void TriggerAnim(int playerId) => usedAnims[playerId].GooberAnim();
    public void TriggerAnimHold(int playerId, BeatData beat, Metronome metronome)
    {
        var holdDurationSec = (beat.beatEnd - beat.beatStart + 1) * (metronome.beatDurationInMS / 1000f);
        usedAnims[playerId].GooberHoldAnim(holdDurationSec);
    }

    private void FixedUpdate()
    {
        usedAnims = new GooberCatchAnim[positions.Length];
        for (var i = 0; i < positions.Length; i++)
        {
            var animCandidate = catchAnims[0];
            var dist = 999f;
            foreach (var catchAnim in catchAnims)
            {
                var newDist = Vector2.Distance(positions[i], catchAnim.transform.position);
                if (!(newDist < dist)) continue;
                dist = newDist;
                animCandidate = catchAnim;
            }

            if (animCandidate)
                usedAnims[i] = animCandidate;
        }
    }
}
