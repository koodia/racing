using System;
using UnityEngine;

public class ItemCreator
{
    private Array values = Enum.GetValues(typeof(ItemPrefab));
    public ItemPrefab PickRandomItem()
    {
        return (ItemPrefab)values.GetValue(My.rand.Next(values.Length));
    }

}
