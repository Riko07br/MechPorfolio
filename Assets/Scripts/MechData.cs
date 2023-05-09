using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mech", menuName = "Mech/New Mech", order = 1)]
public class MechData : ScriptableObject
{
    [SerializeField] string mechName;
    [SerializeField] GameObject mechModel, mechPrefab;
    [SerializeField, TextArea(5, 20)] string mechDescription;



}
