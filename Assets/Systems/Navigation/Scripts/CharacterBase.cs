using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] EFaction _Faction;

    public EFaction Faction => _Faction;
}
