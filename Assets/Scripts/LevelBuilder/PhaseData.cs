// --- CREATE NEW FILE: PhaseData.cs ---

using UnityEngine;

[CreateAssetMenu(fileName = "NewPhaseData", menuName = "Hill Pursuit/Phase Data")]
public class PhaseData : ScriptableObject
{
    [Tooltip("The name of the phase, e.g., 'Alpine Ridge' or 'Dusty Canyons'.")]
    public string phaseName = "New Phase";

    [Tooltip("The background image to display for this phase.")]
    public Sprite backgroundImage;
}