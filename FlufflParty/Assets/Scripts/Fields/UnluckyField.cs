using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnluckyField : Field
{
    public override int Action(int playerIndex)
    {
        return 4;
    }
}
