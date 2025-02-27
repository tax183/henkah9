using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public static int FIELD_UNSELECTED = -1;
    public static int FLYING_PAWNS_NUMBER = 3;
    public PlayerNumber PlayerNumber { get; private set; }
    public int PawnsToSet { get; private set; }

    public PlayerData(PlayerNumber playerNumber, int pawnsToSet)
    {
        this.PlayerNumber = playerNumber;
        PawnsToSet = pawnsToSet;
    }

    public void SetPawn()
    {
        if(PawnsToSet > 0)
        {
            PawnsToSet--;
        }
    }

    public bool HasPawnsToSet()
    {
        return PawnsToSet != 0;
    }
}
