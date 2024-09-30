using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileTrailSettings", menuName = "Scriptable Objects/ProjectileTrailSettings")]
public class ProjectileTrailSettings : ScriptableObject
{
    [SerializeField] public float trailDuration;
    [SerializeField] public float trailMaxLength;
    [SerializeField] public float trailStartWidth;
    [SerializeField] public float trailEndWidth;
    [SerializeField] public Color trailStartColor = Color.white;
    [SerializeField] public Color trailEndColor = Color.white;
}