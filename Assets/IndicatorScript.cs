using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IndicatorScript : MonoBehaviour
{
    [SerializeField]
    public Color OffColor;
    [SerializeField]
    public Color PendingColor;
    [SerializeField]
    public Color OnColor;
    [SerializeField]
    public Material MaterialRef;

    void Start()
    {
        SetState(GGJ.CrankState.NotStarted);
    }

    public void SetState(GGJ.CrankState state)
    {
        switch (state)
        {
            case GGJ.CrankState.NotStarted:
                MaterialRef.color = OffColor;
                break;
            case GGJ.CrankState.Cranking:
                MaterialRef.color = PendingColor;
                break;
            case GGJ.CrankState.Finished:
                MaterialRef.color = OnColor;
                break;
        }
    }
}
