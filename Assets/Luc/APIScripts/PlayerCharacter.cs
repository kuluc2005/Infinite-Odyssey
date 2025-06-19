using System;

[Serializable]
public class PlayerCharacter
{
    public int id;
    public int playerId;
    public string characterClass;
    public string createdAt;
}

[Serializable]
public class CharacterListWrapper
{
    public bool status;
    public PlayerCharacter[] data;
}
