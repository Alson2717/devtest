using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
[Flags]
public enum BasketSpawnPosition
{
    None = 0,
    Left = 1,
    Middle = 2,
    Right = 4
}


[System.Serializable]
public class BasketSpawnPositionBSWorkaround
{
    [SerializeField]
    public bool Left = false;
    [SerializeField]
    public bool Middle = false;
    [SerializeField]
    public bool Right = false;

    public BasketSpawnPosition Combine()
    {
        BasketSpawnPosition p = BasketSpawnPosition.None;
        if (Left)
            p |= BasketSpawnPosition.Left;
        if (Middle)
            p |= BasketSpawnPosition.Middle;
        if (Right)
            p |= BasketSpawnPosition.Right;
        return p;
    }
}

