using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockEntry data;

    public void Initialize(BlockEntry blockData)
    {
        data = blockData;
        tag = data.type == 0 ? "Collectible" : "Obstacle";
    }
}
