using System.Collections.Generic;
using UnityEngine;

public class DefaultInputMapping : MonoBehaviour
{
    public static Dictionary<string, KeyCode> defaultKeys = new Dictionary<string, KeyCode>()
    {
        { "Jump", KeyCode.Space },
        { "Sprint", KeyCode.LeftShift },
        { "Attack", KeyCode.Mouse0 },
        { "Block", KeyCode.Mouse1 },
        { "Roll", KeyCode.LeftControl },
        { "Interact", KeyCode.E },
        { "Inventory", KeyCode.I }
    };
}
