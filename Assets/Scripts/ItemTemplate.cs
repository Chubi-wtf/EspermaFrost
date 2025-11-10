using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class ItemTemplate : ScriptableObject
{
    public string itemName;
    public Color itemColor;
    public enum ITEM_TYPE
    {
        None,
        Key,
        Can,
        Botiquin,
        Adrenalina
    }
    public ITEM_TYPE itemType;

    [Header("Datos de Consumible")]
    public float useDuration;
    public float healAmount;
}