using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckField : Field
{
    public override int Action(int playerIndex)
    {
        return 2;
    }
}
