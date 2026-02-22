using UnityEngine;

/// <summary>
/// Using the composer and metronome judges a player's beat position, and keeps track of these judgments
/// </summary>
public class Judge : MonoBehaviour
{
    public Composer composer;
    public Metronome metronome;
    [SerializeField]
    private float errorMargin = 0.8f;
    [Space]
    [SerializeField]
    private int correctInputs = 0;
    [SerializeField]
    private int incorrectInputs = 0;

    public void CheckInput(float beatPos)
    {
        if (composer == null || metronome == null)
            return;
        
        var trueBeatPos = metronome.GetNearestBeat(beatPos);
        // is beat on time
        if (!(beatPos >= trueBeatPos - errorMargin) || !(trueBeatPos <= trueBeatPos + errorMargin))
        {
            incorrectInputs++;
            return;
        }
        
        var foundBeat = composer.GetBeat(trueBeatPos);
        // did player hit a non-existing beat
        if (foundBeat == null)
        {
            incorrectInputs++;
            return;
        }

        // did player hit a beat that's still hittable
        if (!foundBeat.hit)
        {
            foundBeat.hit = true;
            correctInputs++;
        }
    }
}