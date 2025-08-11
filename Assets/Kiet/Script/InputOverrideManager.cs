using UnityEngine;
using System.Collections.Generic;

public static class InputOverrideManager
{
    private static Dictionary<string, KeyCode> keyOverrides = new Dictionary<string, KeyCode>();

    public static KeyCode GetKeyCode(string actionName, KeyCode defaultKey)
    {
        if (!keyOverrides.ContainsKey(actionName))
        {
            string saved = PlayerPrefs.GetString("keybind_" + actionName, defaultKey.ToString());
            keyOverrides[actionName] = (KeyCode)System.Enum.Parse(typeof(KeyCode), saved);
        }

        return keyOverrides[actionName];
    }

    public static void SetKey(string actionName, KeyCode newKey)
    {
        keyOverrides[actionName] = newKey;
        PlayerPrefs.SetString("keybind_" + actionName, newKey.ToString());
    }

    public static bool GetKey(string actionName, KeyCode defaultKey)
    {
        return Input.GetKey(GetKeyCode(actionName, defaultKey));
    }

    public static bool GetKeyDown(string actionName, KeyCode defaultKey)
    {
        return Input.GetKeyDown(GetKeyCode(actionName, defaultKey));
    }

    public static bool GetKeyUp(string actionName, KeyCode defaultKey)
    {
        return Input.GetKeyUp(GetKeyCode(actionName, defaultKey));
    }

    public static void ResetKey(string actionName, KeyCode defaultKey)
    {
        keyOverrides[actionName] = defaultKey;
        PlayerPrefs.DeleteKey("keybind_" + actionName);
    }
}
